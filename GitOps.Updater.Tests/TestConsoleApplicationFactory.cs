using GitOps.Updater.Cli;
using GitOps.Updater.Cli.Infrastructure;
using GitOps.Updater.Tests.Infrastructure;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.Testing;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace GitOps.Updater.Tests
{
    public class TestConsoleApplicationFactory : ConsoleApplicationFactory<Startup>
    {
        protected override void ConfigureTestServices(ITypeRegistrar registrar)
        {
            var fileSystem = new MockFileSystem();
            registrar.RegisterInstance(typeof(IFileSystem), fileSystem);
            registrar.RegisterInstance(typeof(GitClient), new FakeGitClient(fileSystem));
        }
    }
}
