using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using EcoBridgeAPI.Result;

namespace EcoBridgeAPI.Services.Photo;

public class PhotoService : IPhotoService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private readonly Cloudinary? _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> cloudinaryOptions)
    {
        var settings = cloudinaryOptions?.Value;
        if (settings is not null &&
            !string.IsNullOrWhiteSpace(settings.CloudName) &&
            !string.IsNullOrWhiteSpace(settings.ApiKey) &&
            !string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
    }

    public bool IsEnabled => _cloudinary != null;

    public async Task<Result<string?>> UploadImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (!IsEnabled)
            return Result<string?>.FailResult(null, "Photo uploads are not configured.");

        if (file == null)
            return Result<string?>.FailResult(null, "No file provided.");

        if (file.Length == 0)
            return Result<string?>.FailResult(null, "File is empty.");

        if (file.Length > MaxFileSizeBytes)
            return Result<string?>.FailResult(null, $"File size exceeds the maximum allowed size of {MaxFileSizeBytes} bytes.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return Result<string?>.FailResult(null, "File type is not supported. Allowed: .jpg, .jpeg, .png, .webp");

        try
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "ecobridge/donations"
            };

            var uploadResult = await _cloudinary!.UploadAsync(uploadParams, ct);
            if (uploadResult.Error is not null || uploadResult.SecureUrl is null)
                return Result<string?>.FailResult(null, uploadResult.Error?.Message ?? "Unknown upload error");

            return Result<string?>.SuccessResult(uploadResult.SecureUrl.ToString(), "Upload successful");
        }
        catch (OperationCanceledException)
        {
            return Result<string?>.FailResult(null, "Image upload canceled.");
        }
        catch (Exception ex)
        {
            return Result<string?>.FailResult(null, $"Image upload failed: {ex.Message}");
        }
    }
}
