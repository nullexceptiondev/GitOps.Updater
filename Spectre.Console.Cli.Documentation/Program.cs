// See https://aka.ms/new-console-template for more information

using Spectre.Console.Cli.Documentation;

var xmlDocModel = XmlDocParser.Parse("D:\\dev\\github\\GitOps.Updater\\Spectre.Console.Cli.Documentation\\Sample.xml");

var markdown = XmlDocMarkdownBuilder.Build(xmlDocModel);

File.WriteAllText("d:\\cli.md", markdown);