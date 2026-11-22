namespace L4D2PlayStats.Hub.Web.Models.Players;

public record RelatedAccountsResult(string CommunityId, IReadOnlyList<RelatedAccountResult> Accounts);
