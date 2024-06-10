namespace GitOps.Updater.Cli.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            return pos < 0 ? text : string.Concat(text.AsSpan()[..pos], replace, text.AsSpan(pos + search.Length));
        }

        public static bool StartsWithAny(this string input, params string[] values)
        {
            foreach (var x in values)
                if (input.StartsWith(x))
                    return true;

            return false;
        }
    }
}
