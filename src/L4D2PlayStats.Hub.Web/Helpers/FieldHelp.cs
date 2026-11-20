using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class FieldHelp
{
    public static IHtmlContent Tip(string summary)
    {
        var text = Encode(summary);

        return new HtmlString($"""<span class="field-help" tabindex="0" role="note" data-toggle="tooltip" data-placement="top" data-container="body" title="{text}" aria-label="{text}"><span class="glyphicon glyphicon-question-sign"></span></span>""");
    }

    public static IHtmlContent More(string summary, string title, string details)
    {
        var text = Encode(summary);

        return new HtmlString($"""<a href="#" class="field-help field-help-more" role="button" data-toggle="tooltip" data-placement="top" data-container="body" title="{text}" aria-label="{text}" data-help-summary="{text}" data-help-title="{Encode(title)}" data-help-details="{Encode(details)}"><span class="glyphicon glyphicon-question-sign"></span></a>""");
    }

    private static string Encode(string value)
    {
        return HtmlEncoder.Default.Encode(value);
    }
}