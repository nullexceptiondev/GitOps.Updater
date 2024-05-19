using Spectre.Console.Cli.Extensions.Injection;

namespace Spectre.Console.Cli.Extensions.Testing
{
    public class ConsoleApplicationFactory<TEntryPoint> : IDisposable where TEntryPoint : ConsoleStartup, new()
    {
        //private bool _disposed;
        //private bool _disposedAsync;
        private CommandApp _commandApp;
        private CommandAppTester _commandAppTester;
        private ITypeResolver? _typeResolver;

        public CommandAppTester CommandAppTester
        {
            get
            { 
                EnsureCommandApp();
                return _commandAppTester;
            }
        }

        public object? ResolveService(Type type)
        {
            EnsureCommandApp();
            return _typeResolver?.Resolve(type);
        }

        public TInterface? ResolveService<TInterface>()
        {
            EnsureCommandApp();
            return (TInterface?)_typeResolver?.Resolve(typeof(TInterface));
        }

        private void EnsureCommandApp()
        {
            if (_commandAppTester != null)
            {
                return;
            }

            var commandAppBuilder = new CommandAppBuilder().WithDIContainer().WithStartup<TEntryPoint>().IsUnitTest();
           
            ConfigureTestServices(commandAppBuilder.Registrar); //Register any test services before the build

            commandAppBuilder = commandAppBuilder.Build();

            _typeResolver = commandAppBuilder?.Registrar?.Build();
            _commandApp = commandAppBuilder?.App;
            _commandAppTester = new CommandAppTester(_commandApp);
        }

        protected virtual void ConfigureTestServices(ITypeRegistrar registrar)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
    }
}
