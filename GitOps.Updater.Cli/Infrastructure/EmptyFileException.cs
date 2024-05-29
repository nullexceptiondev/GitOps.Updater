namespace GitOps.Updater.Cli.Infrastructure
{
    [Serializable]
    public class EmptyFileException : Exception
    {
        public EmptyFileException()
        {
        }

        public EmptyFileException(string? message) : base(message)
        {
        }

        public EmptyFileException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}