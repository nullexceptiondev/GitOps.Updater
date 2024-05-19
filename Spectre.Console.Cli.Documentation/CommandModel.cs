using System.Xml.Serialization;

namespace Spectre.Console.Cli.Documentation
{
    //[Serializable]
    //public class Model
    //{
    //    [XmlArray(elementName: "Command")]
    //    public Command[] Commands { get; set; }
    //}

    //[Serializable]
    //public class Command
    //{
    //    [XmlAttribute]
    //    public string Name { get; set; }
    //    [XmlAttribute]
    //    public string Description { get; set; }
    //    [XmlAttribute]
    //    public bool IsBranch { get; set; }
    //    [XmlAttribute]
    //    public bool IsDefault { get; set; }

    //    [XmlElement]
    //    public Parameters Parameters { get; set; }

    //    [XmlArray(elementName: "Command")]
    //    public Command[] Commands { get; set; }
    //}

    //public class Parameters
    //{
    //    [XmlArray(elementName: "Argument")]
    //    public CommandArgument[] Arguments { get; set; }

    //    [XmlArray(elementName: "Option")]
    //    public CommandOption[] Options { get; set; }
    //}

    //[Serializable]
    //public class CommandArgument
    //{
    //    [XmlAttribute]
    //    public string Name { get; set; }
    //    [XmlAttribute]
    //    public string Description { get; set; }
    //    [XmlAttribute]
    //    public int Position { get; set; }
    //    [XmlAttribute]
    //    public bool Required { get; set; }
    //    [XmlAttribute]
    //    public string Kind { get; set; }
    //    [XmlAttribute]
    //    public string ClrType {get; set;}
    //}

    //[Serializable]
    //public class CommandOption
    //{
    //    [XmlAttribute]
    //    public bool Shadowed { get; set; }
    //    [XmlAttribute]
    //    public string Short { get; set; }
    //    [XmlAttribute]
    //    public string Long { get; set; }
    //    [XmlAttribute]
    //    public string Value { get; set; }
    //    [XmlAttribute]
    //    public bool Required { get; set; }
    //    [XmlAttribute]
    //    public string Kind { get; set; }
    //    [XmlAttribute]
    //    public string ClrType { get; set; }
    //    [XmlAttribute]
    //    public string Description { get; set; }
    //}

    //[Serializable]
    //public class Example
    //{
    //    [XmlAttribute("commandLine")]
    //    public string CommandLine { get; set; }
    //}

    [XmlRoot(ElementName = "Model")]
    public class Model
    {
        [XmlElement(ElementName = "Command")]
        public List<Command> Commands { get; set; }
    }

    [XmlRoot(ElementName = "Parameters")]
    public class Parameters
    {
        [XmlElement(ElementName = "Option")]
        public List<Option> Options { get; set; }

        [XmlElement(ElementName = "Argument")]
        public List<Argument> Arguments { get; set; }
    }

    [XmlRoot(ElementName = "Command")]
    public class Command
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "IsBranch")]
        public bool IsBranch { get; set; }

        [XmlAttribute(AttributeName = "ClrType")]
        public string ClrType { get; set; }

        [XmlAttribute(AttributeName = "Settings")]
        public string Settings { get; set; }

        [XmlElement(ElementName = "Parameters")]
        public Parameters Parameters { get; set; }

        [XmlElement(ElementName = "Command")]
        public List<Command> Commands { get; set; }
    }

    [XmlRoot(ElementName = "Option")]
    public class Option
    {
        [XmlAttribute(AttributeName = "Shadowed")]
        public bool Shadowed { get; set; }

        [XmlAttribute(AttributeName = "Short")]
        public string Short { get; set; }

        [XmlAttribute(AttributeName = "Long")]
        public string Long { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public string Value { get; set; }

        [XmlAttribute(AttributeName = "Required")]
        public bool Required { get; set; }

        [XmlAttribute(AttributeName = "Kind")]
        public string Kind { get; set; }

        [XmlAttribute(AttributeName = "ClrType")]
        public string ClrType { get; set; }

        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
    }

    public class Argument
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "Position")]
        public int Position { get; set; }

        [XmlAttribute(AttributeName = "Required")]
        public bool Required { get; set; }

        [XmlAttribute(AttributeName = "Kind")]
        public string Kind { get; set; }

        [XmlAttribute(AttributeName = "ClrType")]
        public string ClrType { get; set; }

        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
    }
}
