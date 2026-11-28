namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipCandidateMatch(int MatchId, DateTime PlayedAtUtc, string? Campaign, int MapCount, int TeamAScore, int TeamBScore, bool MatchTeamAIsTeamA, int TeamAPlayers, int TeamBPlayers, bool Linked);
