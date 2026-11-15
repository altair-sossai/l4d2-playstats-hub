using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireModuleAttribute(HubModule module) : TypeFilterAttribute(typeof(RequireModuleFilter))
{
    public HubModule Module { get; } = module;
}
