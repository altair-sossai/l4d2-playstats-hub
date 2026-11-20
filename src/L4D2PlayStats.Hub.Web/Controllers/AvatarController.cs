using L4D2PlayStats.Hub.Web.Infrastructure.Avatars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class AvatarController(AvatarStore avatarStore, AvatarQueue avatarQueue) : Controller
{
    [HttpGet("/avatar/{communityId:long}")]
    [HttpGet("/avatar/{communityId:long}.jpg")]
    public IActionResult Index(long communityId)
    {
        var path = avatarStore.PathFor(communityId);

        if (System.IO.File.Exists(path))
        {
            Response.Headers[HeaderNames.CacheControl] = $"public, max-age={AppConsts.AvatarCacheInSeconds}";

            return PhysicalFile(path, AppConsts.AvatarContentType);
        }

        avatarQueue.Request(communityId);
        Response.Headers[HeaderNames.CacheControl] = AppConsts.NoStore;

        return Redirect(AppConsts.EmptyAvatarUrl);
    }
}