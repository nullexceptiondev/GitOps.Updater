using System.IO.Abstractions;

namespace GitOps.Updater.Cli.Infrastructure
{
    public abstract class GitClient(IFileSystem fileSystem)
    {
        public IFileSystem FileSystem => fileSystem;

        public abstract string GetUrl(string url, string patName, string pat);
        public abstract Task<bool> CloneAsync(string url, string branch, int depth = 0);
        public abstract Task SetConfigAsync(string userEmail, string userName);
        public abstract Task<bool> PushFilesAsync(string url, string branch, string[] files, string message);

        public abstract string CreateWorkingDirectory();
        public abstract void DeleteWorkingDirectory();
    }
}
