using GitOps.Updater.Cli.Helpers;
using FluentAssertions;

namespace GitOps.Updater.Tests.Helpers
{
    public class YamlHelperTests
    {
        [Fact]
        public void MergeYaml_NoChildren_NoOverride()
        {
            var baseYaml = "image: 1.0.0.0-dev\r\ncount: 0";
            var overrideYaml = "";
            var resultYaml = YamlHelper.MergeYaml(baseYaml, overrideYaml);

            resultYaml.Should().Be("image: 1.0.0.0-dev\r\ncount: 0\r\n");
        }

        [Fact]
        public void MergeYaml_NoChildren_NoBase()
        {
            var baseYaml = "";
            var overrideYaml = "image: 1.0.0.0-dev\r\ncount: 0";
            var resultYaml = YamlHelper.MergeYaml(baseYaml, overrideYaml);

            resultYaml.Should().Be("image: 1.0.0.0-dev\r\ncount: 0\r\n");
        }

        [Fact]
        public void MergeYaml_WithChildren()
        {
            var baseYaml = "image: 1.0.0.0-dev\r\ncount: 0\r\nchild:\r\n  count2: 0\r\n  count3: 0";
            var overrideYaml = "image: 1.0.0.1-dev\r\ncount: 1\r\nchild:\r\n  count2: 2";
            var resultYaml = YamlHelper.MergeYaml(baseYaml, overrideYaml);

            resultYaml.Should().Be("image: 1.0.0.1-dev\r\ncount: 1\r\nchild:\r\n  count2: 2\r\n  count3: 0\r\n");
        }
    }
}
