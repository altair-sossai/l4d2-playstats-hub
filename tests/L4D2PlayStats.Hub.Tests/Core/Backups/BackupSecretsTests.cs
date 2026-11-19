using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Tests.Core.Backups;

[TestClass]
public class BackupSecretsTests
{
    private const string Signature = "sv=2024-11-04&sp=racwdl&sig=c2VjcmV0LXNpZw%3D%3D";
    private const string ContainerUrl = "https://account.blob.core.windows.net/backups?" + Signature;
    private const string AccountKey = "c2VjcmV0LWFjY291bnQta2V5LXZhbHVlLWZvci10ZXN0cw==";
    private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=account;AccountKey=" + AccountKey + ";EndpointSuffix=core.windows.net";

    [TestMethod]
    public void TheContainerAddressIsShownWithoutItsToken()
    {
        var masked = BackupSecrets.MaskContainerUrl(ContainerUrl);

        Assert.AreEqual("https://account.blob.core.windows.net/backups?••••••••", masked);
        Assert.DoesNotContain("sig", masked!);
    }

    [TestMethod]
    public void TheConnectionStringIsShownWithoutItsKey()
    {
        var masked = BackupSecrets.MaskConnectionString(ConnectionString);

        Assert.AreEqual("DefaultEndpointsProtocol=https;AccountName=account;AccountKey=••••••••;EndpointSuffix=core.windows.net", masked);
        Assert.DoesNotContain(AccountKey, masked!);
    }

    [TestMethod]
    public void TheSasOfAConnectionStringIsMaskedToo()
    {
        var masked = BackupSecrets.MaskConnectionString("BlobEndpoint=https://account.blob.core.windows.net/;SharedAccessSignature=" + Signature);

        Assert.AreEqual("BlobEndpoint=https://account.blob.core.windows.net/;SharedAccessSignature=••••••••", masked);
    }

    [TestMethod]
    public void NothingSavedIsShownAsNothing()
    {
        Assert.IsNull(BackupSecrets.MaskContainerUrl(null));
        Assert.IsNull(BackupSecrets.MaskContainerUrl(" "));
        Assert.IsNull(BackupSecrets.MaskConnectionString(null));
        Assert.AreEqual("••••••••", BackupSecrets.MaskContainerUrl("not an address"));
    }

    [TestMethod]
    public void TheMaskFollowsTheChosenDestination()
    {
        var settings = new BackupSettings { ContainerUrl = ContainerUrl, ConnectionString = ConnectionString };

        Assert.AreEqual(BackupSecrets.MaskContainerUrl(ContainerUrl), BackupSecrets.Mask(settings));

        settings.Destination = BackupDestination.ConnectionString;

        Assert.AreEqual(BackupSecrets.MaskConnectionString(ConnectionString), BackupSecrets.Mask(settings));
    }

    [TestMethod]
    public void AnErrorMessageNeverCarriesTheSecrets()
    {
        var settings = new BackupSettings { ContainerUrl = ContainerUrl, ConnectionString = ConnectionString };
        var message = $"Failed {ContainerUrl} then {Signature} and {ConnectionString} and the key {AccountKey}.";

        var redacted = BackupSecrets.Redact(message, settings);

        Assert.DoesNotContain(Signature, redacted);
        Assert.DoesNotContain(AccountKey, redacted);
        Assert.DoesNotContain("sig=", redacted);
        Assert.Contains("••••••••", redacted);
    }

    [TestMethod]
    public void AMessageWithoutSecretsIsKept()
    {
        var settings = new BackupSettings { ContainerUrl = ContainerUrl };

        Assert.AreEqual("The specified container does not exist.", BackupSecrets.Redact("The specified container does not exist.", settings));
        Assert.AreEqual("Nothing set.", BackupSecrets.Redact("Nothing set.", new BackupSettings()));
    }
}
