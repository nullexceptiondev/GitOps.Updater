namespace GitOps.Updater.Cli.Infrastructure
{
    public class GitClientException : Exception
    {
        public GitClientException(string message) : base(message) { }
    }
}
