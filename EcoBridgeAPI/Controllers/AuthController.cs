using EcoBridge.Data;
using EcoBridge.Domains.Models;
using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Result;
using EcoBridgeAPI.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace EcoBridgeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("IpRateLimit")]
public class AuthController(
    EcoBridgeDbContext dbContext,
    ITokenService tokenService,
    IOptions<JWTSettings> jwtOptions) : ControllerBase
{
    private readonly PasswordHasher<Account> _passwordHasher = new();
    private readonly JWTSettings _jwtSettings = jwtOptions.Value;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Accounts.AnyAsync(x => x.Email == email, ct);
        if (exists)
        {
            return BadRequest(Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Email already registered."));
        }

        var account = new Account
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow
        };
        account.PasswordHash = _passwordHasher.HashPassword(account, request.Password);

        await dbContext.Accounts.AddAsync(account, ct);
        await dbContext.SaveChangesAsync(ct);

        var response = await BuildAuthResponse(account, ct);
        return Ok(Result<AuthResponseDTO>.Success(response, "Registered successfully."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (account is null)
        {
            return Unauthorized(Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid credentials."));
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid credentials."));
        }

        var response = await BuildAuthResponse(account, ct);
        return Ok(Result<AuthResponseDTO>.Success(response, "Logged in successfully."));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request, CancellationToken ct)
    {
        try
        {
            var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid token payload."));
            }

            var dbRefreshToken = await dbContext.RefreshTokens
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, ct);
            if (dbRefreshToken is null ||
                dbRefreshToken.AccountId != userId ||
                !dbRefreshToken.IsActive)
            {
                return Unauthorized(Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid refresh token."));
            }

            dbRefreshToken.RevokedAtUtc = DateTime.UtcNow;

            var response = await BuildAuthResponse(dbRefreshToken.Account, ct);
            dbRefreshToken.ReplacedByToken = response.RefreshToken;

            return Ok(Result<AuthResponseDTO>.Success(response, "Token refreshed successfully."));
        }
        catch
        {
            return Unauthorized(Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid token."));
        }
    }

    private async Task<AuthResponseDTO> BuildAuthResponse(Account account, CancellationToken ct)
    {
        var accessToken = await tokenService.GenerateAccessToken(account, ct);
        var refreshToken = tokenService.GenerateRefreshToken();
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);
        var refreshExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays);
        await dbContext.RefreshTokens.AddAsync(new RefreshToken
        {
            Token = refreshToken,
            AccountId = account.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshExpiry
        }, ct);
        await dbContext.SaveChangesAsync(ct);

        return new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAt
        };
    }
}
