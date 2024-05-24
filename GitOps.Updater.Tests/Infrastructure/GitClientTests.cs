using GitOps.Updater.Cli.Infrastructure;
using FluentAssertions;
using System.IO.Abstractions;

namespace GitOps.Updater.Tests.Infrastructure
{
    public class GitClientTests
    {
        [Fact(Skip = "Run manually")]
        public async Task GitCloneAndPush()
        {
            var url = "";
            var branch = "main";
            var userName = "";
            var userEmail = "";
            var message = "File Change";

            var patName = "";
            var pat = "";

            var fileSystem = new FileSystem();
            var gitClient = new LocalGitClient(fileSystem);

            try
            {
                var gitRepo = gitClient.CreateWorkingDirectory();

                url = gitClient.GetUrl(url, patName, pat);

                var cloneResult = await gitClient.CloneAsync(url, branch, 1);

                var localDirectory = fileSystem.DirectoryInfo.New(gitRepo);
                foreach (var info in localDirectory.GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }

                cloneResult.Should().BeTrue();

                await gitClient.SetConfigAsync(userEmail, userName);

                var files = new List<string>();

                var file = fileSystem.Path.Combine(gitRepo, "environments/dev/config.yaml");

                var fileContents = fileSystem.File.ReadAllText(file);

                fileContents = fileContents + "gitchange";

                fileSystem.File.WriteAllText(file, fileContents);

                //var pushResult = await gitClient.PushFilesAsync(url, branch, files.ToArray(), message);

                //pushResult.Should().BeTrue();
            }
            finally
            {
                gitClient.DeleteWorkingDirectory();
            }
        }
    }
}
