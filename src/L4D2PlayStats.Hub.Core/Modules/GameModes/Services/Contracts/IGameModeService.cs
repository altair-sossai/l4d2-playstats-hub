using L4D2PlayStats.Hub.Core.Modules.GameModes.Models;

namespace L4D2PlayStats.Hub.Core.Modules.GameModes.Services.Contracts;

public interface IGameModeService
{
    Task<GameModeRuleSet> GetRuleSetAsync(CancellationToken cancellationToken = default);
    Task<List<GameModeRule>> ListRulesAsync(CancellationToken cancellationToken = default);
    Task<List<GameModeRule>> ListRulesAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountRulesAsync(CancellationToken cancellationToken = default);
    Task<List<GameConfiguration>> ListConfigurationsAsync(CancellationToken cancellationToken = default);
    Task<List<GameConfiguration>> ListConfigurationsAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountConfigurationsAsync(CancellationToken cancellationToken = default);
    Task SeedAsync(CancellationToken cancellationToken = default);
    Task<GameModeRule> SaveRuleAsync(GameModeRule rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(int id, CancellationToken cancellationToken = default);
    Task RegisterSeenAsync(string configurationName, int teamSize, CancellationToken cancellationToken = default);
}
