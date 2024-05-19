using Microsoft.Extensions.DependencyInjection;

namespace Spectre.Console.Cli.Extensions.Injection
{
    public static class CommandAppBuilderExtensions
    {
        /// <summary>
        /// Creates the type registrar based on the DependencyInjection ServiceCollection in CommandAppBuilder
        /// with optional pre-registered services.
        /// </summary>
        /// <param name="builder">CommandAppBuilder to extend.</param>
        /// <param name="services">
        ///     [Optional] Provide pre-registered services, or create new instance when null.
        /// </param>
        /// <returns>Returns the CommandAppBuilder</returns>
        public static CommandAppBuilder WithDIContainer(
            this CommandAppBuilder builder,
            IServiceCollection? services = null)
        {
            // if no pre-registered IServiceCollection specified, create a new empty instance.
            services ??= new ServiceCollection();

            builder.Registrar = new DependencyInjectionTypeRegistrar(services);
            return builder;
        }
    }
}
