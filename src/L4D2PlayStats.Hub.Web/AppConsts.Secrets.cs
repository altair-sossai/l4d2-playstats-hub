namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string SecretsKeySetting = "Hub:SecretsKey";
    public const string SecretsKeyFileSetting = "Hub:SecretsKeyFile";
    public const string SecretsKeySettingName = "Hub__SecretsKey";
    public const string DefaultSecretsKeyFile = "secrets.key";

    public const string SecretsKeyInvalidMessage = "It must hold 32 random bytes written in base64, such as the output of: openssl rand -base64 32";
    public const string SecretsKeyUnreadableMessage = "It could not be read or created: {0}";
}
