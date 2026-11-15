using L4D2PlayStats.Hub.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class AvatarController(AvatarStore avatarStore) : Controller
{
    [HttpGet("/avatar/{communityId:long}")]
    [ResponseCache(Duration = AppConsts.AvatarCacheInSeconds)]
    public IActionResult Index(long communityId)
    {
        var path = avatarStore.PathFor(communityId);

        return System.IO.File.Exists(path)
            ? PhysicalFile(path, "image/jpeg")
            : Redirect("/imgs/avatar-empty.png");
    }
}
