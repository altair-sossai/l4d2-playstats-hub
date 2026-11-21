namespace L4D2PlayStats.Hub.Web.Helpers;

public static class CommunityVoteUrls
{
    public static string Votes(long? voter, long? player, int page = 1)
    {
        return QueryUrl.Build(AppConsts.CommunityVotesPage, (AppConsts.CommunityVoterQuery, voter), (AppConsts.CommunityPlayerQuery, player), (AppConsts.CommunityPageQuery, page));
    }
}
