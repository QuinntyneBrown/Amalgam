using System.Text.Json;

namespace Amalgam.Core.Plugins;

public static class PackageJsonParser
{
    /// <summary>
    /// Reads a package.json file and returns the "name" field.
    /// </summary>
    public static string? GetPackageName(string packageJsonPath)
    {
        var json = File.ReadAllText(packageJsonPath);
        return GetPackageNameFromJson(json);
    }

    /// <summary>
    /// Parses JSON content and returns the "name" field.
    /// </summary>
    public static string? GetPackageNameFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("name", out var nameProp))
        {
            return nameProp.GetString();
        }
        return null;
    }
}
