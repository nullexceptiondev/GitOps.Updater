using System.Diagnostics.CodeAnalysis;
using GitOps.Updater.Cli;
using Spectre.Console;
using Spectre.Console.Cli.Extensions;
using Spectre.Console.Cli.Extensions.Injection;

var commandApp = new CommandAppBuilder()
                        .WithStartup<Startup>()
                        .WithDIContainer()
                        .Build();

try
{
    return await commandApp.RunAsync(args);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return -1;
}

[ExcludeFromCodeCoverage]
public partial class Program { }