using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Result;

namespace EcoBridgeAPI.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO request, CancellationToken ct);
    Task<Result<AuthResponseDTO>> LoginAsync(LoginRequestDTO request, CancellationToken ct);
    Task<Result<AuthResponseDTO>> RefreshAsync(RefreshTokenRequestDTO request, CancellationToken ct);
}
