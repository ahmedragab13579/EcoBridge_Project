using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcoBridge.Data;
using EcoBridge.Domains.Enums;
using EcoBridge.Domains.Models;
using EcoBridgeAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcoBridgeAPI.Services.Auth;

public class AuthService(
    EcoBridgeDbContext dbContext,
    ITokenService tokenService,
    IOptions<JWTSettings> jwtOptions) : IAuthService
{
    private readonly PasswordHasher<Account> _passwordHasher = new();
    private readonly JWTSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result.Result<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO request, CancellationToken ct)
    {
        if (request.RoleId == UserRole.Admin)
        {
            return Result.Result<AuthResponseDTO>.Fail(
                new AuthResponseDTO(),
                "Registration with the Admin role is not allowed.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Accounts.AnyAsync(x => x.Email == email, ct);
        if (exists)
        {
            return Result.Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Email already registered.");
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

        var response = await IssueNewSessionAsync(account, ct);
        return Result.Result<AuthResponseDTO>.Success(response, "Registered successfully.");
    }

    public async Task<Result.Result<AuthResponseDTO>> LoginAsync(LoginRequestDTO request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (account is null)
        {
            return Result.Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid credentials.");
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Result.Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid credentials.");
        }

        if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            account.PasswordHash = _passwordHasher.HashPassword(account, request.Password);
            await dbContext.SaveChangesAsync(ct);
        }

        var response = await IssueNewSessionAsync(account, ct);
        return Result.Result<AuthResponseDTO>.Success(response, "Logged in successfully.");
    }

    public async Task<Result.Result<AuthResponseDTO>> RefreshAsync(RefreshTokenRequestDTO request, CancellationToken ct)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch (SecurityTokenException)
        {
            return Result.Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid token.");
        }

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Result.Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid token payload.");
        }

        var dbRefreshToken = await dbContext.RefreshTokens
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, ct);

        if (dbRefreshToken is null ||
            dbRefreshToken.AccountId != userId ||
            !dbRefreshToken.IsActive)
        {
            return Result.Result<AuthResponseDTO>.Fail(new AuthResponseDTO(), "Invalid refresh token.");
        }

        dbRefreshToken.RevokedAtUtc = DateTime.UtcNow;

        var response = await IssueRotatedSessionAsync(dbRefreshToken.Account, dbRefreshToken, ct);
        return Result.Result<AuthResponseDTO>.Success(response, "Token refreshed successfully.");
    }

    private async Task<AuthResponseDTO> IssueNewSessionAsync(Account account, CancellationToken ct)
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

    /// <summary>
    /// Revokes the previous refresh token, links it to the new token, and persists both in one save.
    /// </summary>
    private async Task<AuthResponseDTO> IssueRotatedSessionAsync(
        Account account,
        RefreshToken previousRefreshToken,
        CancellationToken ct)
    {
        var accessToken = await tokenService.GenerateAccessToken(account, ct);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);
        var refreshExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays);

        previousRefreshToken.ReplacedByToken = newRefreshTokenValue;

        await dbContext.RefreshTokens.AddAsync(new RefreshToken
        {
            Token = newRefreshTokenValue,
            AccountId = account.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshExpiry
        }, ct);

        await dbContext.SaveChangesAsync(ct);

        return new AuthResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpiresAtUtc = accessTokenExpiresAt
        };
    }
}
