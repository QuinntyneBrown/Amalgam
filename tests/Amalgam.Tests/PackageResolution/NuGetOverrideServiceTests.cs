using Amalgam.Core.Configuration;
using Amalgam.Core.PackageResolution;

namespace Amalgam.Tests.PackageResolution;

public class NuGetOverrideServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly NuGetOverrideService _service;

    public NuGetOverrideServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "amalgam-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _service = new NuGetOverrideService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void GenerateOverrides_CreatesPropsForMatchingReferences()
    {
        // Arrange: a library with PackageId and a microservice referencing it
        var libDir = CreateRepo("MyLib", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <PackageId>Acme.MyLib</PackageId>
              </PropertyGroup>
            </Project>
            """);

        var svcDir = CreateRepo("MySvc", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Acme.MyLib" Version="1.0.0" />
                <PackageReference Include="Unrelated.Pkg" Version="2.0.0" />
              </ItemGroup>
            </Project>
            """);

        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "MyLib", Type = RepositoryType.Library, Path = libDir },
                new() { Name = "MySvc", Type = RepositoryType.Microservice, Path = svcDir }
            }
        };

        // Act
        var files = _service.GenerateOverrides(config);

        // Assert
        Assert.True(files.Count >= 2);

        var amalgamProps = Path.Combine(svcDir, ".amalgam", "Directory.Build.props");
        Assert.True(File.Exists(amalgamProps));

        var content = File.ReadAllText(amalgamProps);
        Assert.Contains("PackageReference Remove=\"Acme.MyLib\"", content);
        Assert.Contains("ProjectReference Include=", content);
        Assert.DoesNotContain("Unrelated.Pkg", content);

        // Root props should import .amalgam
        var rootProps = Path.Combine(svcDir, "Directory.Build.props");
        Assert.True(File.Exists(rootProps));
        var rootContent = File.ReadAllText(rootProps);
        Assert.Contains(".amalgam/Directory.Build.props", rootContent);
    }

    [Fact]
    public void GenerateOverrides_DoesNothing_WhenNoMatchingReferences()
    {
        var libDir = CreateRepo("MyLib", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><PackageId>Acme.MyLib</PackageId></PropertyGroup>
            </Project>
            """);

        var svcDir = CreateRepo("MySvc", """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="SomeOther.Pkg" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);

        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "MyLib", Type = RepositoryType.Library, Path = libDir },
                new() { Name = "MySvc", Type = RepositoryType.Microservice, Path = svcDir }
            }
        };

        var files = _service.GenerateOverrides(config);

        Assert.Empty(files);
        Assert.False(Directory.Exists(Path.Combine(svcDir, ".amalgam")));
    }

    [Fact]
    public void CleanOverrides_RemovesAmalgamDirectories()
    {
        var svcDir = CreateRepo("MySvc", "<Project />");
        var amalgamDir = Path.Combine(svcDir, ".amalgam");
        Directory.CreateDirectory(amalgamDir);
        File.WriteAllText(Path.Combine(amalgamDir, "Directory.Build.props"), "<Project />");

        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "MySvc", Type = RepositoryType.Microservice, Path = svcDir }
            }
        };

        _service.CleanOverrides(config);

        Assert.False(Directory.Exists(amalgamDir));
    }

    private string CreateRepo(string name, string csprojContent)
    {
        var dir = Path.Combine(_tempDir, name);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, $"{name}.csproj"), csprojContent);
        return dir;
    }
}
