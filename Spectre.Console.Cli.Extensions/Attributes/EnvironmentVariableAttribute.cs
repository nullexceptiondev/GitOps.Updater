namespace Spectre.Console.Cli.Extensions
{
    public sealed class EnvironmentVariableAttribute : ParameterValueProviderAttribute
    {
        private readonly string _name;

        public EnvironmentVariableAttribute(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool TryGetValue(CommandParameterContext context, out object? result)
        {
            // Parameter is a string?
            if (context.Parameter.ParameterType == typeof(string))
            {
                // No value provided?
                if (context.Value == null)
                {
                    result = Environment.GetEnvironmentVariable(_name);
                    return result != null;
                }
            }
            else if (context.Parameter.ParameterType == typeof(Boolean))
            {
                // No value provided?
                if (context.Value == null)
                {
                    result = false;
                    var variable = Environment.GetEnvironmentVariable(_name);
                    if (!string.IsNullOrEmpty(variable))
                    {
                        var trueValues = new List<String> { "Y", "Yes", "True", "1", "T" };
                        result = trueValues.Contains(variable, StringComparer.InvariantCultureIgnoreCase);
                    }

                    return result != null;
                }
            }

            result = null;
            return false;
        }
    }
}
