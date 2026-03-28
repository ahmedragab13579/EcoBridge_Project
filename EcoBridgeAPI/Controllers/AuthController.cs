using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Result;
using EcoBridgeAPI.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoBridgeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("IpRateLimit")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        if (result._success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        if (result._success)
            return Ok(result);
        return Unauthorized(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request, CancellationToken ct)
    {
        var result = await authService.RefreshAsync(request, ct);
        if (result._success)
            return Ok(result);
        return Unauthorized(result);
    }
}
