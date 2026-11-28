namespace L4D2PlayStats.Hub.Web.Models;

public record PlayerChipModel(long CommunityId, string Name, bool Link = true, bool NewTab = false);
