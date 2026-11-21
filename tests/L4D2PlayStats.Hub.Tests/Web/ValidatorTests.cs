using System.Text.Json;
using FluentValidation;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Admin;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using L4D2PlayStats.Hub.Web.Models.Community;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Models.Skins;
using L4D2PlayStats.Hub.Web.Validators.Admin;
using L4D2PlayStats.Hub.Web.Validators.AntiCheat;
using L4D2PlayStats.Hub.Web.Validators.Community;
using L4D2PlayStats.Hub.Web.Validators.Live;
using L4D2PlayStats.Hub.Web.Validators.Players;
using L4D2PlayStats.Hub.Web.Validators.Skins;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class ValidatorTests
{
    private const string WebProject = "src/L4D2PlayStats.Hub.Web";

    [TestMethod]
    [DataRow("76561197960287930")]
    [DataRow("STEAM_0:0:11101")]
    [DataRow("STEAM_1:0:11101")]
    [DataRow("[U:1:22202]")]
    [DataRow("https://steamcommunity.com/profiles/76561197960287930")]
    public void EverySteamIdFormatIsAccepted(string communityId)
    {
        Assert.IsNull(Error(new ChatBanCommandValidator(), new ChatBanCommand { CommunityId = communityId }));
        Assert.IsNull(Error(new AdministratorFormValidator(), new AdministratorForm { CommunityId = communityId }));
        Assert.IsNull(Error(new PlayerFormValidator(), new PlayerForm { CommunityId = communityId }));
        Assert.IsNull(Error(new AntiCheatAccessPlayerCommandValidator(), new AntiCheatAccessPlayerCommand { Player = communityId }));
    }

    [TestMethod]
    [DataRow(null, WebConsts.SteamIdRequiredMessage)]
    [DataRow("   ", WebConsts.SteamIdRequiredMessage)]
    [DataRow("123", WebConsts.SteamIdInvalidMessage)]
    [DataRow("STEAM_X", WebConsts.SteamIdInvalidMessage)]
    public void TheApiCommandsTellAMissingSteamIdFromAnInvalidOne(string? communityId, string expected)
    {
        Assert.AreEqual(expected, Error(new ChatBanCommandValidator(), new ChatBanCommand { CommunityId = communityId }));
        Assert.AreEqual(expected, Error(new AntiCheatAccessPlayerCommandValidator(), new AntiCheatAccessPlayerCommand { Player = communityId }));
        Assert.AreEqual(expected, Error(new SkinOwnershipCommandValidator(), new SkinOwnershipCommand { CommunityId = communityId, SkinId = "skin" }));
        Assert.AreEqual(expected, Error(new PunishmentCommandValidator(), new PunishmentCommand { CommunityId = communityId, LostExperiencePoints = 10 }));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not an id")]
    public void TheAdministrationFormsKeepTheirSteamIdMessage(string? communityId)
    {
        Assert.AreEqual(WebConsts.InvalidSteamIdMessage, Error(new AdministratorFormValidator(), new AdministratorForm { CommunityId = communityId }));
        Assert.AreEqual(WebConsts.InvalidSteamIdMessage, Error(new PlayerFormValidator(), new PlayerForm { CommunityId = communityId }));
        Assert.AreEqual(WebConsts.InvalidSteamIdMessage, Error(new BlockVoterFormValidator(), new BlockVoterForm { CommunityId = communityId }));
        Assert.AreEqual(WebConsts.InvalidSteamIdMessage, Error(new ForcedNameFormValidator(), new ForcedNameForm { CommunityId = communityId, Name = "Ellis" }));
    }

    [TestMethod]
    public void AForcedNameNeedsAName()
    {
        Assert.AreEqual(WebConsts.ForcedNameInvalidNameMessage, Error(new ForcedNameFormValidator(), new ForcedNameForm { CommunityId = "76561197960287930", Name = "  " }));
        Assert.IsNull(Error(new ForcedNameFormValidator(), new ForcedNameForm { CommunityId = "76561197960287930", Name = "Ellis" }));
    }

    [TestMethod]
    public void TheFormsWithANameNeedIt()
    {
        Assert.AreEqual(WebConsts.NameRequiredMessage, Error(new ServerFormValidator(), new ServerForm { Name = " " }));
        Assert.IsNull(Error(new ServerFormValidator(), new ServerForm { Name = "Server" }));
        Assert.AreEqual(WebConsts.NameRequiredMessage, Error(new GameModeRuleValidator(), new GameModeRule { Name = "" }));
        Assert.IsNull(Error(new GameModeRuleValidator(), new GameModeRule { Name = "ZoneMod" }));
    }

    [TestMethod]
    [DataRow(null, "c1m1_hotel", false)]
    [DataRow("Dead Center", null, false)]
    [DataRow("Dead Center", " , ;\n", false)]
    [DataRow("Dead Center", "c1m1_hotel, c1m2_streets", true)]
    public void ACampaignNeedsANameAndAMap(string? name, string? maps, bool valid)
    {
        var error = Error(new CampaignFormValidator(), new CampaignForm { Name = name, Maps = maps });

        Assert.AreEqual(valid ? null : WebConsts.CampaignRequiredMessage, error);
    }

    [TestMethod]
    public void ARemoteCommandIsRequired()
    {
        Assert.AreEqual(WebConsts.InvalidRemoteCommandMessage, Error(new RemoteCommandFormValidator(), new RemoteCommandForm { ServerId = 1, Command = "" }));
        Assert.IsNull(Error(new RemoteCommandFormValidator(), new RemoteCommandForm { ServerId = 1, Command = "sm_forcestart" }));
    }

    [TestMethod]
    [DataRow("everyone", null)]
    [DataRow("listed", null)]
    [DataRow("nobody", WebConsts.AntiCheatModeInvalidMessage)]
    [DataRow(null, WebConsts.AntiCheatModeInvalidMessage)]
    public void TheAntiCheatModeMustBeKnown(string? mode, string? expected)
    {
        Assert.AreEqual(expected, Error(new AntiCheatAccessModeCommandValidator(), new AntiCheatAccessModeCommand { Mode = mode }));
    }

    [TestMethod]
    public void APunishmentRemovesSomeExperience()
    {
        Assert.AreEqual(WebConsts.PunishmentPointsInvalidMessage, Error(new PunishmentCommandValidator(), new PunishmentCommand { CommunityId = "76561197960287930", LostExperiencePoints = 0 }));
        Assert.IsNull(Error(new PunishmentCommandValidator(), new PunishmentCommand { CommunityId = "STEAM_1:0:11101", LostExperiencePoints = 150 }));
    }

    [TestMethod]
    [DataRow(null, true)]
    [DataRow(-1, false)]
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(100000, true)]
    [DataRow(1000000, false)]
    public void ASkinPriceStaysWithinItsLimits(int? price, bool valid)
    {
        var error = Error(new SkinPriceCommandValidator(), new SkinPriceCommand { Price = price });

        Assert.AreEqual(valid ? null : WebConsts.SkinPriceInvalidMessage, error);
    }

    [TestMethod]
    public void AGiftNeedsASkin()
    {
        Assert.AreEqual(WebConsts.SkinRequiredMessage, Error(new SkinOwnershipCommandValidator(), new SkinOwnershipCommand { CommunityId = "76561197960287930", SkinId = "" }));
    }

    [TestMethod]
    public void TheSkinShopAddressesMustBeWebAddresses()
    {
        Assert.IsNull(Error(new SkinShopSettingsValidator(), new SkinShopSettings { AssetsUrl = "https://cdn.example.com", CatalogUrl = "http://example.com/skins.json" }));
        Assert.AreEqual(WebConsts.SkinSettingsInvalidUrl, Error(new SkinShopSettingsValidator(), new SkinShopSettings { AuthorContactUrl = "ftp://example.com" }));
    }

    [TestMethod]
    [DataRow("pt-BR")]
    [DataRow("es-ES")]
    public void EveryValidationMessageIsTranslated(string culture)
    {
        var translations = Translations(culture);

        string[] messages =
        [
            WebConsts.InvalidSteamIdMessage, WebConsts.SteamIdRequiredMessage, WebConsts.SteamIdInvalidMessage, WebConsts.NameRequiredMessage,
            WebConsts.CampaignRequiredMessage, WebConsts.ServerInUseMessage, WebConsts.LastAdministratorMessage, WebConsts.InvalidRemoteCommandMessage,
            WebConsts.AntiCheatModeInvalidMessage, WebConsts.SkinPriceInvalidMessage, WebConsts.SkinRequiredMessage, WebConsts.PunishmentPointsInvalidMessage,
            WebConsts.ForcedNameInvalidNameMessage, WebConsts.SkinSettingsInvalidUrl
        ];

        var missing = messages.Where(message => !translations.ContainsKey(message)).ToList();

        Assert.IsEmpty(missing, string.Join(" | ", missing));
    }

    private static string? Error<T>(IValidator<T> validator, T command)
    {
        var result = validator.Validate(command);

        return result.IsValid ? null : result.Errors[0].ErrorMessage;
    }

    private static Dictionary<string, string> Translations(string culture)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, WebProject)))
            directory = directory.Parent;

        Assert.IsNotNull(directory);

        var folder = Path.Combine(directory.FullName, WebProject, "Resources");
        var texts = new Dictionary<string, string>();

        foreach (var file in Directory.EnumerateFiles(folder, $"{culture}*.json"))
        foreach (var pair in JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) ?? [])
            texts[pair.Key] = pair.Value;

        return texts;
    }
}
