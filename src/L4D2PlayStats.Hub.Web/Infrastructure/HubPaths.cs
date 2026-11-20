namespace L4D2PlayStats.Hub.Web.Infrastructure;

public class HubPaths
{
    public HubPaths(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration[AppConsts.DataDirectorySetting];
        var directory = string.IsNullOrWhiteSpace(configured) ? AppConsts.DefaultDataDirectory : configured.Trim();

        DataDirectory = Path.IsPathRooted(directory) ? directory : Path.Combine(environment.ContentRootPath, directory);

        Directory.CreateDirectory(DataDirectory);

        var keyFile = configuration[AppConsts.SecretsKeyFileSetting];

        SecretsKeyFile = string.IsNullOrWhiteSpace(keyFile)
            ? Path.Combine(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(DataDirectory))) ?? environment.ContentRootPath, AppConsts.DefaultSecretsKeyFile)
            : Path.GetFullPath(keyFile.Trim(), environment.ContentRootPath);
    }

    public string DataDirectory { get; }

    public string SecretsKeyFile { get; }

    public string Avatars => Path.Combine(DataDirectory, AppConsts.AvatarFolder);
    public string Branding => Path.Combine(DataDirectory, AppConsts.BrandingFolder);

    public string Keys => Path.Combine(DataDirectory, AppConsts.KeysFolder);

    public string SetupCode => Path.Combine(DataDirectory, AppConsts.SetupCodeFile);
}