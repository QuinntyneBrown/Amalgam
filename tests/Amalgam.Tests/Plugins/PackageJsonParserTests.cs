using Amalgam.Core.Plugins;

namespace Amalgam.Tests.Plugins;

public class PackageJsonParserTests
{
    [Fact]
    public void GetPackageNameFromJson_ReturnsName()
    {
        var json = """
            {
              "name": "@acme/my-plugin",
              "version": "1.0.0"
            }
            """;

        var name = PackageJsonParser.GetPackageNameFromJson(json);

        Assert.Equal("@acme/my-plugin", name);
    }

    [Fact]
    public void GetPackageNameFromJson_ReturnsNull_WhenNoName()
    {
        var json = """{ "version": "1.0.0" }""";

        var name = PackageJsonParser.GetPackageNameFromJson(json);

        Assert.Null(name);
    }

    [Fact]
    public void GetPackageName_ReadsFile()
    {
        var tmpFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tmpFile, """{ "name": "test-pkg" }""");
            var name = PackageJsonParser.GetPackageName(tmpFile);
            Assert.Equal("test-pkg", name);
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }
}
