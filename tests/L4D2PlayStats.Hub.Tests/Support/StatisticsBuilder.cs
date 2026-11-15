using System.Reflection;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Tests.Support;

public class StatisticsBuilder
{
    public const long PlayerA1 = 76561197990571514;
    public const string SampleFileName = "2023-02-04_03-13_0011_c8m1_apartment.txt";

    private readonly ParsedStatistics _statistics;

    private StatisticsBuilder(string content)
    {
        _statistics = new ParsedStatistics(content);
    }

    public ParsedStatistics Statistics => _statistics;

    public string FileName => $"{_statistics.GameRound!.When:yyyy-MM-dd_HH-mm}_{_statistics.GameRound.Round:0000}_{_statistics.GameRound.MapName}.txt";

    public string Content => _statistics.ToString();

    public static string SampleContent()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames().First(name => name.EndsWith(SampleFileName, StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static StatisticsBuilder Sample()
    {
        return new StatisticsBuilder(SampleContent());
    }

    public static StatisticsBuilder Map(string mapName, DateTime when, int round, int teamAScore, int teamBScore)
    {
        return Sample().WithMap(mapName).At(when).WithRound(round).WithScores(teamAScore, teamBScore);
    }

    public StatisticsBuilder WithMap(string mapName)
    {
        _statistics.GameRound!.MapName = mapName;

        return this;
    }

    public StatisticsBuilder At(DateTime when)
    {
        _statistics.GameRound!.When = when;

        return this;
    }

    public StatisticsBuilder WithRound(int round)
    {
        _statistics.GameRound!.Round = round;

        return this;
    }

    public StatisticsBuilder WithConfiguration(string configurationName, int teamSize = 4)
    {
        _statistics.GameRound!.ConfigurationName = configurationName;
        _statistics.GameRound.TeamSize = teamSize;

        return this;
    }

    public StatisticsBuilder WithScores(int teamAScore, int teamBScore)
    {
        _statistics.Scoring!.TeamA!.Score = teamAScore;
        _statistics.Scoring.TeamB!.Score = teamBScore;

        return this;
    }

    public StatisticsBuilder WithoutPlayer(long communityId)
    {
        var id = communityId.ToString();

        foreach (var half in _statistics.Halves)
        {
            half.Players.RemoveAll(p => p.CommunityId == id);
            half.InfectedPlayers.RemoveAll(p => p.CommunityId == id);
        }

        return this;
    }

    public List<long> Survivors(char team)
    {
        var half = _statistics.Halves.First(h => h.RoundHalf!.Team == team);

        return [.. half.Players.Select(p => long.Parse(p.CommunityId!))];
    }
}
