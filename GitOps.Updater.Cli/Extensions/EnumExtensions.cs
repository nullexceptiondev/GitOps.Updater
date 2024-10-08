﻿namespace GitOps.Updater.Cli.Extensions
{
    public static class EnumExtensions
    {
        public static string ToFlagString<T>(this T value, bool ignoreZero = false) where T : Enum
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Only supported for enum types.");

            return string.Join(" & ", Enum.GetValues(typeof(T))
                .Cast<T>()
                .Where(v => (!ignoreZero || Convert.ToInt32(v) != 0) && value.HasFlag(v)));
        }

        public static bool HasAnyFlag<T>(this T value, params T[] flags) where T : Enum
        {
            foreach (var flag in flags)
            {
                if (value.HasFlag(flag)) return true;
            }

            return false;
        }
    }
}
