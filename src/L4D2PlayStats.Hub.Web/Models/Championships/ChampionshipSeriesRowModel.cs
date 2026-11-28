using L4D2PlayStats.Hub.Core.Modules.Championships;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public record ChampionshipSeriesRowModel(ChampionshipPageModel Page, ChampionshipSeries Series, string Url, bool ShowStage);
