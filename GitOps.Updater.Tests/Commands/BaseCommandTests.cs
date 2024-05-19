using GitOps.Updater.Tests;

namespace GitOps.Updater.Tests.Commands
{
    public class BaseCommandTests
    {
        readonly TestConsoleApplicationFactory _factory;

        public TestConsoleApplicationFactory Factory { get { return _factory; } }

        public BaseCommandTests()
        {
            _factory = new TestConsoleApplicationFactory();
        }
    }
}
