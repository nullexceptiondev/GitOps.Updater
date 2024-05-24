namespace GitOps.Updater.Cli.Extensions
{
    public static class EnumExtensions
    {
        public static string ToFlagString<T>(this T value) where T : Enum
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Only supported for enum types.");

            return string.Join("& ", Enum.GetValues(typeof(T))
                .Cast<T>()
                .Where(v => value.HasFlag(v)));
        }
    }
}
