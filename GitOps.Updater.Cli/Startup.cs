using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions;
using System.IO.Abstractions;
using GitOps.Updater.Cli.Infrastructure;
using GitOps.Updater.Cli.Commands;

namespace GitOps.Updater.Cli
{
    public class Startup : ConsoleStartup
    {
        public override IConfigurator ConfigureCommands(IConfigurator config)
        {
            config.AddBranch("image", image =>
            {
                image.AddCommand<UpdateImageCommand>("update");
            });

            return config;
        }

        public override void ConfigureServices(ITypeRegistrar typeRegistrar, bool isUnitTest)
        {
            typeRegistrar.RegisterInstance(typeof(IFileSystem), new FileSystem());
            typeRegistrar.Register(typeof(GitClient), typeof(LocalGitClient));
        }
    }
}
