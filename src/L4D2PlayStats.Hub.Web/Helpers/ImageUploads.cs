using L4D2PlayStats.Hub.Web.Models;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ImageUploads
{
    public static bool IsSupported(string? fileName)
    {
        return ContentType(fileName) != null;
    }

    public static string? ContentType(string? fileName)
    {
        return string.IsNullOrEmpty(fileName) ? null : AppConsts.LogoContentTypes.GetValueOrDefault(Path.GetExtension(fileName).ToLowerInvariant());
    }

    public static async Task<ImageUploadError?> CheckAsync(IFormFile file, long maxBytes, CancellationToken cancellationToken)
    {
        if (!IsSupported(file.FileName))
            return ImageUploadError.Unsupported;

        if (file.Length > maxBytes)
            return ImageUploadError.TooLarge;

        await using var content = file.OpenReadStream();

        var header = new byte[AppConsts.ImageSignatureLength];
        var read = await content.ReadAtLeastAsync(header, header.Length, false, cancellationToken);

        return ImageSignature.IsImage(header.AsSpan(0, read)) ? null : ImageUploadError.Unsupported;
    }
}
