using System.Security.Claims;
using EcoBridge.Domains.Models;

namespace EcoBridgeAPI.Services.Auth;

public interface ITokenService
{
    Task<string> GenerateAccessToken(Account user, CancellationToken ct);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
