using System.Security.Cryptography;
using System.Text;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Secrets;

public static class SecretsKeyLoader
{
    public static SecretsKey Load(IConfiguration configuration, HubPaths paths)
    {
        var configured = configuration[AppConsts.SecretsKeySetting];

        if (!string.IsNullOrWhiteSpace(configured))
            return Decode(configured, AppConsts.SecretsKeySettingName, null);

        var file = paths.SecretsKeyFile;

        try
        {
            var created = File.Exists(file) ? null : TryCreate(file);

            return created != null
                ? new SecretsKey(created, file, file, true, null)
                : Decode(File.ReadAllText(file), file, file);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new SecretsKey(null, file, file, false, string.Format(AppConsts.SecretsKeyUnreadableMessage, exception.Message));
        }
    }

    public static bool IsKeyFile(string path, SecretsKey key)
    {
        if (key.File == null)
            return false;

        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return string.Equals(Path.GetFullPath(path), Path.GetFullPath(key.File), comparison);
    }

    private static SecretsKey Decode(string text, string location, string? file)
    {
        var value = Base64(text.Trim());

        return value is { Length: CoreConsts.SecretKeyBytes }
            ? new SecretsKey(value, location, file, false, null)
            : new SecretsKey(null, location, file, false, AppConsts.SecretsKeyInvalidMessage);
    }

    private static byte[]? Base64(string text)
    {
        try
        {
            return Convert.FromBase64String(text);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static byte[]? TryCreate(string file)
    {
        var directory = Path.GetDirectoryName(file);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var key = RandomNumberGenerator.GetBytes(CoreConsts.SecretKeyBytes);
        var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write, Share = FileShare.None };

        if (!OperatingSystem.IsWindows())
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;

        try
        {
            using var stream = new FileStream(file, options);

            stream.Write(Encoding.ASCII.GetBytes(Convert.ToBase64String(key)));
        }
        catch (IOException) when (File.Exists(file))
        {
            return null;
        }

        return key;
    }
}