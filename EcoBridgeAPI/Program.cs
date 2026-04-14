using EcoBridge.Data;
using EcoBridgeAPI.Services.Auth;
using EcoBridgeAPI.Services.Donation;
using EcoBridgeAPI.Services.Photo;
using EcoBridgeAPI.Services.Statistics;
using EcoBridgeAPI.Services.Volunteer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Services
// =========================

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage != string.Empty ? x.ErrorMessage : x.Exception?.Message)
            .ToList();

        var errorMessage = string.Join(" | ", errors);

        // تبسيط رسالة الخطأ لو المشكلة في الـ RoleId
        if (errorMessage.Contains("converted to EcoBridge.Domains.Enums.UserRole"))
        {
            errorMessage = "Invalid Role ID. Please provide a valid role number (2 for Donor, 3 for Charity, 4 for Volunteer).";
        }

        // ترجيع الإيرور بنفس شكل الـ Result بتاع المشروع
        var result = EcoBridgeAPI.Result.Result<object>.FailResult(null!, errorMessage);

        return new BadRequestObjectResult(result);
    };
});

builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddDbContext<EcoBridgeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// Validate Cloudinary configuration and log guidance
var cloudinarySettings = builder.Configuration.GetSection("Cloudinary").Get<CloudinarySettings>();
if (cloudinarySettings == null ||
    string.IsNullOrWhiteSpace(cloudinarySettings.CloudName) ||
    string.IsNullOrWhiteSpace(cloudinarySettings.ApiKey) ||
    string.IsNullOrWhiteSpace(cloudinarySettings.ApiSecret))
{
    Console.WriteLine("Cloudinary is not fully configured. Photo uploads will be disabled. To enable, set Cloudinary:CloudName, Cloudinary:ApiKey and Cloudinary:ApiSecret in configuration.");
}

// =========================
// Swagger & OpenAPI
// =========================
builder.Services.AddEndpointsApiExplorer();

// 1. تسجيل الـ OpenAPI Document باسم "v1" (ضروري عشان MapOpenApi تلاقيها)
builder.Services.AddOpenApi("v1");

// 2. إبقاء SwaggerGen لو محتاج تستخدم Swagger UI التقليدي
builder.Services.AddSwaggerGen();

// =========================
// JWT Authentication
// =========================
var jwtSettings = builder.Configuration.GetSection("JWT").Get<JWTSettings>();

// Basic runtime validation for JWT configuration to fail fast with clear message
if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) || string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException("JWT configuration is missing or incomplete. Please configure 'JWT' section in appsettings.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// =========================
// CORS
// =========================
// =========================
// CORS
// =========================
// 1. قراءة القيمة كنص عادي (String) بناءً على ملف appsettings.json
var allowedOrigin = builder.Configuration.GetValue<string>("AllowOrigins");

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        // 2. لو القيمة نجمة، افتحها للكل
        if (allowedOrigin == "*")
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        // 3. لو حطيت لينك محدد بعدين، هيقراه ويشغله
        else if (!string.IsNullOrEmpty(allowedOrigin))
        {
            policy.WithOrigins(allowedOrigin)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// =========================
// Rate Limiting
// =========================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"error": "Too many requests. Please check the Retry-After header."}""",
            token
        );
    };

    options.AddPolicy("IpRateLimit", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            }));

    options.AddPolicy("UserRateLimit", httpContext =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: userId,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 40,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            });
    });
});

// =========================
// Build App
// =========================
var app = builder.Build();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "EcoBridge API v1");
    options.RoutePrefix = "swagger";
});

//app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();
app.MapControllers();

app.Run();
