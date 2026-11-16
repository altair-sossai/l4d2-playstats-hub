using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerRelationRowModel(long communityId, RankingPlayer? player, string? knownName, PlayerRelation? first, PlayerRelation? second)
{
    public long CommunityId { get; } = communityId;
    public RankingPlayer? Player { get; } = player;
    public string Name => Player?.MainName ?? knownName ?? CommunityId.ToString();

    public PlayerRelationModel? First { get; } = first == null ? null : new PlayerRelationModel(first);
    public PlayerRelationModel? Second { get; } = second == null ? null : new PlayerRelationModel(second);

    public int TogetherGames => (First?.TogetherGames ?? 0) + (Second?.TogetherGames ?? 0);
    public int TogetherWins => (First?.TogetherWins ?? 0) + (Second?.TogetherWins ?? 0);

    public int AgainstGames => (First?.AgainstGames ?? 0) + (Second?.AgainstGames ?? 0);
    public int AgainstWins => (First?.AgainstWins ?? 0) + (Second?.AgainstWins ?? 0);

    public int TotalGames => First != null
        ? First.TogetherGames + First.AgainstGames
        : (Second?.TogetherGames ?? 0) + (Second?.AgainstGames ?? 0);
}
