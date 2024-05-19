namespace GitOps.Updater.Cli.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            return pos < 0 ? text : string.Concat(text.AsSpan()[..pos], replace, text.AsSpan(pos + search.Length));
        }
    }
}
