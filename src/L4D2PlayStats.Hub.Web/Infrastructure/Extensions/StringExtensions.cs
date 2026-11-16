namespace L4D2PlayStats.Hub.Web.Infrastructure.Extensions;

public static class StringExtensions
{
    extension(string name)
    {
        public string TruncatePlayerName()
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (name.Length >= AppConsts.LongPlayerNameLength && !name.Contains(' '))
                return name.Truncate(AppConsts.LongPlayerNameVisibleLength).Trim();

            return name.Truncate(AppConsts.PlayerNameVisibleLength).Trim();
        }

        public string Truncate(int max)
        {
            return name.Length <= max ? name : name[..max];
        }
    }
}
