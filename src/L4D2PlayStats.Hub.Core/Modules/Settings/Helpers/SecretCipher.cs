using System.Security.Cryptography;
using System.Text;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

public static class SecretCipher
{
    public static bool IsEncrypted(string? value)
    {
        return value != null && value.StartsWith(AppConsts.SecretPrefix, StringComparison.Ordinal);
    }

    public static string Encrypt(byte[] key, string plain)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plain);
        var payload = new byte[AppConsts.SecretNonceBytes + plainBytes.Length + AppConsts.SecretTagBytes];
        var nonce = payload.AsSpan(0, AppConsts.SecretNonceBytes);
        var cipher = payload.AsSpan(AppConsts.SecretNonceBytes, plainBytes.Length);
        var tag = payload.AsSpan(AppConsts.SecretNonceBytes + plainBytes.Length);

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(key, AppConsts.SecretTagBytes);

        aes.Encrypt(nonce, plainBytes, cipher, tag);

        return AppConsts.SecretPrefix + Convert.ToBase64String(payload);
    }

    public static bool TryDecrypt(byte[] key, string value, out string plain)
    {
        plain = string.Empty;

        if (!IsEncrypted(value))
            return false;

        var payload = Decode(value[AppConsts.SecretPrefix.Length..]);

        if (payload == null || payload.Length < AppConsts.SecretNonceBytes + AppConsts.SecretTagBytes)
            return false;

        var cipherLength = payload.Length - AppConsts.SecretNonceBytes - AppConsts.SecretTagBytes;
        var output = new byte[cipherLength];

        try
        {
            using var aes = new AesGcm(key, AppConsts.SecretTagBytes);

            aes.Decrypt(payload.AsSpan(0, AppConsts.SecretNonceBytes), payload.AsSpan(AppConsts.SecretNonceBytes, cipherLength), payload.AsSpan(AppConsts.SecretNonceBytes + cipherLength), output);
        }
        catch (CryptographicException)
        {
            return false;
        }

        plain = Encoding.UTF8.GetString(output);

        return true;
    }

    private static byte[]? Decode(string base64)
    {
        try
        {
            return Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}