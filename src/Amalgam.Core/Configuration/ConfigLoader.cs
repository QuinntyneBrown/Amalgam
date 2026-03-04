using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Amalgam.Core.Configuration;

public class ConfigLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    public static AmalgamConfig Load(string filePath)
    {
        var yaml = File.ReadAllText(filePath);
        return Parse(yaml);
    }

    public static AmalgamConfig Parse(string yaml)
    {
        var config = Deserializer.Deserialize<AmalgamConfig>(yaml);
        return config ?? new AmalgamConfig();
    }

    public static string Serialize(AmalgamConfig config)
    {
        return Serializer.Serialize(config);
    }

    public static void Save(AmalgamConfig config, string filePath)
    {
        var yaml = Serialize(config);
        File.WriteAllText(filePath, yaml);
    }
}
