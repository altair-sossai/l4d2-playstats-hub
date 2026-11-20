using L4D2PlayStats.Hub.Web.Models.AntiCheat;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IAntiCheatVersionService
{
    Task<ClientVersionResponse?> GetLatestAsync(CancellationToken cancellationToken = default);
}