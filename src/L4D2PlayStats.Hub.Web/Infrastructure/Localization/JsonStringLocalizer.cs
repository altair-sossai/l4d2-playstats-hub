using System.Globalization;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Localization;

public class JsonStringLocalizer(JsonStringLocalizerFactory factory) : IStringLocalizer
{
    public LocalizedString this[string name]
    {
        get
        {
            var value = factory.Find(name, CultureInfo.CurrentUICulture);

            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var value = factory.Find(name, CultureInfo.CurrentUICulture);

            return new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, value ?? name, arguments), value == null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return factory.Load(CultureInfo.CurrentUICulture.Name).Select(pair => new LocalizedString(pair.Key, pair.Value));
    }
}