namespace Spectre.Console.Cli.Extensions
{
    public class CommandAppBuilder
    {
        private bool _isUnitTest = false;

        public CommandApp? App { get; set; } = null;
        public ITypeRegistrar? Registrar { get; set; }
        internal ConsoleStartup? Startup { get; set; }

        /// <summary>
        /// Sets up the Startup class to use in this builder.
        /// </summary>
        /// <typeparam name="TStartup">Type of the Startup class in this project,
        /// which must derive from StartupBase.</typeparam>
        /// <returns>Returns the CommandAppBuilder.</returns>
        public CommandAppBuilder WithStartup<TStartup>()
            where TStartup : ConsoleStartup, new()
        {
            // Create the startup class instance from the app project.
            Startup = new TStartup();
            return this;
        }

        public CommandAppBuilder IsUnitTest()
        {
            _isUnitTest = true; 
            return this;
        }

        /// <summary>
        /// Builds the CommandApp encapsulated by this builder. It ensures that the
        /// application services are configured, the CommandApp is created with the
        /// type registrar, and the app Commands are configured.
        /// </summary>
        /// <returns>Returns the CommandAppBuilder.</returns>
        public CommandAppBuilder Build()
        {
            ArgumentNullException.ThrowIfNull(Startup);

            if (Registrar != null)
            {
                // Configure all services with this DI framework.
                Startup.ConfigureServices(Registrar, _isUnitTest);
            }

            // Create the CommandApp with the type registrar.
            App = new CommandApp(Registrar);

            // If a default command was specified, then add it to the CommandApp now.
            //SetDefaultCommand?.Invoke();

            // Configure any commands in the application.
            App.Configure(config => Startup.ConfigureCommands(config));

            // Configure any custom set configuration if it's available.
            //SetCustomConfig?.Invoke();

            return this;
        }

        /// <summary>
        /// Runs the CommandApp asynchronously.
        /// </summary>
        /// <param name="args">Command line arguments run with.</param>
        /// <returns>Return value from the application.</returns>
        public async Task<int> RunAsync(string[] args)
        {
            _ = App ?? throw new ArgumentNullException(
                nameof(App), "Build was not called prior to calling RunAsync.");

            return await App.RunAsync(args);
        }

        /// <summary>
        /// Runs the CommandApp.
        /// </summary>
        /// <param name="args">Command line arguments run with.</param>
        /// <returns>Return value from the application.</returns>
        public int Run(string[] args)
        {
            _ = App ?? throw new ArgumentNullException(
                nameof(App), "Build was not called prior to calling Run.");

            return App.Run(args);
        }
    }
}
