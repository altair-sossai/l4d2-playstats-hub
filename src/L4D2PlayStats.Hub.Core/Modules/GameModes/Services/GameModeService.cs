using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Models;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.GameModes.Services;

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
