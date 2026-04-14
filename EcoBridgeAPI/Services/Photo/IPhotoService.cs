using Microsoft.AspNetCore.Http;
using EcoBridgeAPI.Result;

namespace EcoBridgeAPI.Services.Photo;

public interface IPhotoService
{
    Task<Result<string?>> UploadImageAsync(IFormFile file, CancellationToken ct = default);
    bool IsEnabled { get; }
}
