using System.Globalization;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Web.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Extensions;

public static partial class GameEventHtmlExtensions
{
    [GeneratedRegex(AppConsts.EventPlaceholderPattern)]
    private static partial Regex PlaceholderRegex();

    public static string StarsText(this GameEvent gameEvent)
    {
        return new string(AppConsts.EventStar, Math.Max(0, gameEvent.Stars));
    }

    extension(IHtmlHelper html)
    {
        public IHtmlContent EventPlayer(LivePlayer player)
        {
            return html.EventPlayer(player, string.Empty);
        }

        public IHtmlContent EventPlayer(LivePlayer? player, string fallback)
        {
            var name = player?.Name ?? fallback;
            var url = LiveDisplay.PlayerUrl(player);

            var tag = new TagBuilder(string.IsNullOrEmpty(url) ? "span" : "a");
            tag.AddCssClass(AppConsts.EventPlayerClass);

            if (!string.IsNullOrEmpty(url))
            {
                tag.Attributes["href"] = url;
                tag.Attributes["target"] = AppConsts.LinkTargetBlank;
            }

            tag.InnerHtml.Append(name);

            return tag;
        }

        public IHtmlContent EventSpan(string cssClass, string text)
        {
            var span = new TagBuilder("span");
            span.AddCssClass(cssClass);
            span.InnerHtml.Append(text);

            return span;
        }

        public IHtmlContent EventStat(string format, params object[] values)
        {
            var args = values
                .Select(value => html.EventSpan(AppConsts.EventHighlightClass, Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty))
                .ToArray();

            return html.EventFormat(format, args);
        }

        public IHtmlContent EventFormat(string format, params IHtmlContent[] args)
        {
            var content = new HtmlContentBuilder();
            var regex = PlaceholderRegex();
            var segments = regex.Split(format);

            for (var i = 0; i < segments.Length; i++)
                if (i % 2 == 0)
                    content.Append(segments[i]);
                else
                    content.AppendHtml(args[int.Parse(segments[i], CultureInfo.InvariantCulture)]);

            return content;
        }
    }
}