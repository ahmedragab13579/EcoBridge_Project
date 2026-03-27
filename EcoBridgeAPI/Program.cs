using EcoBridge.Data;
using EcoBridgeAPI.Services.Donation;
using EcoBridgeAPI.Services.Statistics;
using EcoBridgeAPI.Services.Volunteer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IDonationServices, DonationServices>();
builder.Services.AddScoped<IVolunteerServices, VolunteerServices>();
builder.Services.AddDbContext<EcoBridgeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
