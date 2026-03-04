using System.Xml.Linq;

namespace Amalgam.Core.PackageResolution;

public class CsprojInfo
{
    public string? PackageId { get; set; }
    public string? AssemblyName { get; set; }
    public List<PackageRef> PackageReferences { get; set; } = new();

    /// <summary>
    /// Returns the NuGet package name: PackageId if set, otherwise AssemblyName, otherwise null.
    /// </summary>
    public string? GetPackageName() => PackageId ?? AssemblyName;
}

public class PackageRef
{
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
}

public static class CsprojParser
{
    public static CsprojInfo Parse(string csprojPath)
    {
        var doc = XDocument.Load(csprojPath);
        return ParseXml(doc);
    }

    public static CsprojInfo ParseXml(XDocument doc)
    {
        var info = new CsprojInfo();

        // Look in all PropertyGroup elements for PackageId and AssemblyName
        foreach (var pg in doc.Descendants("PropertyGroup"))
        {
            var packageId = pg.Element("PackageId")?.Value;
            if (packageId != null)
                info.PackageId = packageId;

            var assemblyName = pg.Element("AssemblyName")?.Value;
            if (assemblyName != null)
                info.AssemblyName = assemblyName;
        }

        // Collect all PackageReference elements
        foreach (var pr in doc.Descendants("PackageReference"))
        {
            var include = pr.Attribute("Include")?.Value;
            if (include == null) continue;

            info.PackageReferences.Add(new PackageRef
            {
                Name = include,
                Version = pr.Attribute("Version")?.Value ?? pr.Element("Version")?.Value
            });
        }

        return info;
    }
}
