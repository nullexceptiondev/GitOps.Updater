using GitOps.Updater.Cli.Extensions;
using System.Text.RegularExpressions;

namespace GitOps.Updater.Cli.Helpers
{
    internal static partial class VersionHelper
    {
        internal static Version? GetVersionFromDirectory(string directoryPattern, string rootDirectory, string directory)
        {
            var versionInfo = GetVersionInfoFromPattern(directoryPattern, null);

            if (versionInfo is not null && !string.IsNullOrEmpty(versionInfo.VariableName))
            {
                var major = 0;
                var minor = 0;
                var build = 0;
                var revision = 0;

                var tempDirectory = directory.Remove(0, rootDirectory.Length + 1); //Remove root and starting slash
                var versionRightString = directoryPattern.Substring(versionInfo.VariableNameStartIndex + versionInfo.VariableName.Length);
                var versionString = tempDirectory.Substring(versionInfo.VariableNameStartIndex, tempDirectory.Length - versionInfo.VariableNameStartIndex - versionRightString.Length);

                string[] numbers = Regex.Split(versionString, @"\D+");

                if (numbers.Length > 0) major = Convert.ToInt32(numbers[0]);
                if (numbers.Length > 1) minor = Convert.ToInt32(numbers[1]);
                if (numbers.Length > 2) build = Convert.ToInt32(numbers[2]);
                if (numbers.Length > 3) revision = Convert.ToInt32(numbers[3]);

                return new Version(major, minor, build, revision);
            }

            return null;
        }

        internal static (string VersionNumber, string VersionTag) SplitVersion(string version)
        {
            var tagIndex = version.IndexOf('-');
            string versionNumber;
            string versionTag;

            if (tagIndex >= 0)
            {
                versionNumber = version.Substring(0, tagIndex);
                versionTag = version.Substring(tagIndex + 1);
            }
            else
            {
                versionNumber = version;
                versionTag = "";
            }
            return (versionNumber, versionTag);
        }

        internal static bool VersionRuleMatch(string versionRule, string version, out string versionNumber, out string versionTag)
        {
            var versionSplit = SplitVersion(version);
            versionNumber = versionSplit.VersionNumber;
            versionTag = versionSplit.VersionTag;

            var tagIndex = versionRule.IndexOf('-');
            string versionRuleNumber;
            string[] versionRuleTags;

            if (tagIndex >= 0)
            {
                versionRuleNumber = versionRule.Substring(0, tagIndex);
                var versionRuleTag = versionRule.Substring(tagIndex + 1);

                if (versionRuleTag.StartsWith('[') && versionRuleTag.EndsWith(']'))
                {
                    versionRuleTags = versionRuleTag.Replace("[", "").Replace("]", "").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }
                else
                {
                    versionRuleTags = [versionRuleTag];
                }
            }
            else
            {
                versionRuleNumber = versionRule;
                versionRuleTags = [];
            }

            // Split the version and rule into segments
            var versionSegments = versionNumber.Split('.');
            var ruleSegments = versionRuleNumber.Split('.');

            // Ensure that the number of segments in the version and rule match
            if (versionSegments.Length != ruleSegments.Length)
                return false;

            for (int i = 0; i < versionSegments.Length; i++)
            {
                // If the rule segment is not a wildcard, compare it directly
                if (ruleSegments[i] != "*")
                {
                    if (versionSegments[i] != ruleSegments[i])
                        return false;
                }
                // If the rule segment is a wildcard, skip the comparison
            }

            if (versionRuleTags.Length > 0 && !versionRuleTags.Contains(versionTag, StringComparer.OrdinalIgnoreCase))
                return false;

            // All segments match, so the version satisfies the rule
            return true;
        }

        internal static VersionInfo GetVersionInfoFromPattern(string pattern, Version? versionNumber)
        {
            var versionRegex = VersionVariableRegex();
            var match = versionRegex.Match(pattern);

            var variableName = "";
            var versionValue = "";
            int versionSegmentCount = 0;
            int variableStartIndex = -1;

            if (match.Success)
            {
                variableName = match.Value;
                variableStartIndex = match.Index;

                var content = match.Groups[1].Value;
                versionSegmentCount = content.Count(c => c == 'X');

                if (versionNumber is not null)
                {
                    int[] fields = { versionNumber.Major, versionNumber.Minor, versionNumber.Build, versionNumber.Revision };
                    versionValue = content;
                    for (int i = 0; i < versionSegmentCount; i++)
                    {
                        versionValue = versionValue.ReplaceFirst("X", fields[i].ToString());
                    }
                }
            }

            return new VersionInfo(variableName, versionValue, versionSegmentCount, variableStartIndex);
        }

        //internal static Version DecrementVersion(Version version, int segmentCount)
        //{
        //    int major = version.Major;
        //    int minor = segmentCount > 1 ? version.Minor : 0;
        //    int build = segmentCount > 2 ? version.Build : 0;
        //    int revision = segmentCount > 3 ? version.Revision : 0;

        //    if (revision > 0)
        //    {
        //        revision--;
        //    }
        //    else if (build > 0)
        //    {
        //        build--;
        //        revision = 9999; // or whatever maximum you want for revision
        //    }
        //    else if (minor > 0)
        //    {
        //        minor--;
        //        build = 9999; // or whatever maximum you want for build
        //        revision = 9999; // or whatever maximum you want for revision
        //    }
        //    else if (major > 0)
        //    {
        //        major--;
        //        minor = 9999; // or whatever maximum you want for minor
        //        build = 9999; // or whatever maximum you want for build
        //        revision = 9999; // or whatever maximum you want for revision
        //    }

        //    return new Version(major, minor, build, revision);
        //}

        [GeneratedRegex(@"\{v(.*?)\}")]
        private static partial Regex VersionVariableRegex();
    }

    public record VersionInfo(string VariableName, string Value, int SegmentCount, int VariableNameStartIndex);
}
