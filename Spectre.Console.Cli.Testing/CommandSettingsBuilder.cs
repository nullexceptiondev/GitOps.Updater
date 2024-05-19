//using Spectre.Console.Cli.Help;
//using System.Reflection;
//using System.Reflection.Metadata;

//namespace Spectre.Console.Cli.Testing
//{
//    public class CommandSettingsBuilder
//    {
//        public static string[] BuildExample<TCommandSettings>() where TCommandSettings : CommandSettings
//        {
//            var properties = typeof(TCommandSettings).GetProperties();

//            var commandOptions = properties.Select(p => p.GetCustomAttribute<CommandOptionAttribute>(true)).Where(opt => opt is not null);
//            var commandArguments = properties.Select(p => p.GetCustomAttribute<CommandArgumentAttribute>(true)).Where(arg => arg is not null).OrderBy(arg => arg.Position);
//            var args = new List<string>();

//            foreach (var argument in commandArguments) 
//            {
//            }

//            foreach(var option in commandOptions)
//            {
//                args.Add($"-{GetOptionName(option)}");
//                args.Add(GetValue(null));
//            }

//            //if (parameter is CommandOption commandOptionParameter)
//            //{
//            //    if (commandOptionParameter.IsShadowed)
//            //    {
//            //        parameterNode.AddNode(ValueMarkup("IsShadowed", commandOptionParameter.IsShadowed.ToString()));
//            //    }

//            //    if (commandOptionParameter.LongNames.Count > 0)
//            //    {
//            //        parameterNode.AddNode(ValueMarkup(
//            //            "Long Names",
//            //            string.Join("|", commandOptionParameter.LongNames.Select(i => $"--{i}"))));

//            //        parameterNode.AddNode(ValueMarkup(
//            //            "Short Names",
//            //            string.Join("|", commandOptionParameter.ShortNames.Select(i => $"-{i}"))));
//            //    }
//            //}
//            //else if (parameter is CommandArgument commandArgumentParameter)
//            //{
//            //    parameterNode.AddNode(ValueMarkup("Position", commandArgumentParameter.Position.ToString()));
//            //    parameterNode.AddNode(ValueMarkup("Value", commandArgumentParameter.Value));
//            //}

//            return args.ToArray();
//        }

//        private static string GetOptionName(CommandOptionAttribute option)
//        {
//            if (option.ShortNames.Count > 0) return option.ShortNames[0];
//            if (option.LongNames.Count > 0) return option.LongNames[0];

//            return "";
//        }

//        public static string GetValue(Type type)
//        {
//            return "";
//        }
//    }
//}
