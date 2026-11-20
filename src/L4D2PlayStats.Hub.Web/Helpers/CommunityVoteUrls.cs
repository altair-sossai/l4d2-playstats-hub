namespace L4D2PlayStats.Hub.Web.Helpers;

public static class CommunityVoteUrls
{
    public static string Votes(long? voter, long? player, int page = 1)
    {
        var query = new List<string>();

        if (voter != null)
            query.Add($"{AppConsts.CommunityVoterQuery}={voter}");

        if (player != null)
            query.Add($"{AppConsts.CommunityPlayerQuery}={player}");

        if (page > 1)
            query.Add($"{AppConsts.CommunityPageQuery}={page}");

        return query.Count == 0 ? AppConsts.CommunityVotesPage : $"{AppConsts.CommunityVotesPage}?{string.Join("&", query)}";
    }
}
