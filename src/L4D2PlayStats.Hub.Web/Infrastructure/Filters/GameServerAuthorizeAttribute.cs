using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class GameServerAuthorizeAttribute() : TypeFilterAttribute(typeof(GameServerAuthorizeFilter));
