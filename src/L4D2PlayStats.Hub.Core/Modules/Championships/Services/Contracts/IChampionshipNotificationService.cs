using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipNotificationService
{
    void JoinAsked(Championship championship, long communityId);
    void JoinCancelled(Championship championship, long communityId);
    void JoinAnswered(Championship championship, long communityId, bool approved);
    void StatusChanged(Championship championship, ChampionshipStatus previous);
    void ParticipantAdded(Championship championship, long communityId);
    void ParticipantRemoved(Championship championship, long communityId);
    void MemberAdded(Championship championship, ChampionshipTeam team, long communityId);
    void LeaderChosen(Championship championship, ChampionshipTeam team, long communityId, bool captain);
    void DraftOrdered(Championship championship);
    void Picked(Championship championship, ChampionshipTeam team, long communityId);
    void IdentitySent(Championship championship, ChampionshipTeam team);
    void IdentityAnswered(Championship championship, ChampionshipTeam team, bool approved);
    void StageBuilt(Championship championship, ChampionshipStage stage);
    void DateProposed(Championship championship, ChampionshipSeries series, int teamId);
    void DateAnswered(Championship championship, ChampionshipSeries series, ChampionshipDateProposal proposal, bool accepted);
    void Scheduled(Championship championship, ChampionshipSeries series);
    void Vetoed(Championship championship, ChampionshipSeries series, int teamId);
    void CampaignPicked(Championship championship, ChampionshipSeries series, ChampionshipGame game, int teamId);
    void GameFinished(Championship championship, ChampionshipSeries series, ChampionshipGame game, bool seriesWasFinished);
    void Walkover(Championship championship, ChampionshipSeries series);
}
