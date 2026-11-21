using L4D2PlayStats.Hub.Core.Modules.Patents.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Patents.Services.Contracts;

public interface IPatentService
{
    Task<PatentScale> ScaleAsync(CancellationToken cancellationToken = default);
}
