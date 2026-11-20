using System.Security.Cryptography;
using System.Text;

namespace L4D2PlayStats.Hub.Core.Infrastructure;

public static class SecretGenerator
{
    public static string NewSecret(int bytes)
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(bytes)).ToLowerInvariant();
    }

    public static string Hash(string secret)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
    }

    public static bool Matches(string secret, string hash)
    {
        var computed = Encoding.UTF8.GetBytes(Hash(secret));
        var expected = Encoding.UTF8.GetBytes(hash);

        return CryptographicOperations.FixedTimeEquals(computed, expected);
    }
}
