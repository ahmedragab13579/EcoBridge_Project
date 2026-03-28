using EcoBridgeAPI.DTO;

namespace EcoBridgeAPI.Services.Auth;

public interface IAuthService
{
    Task<Result.Result<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO request, CancellationToken ct);
    Task<Result.Result<AuthResponseDTO>> LoginAsync(LoginRequestDTO request, CancellationToken ct);
    Task<Result.Result<AuthResponseDTO>> RefreshAsync(RefreshTokenRequestDTO request, CancellationToken ct);
}
