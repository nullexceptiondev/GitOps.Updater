using gfs.YamlDotNet.YamlPath;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GitOps.Updater.Cli.Helpers
{
    public static class YamlHelper
    {

        public static async Task<YamlMappingNode?> ReadYamlFile(IFileSystem fileSystem, string path)
        {
            var tenantValuesFileContents = await fileSystem.File.ReadAllTextAsync(path);

            if (string.IsNullOrEmpty(tenantValuesFileContents)) return null;

            var yaml = new YamlStream();
            yaml.Load(new StringReader(tenantValuesFileContents));

            return (YamlMappingNode)yaml.Documents[0].RootNode;
        }

        public static string QueryYaml(YamlMappingNode rootNode, string yamlPath)
        {
            var yamlNodes = rootNode.Query(yamlPath);

            if (yamlNodes.Count() == 1 && yamlNodes.First() is YamlScalarNode scalarNode)
            {
                return scalarNode.Value;
            }

            return string.Empty;
        }

        public static string MergeYaml(string baseYaml, string overrideYaml)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new DictionaryTypeConverter())
                .Build();

            var baseDict = deserializer.Deserialize<Dictionary<string, object>>(baseYaml) ?? [];
            var overrideDict = deserializer.Deserialize<Dictionary<string, object>>(overrideYaml) ?? [];

            MergeDicts(baseDict, overrideDict);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return serializer.Serialize(baseDict);
        }

        private static void MergeDicts(Dictionary<string, object> baseDict, Dictionary<string, object> overrideDict)
        {
            foreach (var item in overrideDict)
            {
                if (baseDict.ContainsKey(item.Key) && baseDict[item.Key] is Dictionary<string, object> baseSubDict && item.Value is Dictionary<string, object> overrideSubDict)
                {
                    MergeDicts(baseSubDict, overrideSubDict);
                }
                else
                {
                    baseDict[item.Key] = item.Value;
                }
            }
        }
    }

    class DictionaryTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(Dictionary<object, object>);
        }

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer objectDeserializer)
        {
            var nestedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var dict = nestedDeserializer.Deserialize<Dictionary<string, object>>(parser);
            return dict;
        }

        [ExcludeFromCodeCoverage]
        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer objectSerializer)
        {
            throw new NotImplementedException();
        }
    }
}
