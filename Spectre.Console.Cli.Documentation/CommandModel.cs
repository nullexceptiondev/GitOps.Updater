using System.Xml.Serialization;

namespace Spectre.Console.Cli.Documentation
{
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
