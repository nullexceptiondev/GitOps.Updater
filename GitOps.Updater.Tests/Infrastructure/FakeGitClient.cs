using GitOps.Updater.Cli.Infrastructure;
using System.IO.Abstractions;

namespace GitOps.Updater.Tests.Infrastructure
{
    public class FakeGitClient(IFileSystem fileSystem) : GitClient(fileSystem)
    {
        public string WorkingDirectory { get; set; } = string.Empty;

        public bool CloneSuccessful { get; set; } = true;
        public bool PushSuccessful { get; set; } = true;

        public override Task<bool> CloneAsync(string repoUri, string branch, int depth = 0)
        {
            return Task.FromResult(CloneSuccessful);
        }

        public override string CreateWorkingDirectory()
        {
            return WorkingDirectory;
        }

        public override void DeleteWorkingDirectory()
        {
            //Do nothing so we can test the files in the filesystem
        }

        public override string GetUrl(string url, string patName, string pat)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> PushFilesAsync(string url, string branch, string[] files, string message)
        {
            return Task.FromResult(PushSuccessful);
        }

        public override Task SetConfigAsync(string userEmail, string userName)
        {
            return Task.CompletedTask;
        }
    }
}
