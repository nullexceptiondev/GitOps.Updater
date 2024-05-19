
using System.Xml.Serialization;

namespace Spectre.Console.Cli.Documentation
{
    internal static class XmlDocParser
    {
        public static Model Parse(string filename)
        {
            Model model;
            var serializer = new XmlSerializer(typeof(Model));
            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state
                model = (Model)serializer.Deserialize(reader);
            }

            return model;
        }
    }
}
