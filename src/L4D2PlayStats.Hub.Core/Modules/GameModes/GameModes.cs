using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.GameModes;

public class GameModeRule
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ConfigurationPattern { get; set; } = AppConsts.AnyConfigurationPattern;
    public int? TeamSize { get; set; }
    public int MinimumMaps { get; set; } = AppConsts.DefaultMinimumMaps;
    public bool CountsForRanking { get; set; } = true;
    public int Priority { get; set; }

    public bool Matches(string? configurationName, int teamSize)
    {
        if (TeamSize != null && TeamSize != teamSize)
            return false;

        return WildcardPattern.IsMatch(ConfigurationPattern, configurationName ?? string.Empty);
    }
}

public class GameConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TeamSize { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
}

public static class WildcardPattern
{
    public static bool IsMatch(string pattern, string value)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var expression = "^" + Regex.Escape(pattern.Trim()).Replace("\\*", ".*").Replace("\\?", ".") + "$";

        return Regex.IsMatch(value.Trim(), expression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

public class GameModeRuleSet(IReadOnlyList<GameModeRule> rules)
{
    private readonly List<GameModeRule> _rules = [.. rules.OrderBy(r => r.Priority).ThenBy(r => r.Id)];

    public GameModeRule? Find(string? configurationName, int teamSize)
    {
        return _rules.FirstOrDefault(rule => rule.Matches(configurationName, teamSize));
    }

    public bool CountsForRanking(string? configurationName, int teamSize, int maps)
    {
        var rule = Find(configurationName, teamSize);

        return rule is { CountsForRanking: true } && maps >= rule.MinimumMaps;
    }
}

public interface IGameModeService
{
    Task<GameModeRuleSet> GetRuleSetAsync(CancellationToken cancellationToken = default);
    Task<List<GameModeRule>> ListRulesAsync(CancellationToken cancellationToken = default);
    Task<List<GameConfiguration>> ListConfigurationsAsync(CancellationToken cancellationToken = default);
    Task SeedAsync(CancellationToken cancellationToken = default);
    Task<GameModeRule> SaveRuleAsync(GameModeRule rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(int id, CancellationToken cancellationToken = default);
    Task RegisterSeenAsync(string configurationName, int teamSize, CancellationToken cancellationToken = default);
}

public class GameModeService(HubDbContext dbContext, IMemoryCache memoryCache, HubState hubState, IClock clock) : IGameModeService
{
    private const string CacheKey = "game_mode_rules";

    public async Task<GameModeRuleSet> GetRuleSetAsync(CancellationToken cancellationToken = default)
    {
        var ruleSet = await memoryCache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConsts.SettingsCacheInMinutes);

            return new GameModeRuleSet(await ListRulesAsync(cancellationToken));
        });

        return ruleSet!;
    }

    public Task<List<GameModeRule>> ListRulesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.GameModeRules.AsNoTracking().OrderBy(r => r.Priority).ThenBy(r => r.Id).ToListAsync(cancellationToken);
    }

    public Task<List<GameConfiguration>> ListConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.GameConfigurations.AsNoTracking().OrderByDescending(c => c.LastSeenAtUtc).ToListAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.GameModeRules.AnyAsync(cancellationToken))
            return;

        dbContext.GameModeRules.Add(new GameModeRule
        {
            Name = "4v4",
            ConfigurationPattern = AppConsts.AnyConfigurationPattern,
            TeamSize = AppConsts.DefaultTeamSize,
            MinimumMaps = AppConsts.DefaultMinimumMaps,
            CountsForRanking = true,
            Priority = 100
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        memoryCache.Remove(CacheKey);
    }

    public async Task<GameModeRule> SaveRuleAsync(GameModeRule rule, CancellationToken cancellationToken = default)
    {
        var existing = rule.Id == 0 ? null : await dbContext.GameModeRules.FirstOrDefaultAsync(r => r.Id == rule.Id, cancellationToken);

        if (existing == null)
        {
            existing = new GameModeRule();
            dbContext.GameModeRules.Add(existing);
        }

        existing.Name = rule.Name.Trim();
        existing.ConfigurationPattern = string.IsNullOrWhiteSpace(rule.ConfigurationPattern) ? AppConsts.AnyConfigurationPattern : rule.ConfigurationPattern.Trim();
        existing.TeamSize = rule.TeamSize;
        existing.MinimumMaps = Math.Max(1, rule.MinimumMaps);
        existing.CountsForRanking = rule.CountsForRanking;
        existing.Priority = rule.Priority;

        await dbContext.SaveChangesAsync(cancellationToken);
        memoryCache.Remove(CacheKey);

        return existing;
    }

    public async Task<bool> DeleteRuleAsync(int id, CancellationToken cancellationToken = default)
    {
        var rule = await dbContext.GameModeRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule == null)
            return false;

        dbContext.GameModeRules.Remove(rule);
        await dbContext.SaveChangesAsync(cancellationToken);
        memoryCache.Remove(CacheKey);

        return true;
    }

    public async Task RegisterSeenAsync(string configurationName, int teamSize, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var configuration = await dbContext.GameConfigurations.FirstOrDefaultAsync(c => c.Name == configurationName && c.TeamSize == teamSize, cancellationToken);

        if (configuration == null)
        {
            configuration = new GameConfiguration { Name = configurationName, TeamSize = teamSize, FirstSeenAtUtc = now };
            dbContext.GameConfigurations.Add(configuration);
            hubState.DataChanged();
        }

        configuration.LastSeenAtUtc = now;
    }
}
