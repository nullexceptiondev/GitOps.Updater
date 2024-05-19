using DownMark;
using DownMark.Extensions;
using System.Data;

namespace Spectre.Console.Cli.Documentation
{
    internal static class XmlDocMarkdownBuilder
    {
        public static string Build(Model model)
        {
            var builder = new MarkdownBuilder();

            CreateCommands(builder, model.Commands);

            return builder.Build();
        }

        private static void CreateCommands(MarkdownBuilder builder, List<Command> commands)
        {
            foreach (var command in commands)
            {
                if (!command.IsBranch)
                {
                    CreateCommand(builder, command);
                }
                else
                {
                    builder = builder.Header(command.Name, DownMark.Models.HeaderSize.H1);

                    CreateCommands(builder, command.Commands);
                }
            }
        }

        private static void CreateCommand(MarkdownBuilder builder, Command command)
        {
            builder.Header(command.Name, DownMark.Models.HeaderSize.H2);

            if (command.Parameters.Arguments.Count > 0)
            {
                builder.Header("Arguments", DownMark.Models.HeaderSize.H3);

                var argumentsTable = new DataTable();

                argumentsTable.Columns.Add("Position");
                argumentsTable.Columns.Add("Name");

                foreach (var argument in command.Parameters.Arguments)
                {
                    argumentsTable.Rows.Add(argument.Position, argument.Name);
                }

                builder.Table(argumentsTable);
            }

            if (command.Parameters.Options.Count > 0)
            {
                builder.Header("Options", DownMark.Models.HeaderSize.H3);

                var optionsTable = new DataTable();

                optionsTable.Columns.Add("Short");
                optionsTable.Columns.Add("Long");
                optionsTable.Columns.Add("Description");
                optionsTable.Columns.Add("Required?");

                foreach (var option in command.Parameters.Options)
                {
                    optionsTable.Rows.Add(option.Short, option.Long, option.Description, option.Required);
                }

                builder.Table(optionsTable);
            }
        }
    }
}
