using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions;
using System.IO.Abstractions;
using GitOps.Updater.Cli.Infrastructure;
using GitOps.Updater.Cli.Commands;
using System.Reflection;

namespace GitOps.Updater.Cli
{
    public class Startup : ConsoleStartup
    {
        public override IConfigurator ConfigureCommands(IConfigurator config)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            //config.SetApplicationName("GitOps Updater");
            config.SetApplicationVersion(version.ToString());
            config.AddBranch("image", image =>
            {
                image.AddCommand<UpdateImageCommand>("update")
                     .WithDescription("Update image tags")
                     .WithExample("image", "update", "-f", "c:\\file.csv");
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
