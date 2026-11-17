namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ImageSignature
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Gif = "GIF8"u8.ToArray();
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    public static bool IsImage(ReadOnlySpan<byte> header)
    {
        return header.StartsWith(Png)
               || header.StartsWith(Jpeg)
               || header.StartsWith(Gif)
               || (header.StartsWith(Riff) && header.Length >= AppConsts.ImageSignatureLength && header[AppConsts.WebpSignatureOffset..].StartsWith(Webp));
    }
}
