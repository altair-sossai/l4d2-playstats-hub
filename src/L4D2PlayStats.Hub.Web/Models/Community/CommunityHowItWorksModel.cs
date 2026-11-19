using L4D2PlayStats.Hub.Core.Modules.Community.Settings;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public record CommunityHowItWorksModel(CommunityRankingSettings Settings, bool Collapsed);
