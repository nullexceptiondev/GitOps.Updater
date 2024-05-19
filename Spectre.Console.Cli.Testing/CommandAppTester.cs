using Spectre.Console.Testing;

namespace Spectre.Console.Cli.Extensions.Testing
{
    public sealed class CommandAppTester
    {
        private CommandApp _commandApp;

        public CommandApp CommandApp => _commandApp;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAppTester"/> class.
        /// </summary>
        /// <param name="registrar">The registrar.</param>
        public CommandAppTester(CommandApp commandApp)
        {
            _commandApp = commandApp;
        }

        /// <summary>
        /// Runs the command application and expects an exception of a specific type to be thrown.
        /// </summary>
        /// <typeparam name="T">The expected exception type.</typeparam>
        /// <param name="args">The arguments.</param>
        /// <returns>The information about the failure.</returns>
        public CommandAppFailure RunAndCatch<T>(params string[] args)
            where T : Exception
        {
            var console = new TestConsole().Width(int.MaxValue);

            try
            {
                Run(args, console, c => c.PropagateExceptions());
                throw new InvalidOperationException("Expected an exception to be thrown, but there was none.");
            }
            catch (T ex)
            {
                if (ex is CommandAppException commandAppException && commandAppException.Pretty != null)
                {
                    console.Write(commandAppException.Pretty);
                }
                else
                {
                    console.WriteLine(ex.Message);
                }

                return new CommandAppFailure(ex, console.Output);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Expected an exception of type '{typeof(T).FullName}' to be thrown, "
                    + $"but received {ex.GetType().FullName}.");
            }
        }

        /// <summary>
        /// Runs the command application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The result.</returns>
        public CommandAppResult Run(params string[] args)
        {
            var console = new TestConsole().Width(int.MaxValue);
            return Run(args, console);
        }

        private CommandAppResult Run(string[] args, TestConsole console, Action<IConfigurator>? config = null)
        {
            CommandContext? context = null;
            CommandSettings? settings = null;

            _commandApp.Configure(c => c.ConfigureConsole(console));
            _commandApp.Configure(c => c.SetInterceptor(new CallbackCommandInterceptor((ctx, s) =>
            {
                context = ctx;
                settings = s;
            })));

            AnsiConsole.Console = console;

            var result = _commandApp.Run(args);

            var output = console.Output
                .NormalizeLineEndings()
                .TrimLines()
                .Trim();

            return new CommandAppResult(result, output, context, settings);
        }

        //public string[] GenerateTestArguments<TSettings>(params string[] ignoreArguments) where TSettings : CommandSettings
        //{
        //    var properties = typeof(TSettings).GetProperties();
        //    var args = properties.Select(p => $"--{p.Name.ToLower()} <value>").ToList();
        //}
    }
}
