using GitOps.Updater.Cli.Helpers;
using FluentAssertions;

namespace GitOps.Updater.Tests.Helpers
{
    public class VersionHelperTests
    {
        [Theory]
        [InlineData("*.*.*.*", "1.2.3.4", true)]
        [InlineData("1.*.*.*", "1.2.3.4", true)]
        [InlineData("1.2.*.*", "1.2.3.4", true)]
        [InlineData("1.2.3.*", "1.2.3.4", true)]
        [InlineData("1.2.3.4", "1.2.3.4", true)]
        [InlineData("2.*.*.*", "1.2.3.4", false)]
        [InlineData("1.1.*.*", "1.2.3.4", false)]
        [InlineData("1.2.2.*", "1.2.3.4", false)]
        [InlineData("1.2.3.3", "1.2.3.4", false)]
        [InlineData("*.*.*.*", "1.2.3.4-dev", true)]
        [InlineData("*.*.*.*-dev", "1.2.3.4-dev", true)]
        [InlineData("*.*.*.*-[dev]", "1.2.3.4-dev", true)]
        [InlineData("*.*.*.*-[dev|prod]", "1.2.3.4-dev", true)]
        [InlineData("*.*.*.*-[ dev |   prod ]", "1.2.3.4-dev", true)]
        [InlineData("*.*.*.*-[ dev |   prod | ]", "1.2.3.4-dev", true)]
        [InlineData("*.*.*.*-[dev|prod]", "1.2.3.4-beta", false)]
        [InlineData("*.*.*", "1.2.3.4", false)]
        [InlineData("*.*.*.*", "1.2.3", false)]

        public void AllowUpdate(string versionRule, string version, bool expectedResult)
        {
            var result = VersionHelper.VersionRuleMatch(versionRule, version, out var versionNumber, out var versionTag);
            result.Should().Be(expectedResult);
        }

        //[Theory]
        //[InlineData("0.0.0.0", 1, "0.0.0.0")]
        //[InlineData("1.0.0.0", 1, "0.9999.9999.9999")]
        //[InlineData("1.1.0.0", 2, "1.0.9999.9999")]
        //[InlineData("1.1.1.0", 3, "1.1.0.9999")]
        //[InlineData("1.1.1.1", 4, "1.1.1.0")]
        //public void DecrementVersion(string version, int segmentCount, string expectedResult)
        //{
        //    var result = VersionHelper.DecrementVersion(Version.Parse(version), segmentCount);
        //    result.ToString().Should().Be(expectedResult);
        //}

        [Theory]
        [InlineData("releases/v{vX}", "/files", "/files/releases/v1", "1.0.0.0")]
        [InlineData("releases/v{vX.X}", "/files", "/files/releases/v1.2", "1.2.0.0")]
        [InlineData("releases/v{vX.X.X}", "/files", "/files/releases/v1.2.3", "1.2.3.0")]
        [InlineData("releases/v{vX.X.X.X}", "/files", "/files/releases/v1.2.3.4", "1.2.3.4")]
        [InlineData("releases\\v{vX}", "c:\\files", "c:\\files\\releases\\v1", "1.0.0.0")]
        [InlineData("releases\\v{vX.X}", "c:\\files", "c:\\files\\releases\\v1.2", "1.2.0.0")]
        [InlineData("releases\\v{vX.X.X}", "c:\\files", "c:\\files\\releases\\v1.2.3", "1.2.3.0")]
        [InlineData("releases\\v{vX.X.X.X}", "c:\\files", "c:\\files\\releases\\v1.2.3.4", "1.2.3.4")]
        [InlineData("releases/v{vX-X-X}", "/files", "/files/releases/v11-12-13", "11.12.13.0")]
        [InlineData("releases/v{vX_X_X}", "/files", "/files/releases/v11_12_13", "11.12.13.0")]
        public void GetVersionFromDirectory(string directoryPattern, string rootDirectory, string directory, string expectedValue)
        {
            var result = VersionHelper.GetVersionFromDirectory(directoryPattern, rootDirectory, directory);
            var expectedVersion = Version.Parse(expectedValue);
            result.Should().Be(expectedVersion);
        }
    }
}
