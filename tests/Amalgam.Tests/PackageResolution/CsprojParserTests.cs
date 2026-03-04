using System.Xml.Linq;
using Amalgam.Core.PackageResolution;

namespace Amalgam.Tests.PackageResolution;

public class CsprojParserTests
{
    [Fact]
    public void ParseXml_ExtractsPackageId()
    {
        var xml = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <PackageId>My.Custom.Package</PackageId>
                <AssemblyName>MyAssembly</AssemblyName>
              </PropertyGroup>
            </Project>
            """);

        var info = CsprojParser.ParseXml(xml);

        Assert.Equal("My.Custom.Package", info.PackageId);
        Assert.Equal("MyAssembly", info.AssemblyName);
        Assert.Equal("My.Custom.Package", info.GetPackageName());
    }

    [Fact]
    public void ParseXml_FallsBackToAssemblyName()
    {
        var xml = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <AssemblyName>MyLib</AssemblyName>
              </PropertyGroup>
            </Project>
            """);

        var info = CsprojParser.ParseXml(xml);

        Assert.Null(info.PackageId);
        Assert.Equal("MyLib", info.GetPackageName());
    }

    [Fact]
    public void ParseXml_ExtractsPackageReferences()
    {
        var xml = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
                <PackageReference Include="Serilog" Version="3.0.0" />
              </ItemGroup>
            </Project>
            """);

        var info = CsprojParser.ParseXml(xml);

        Assert.Equal(2, info.PackageReferences.Count);
        Assert.Equal("Newtonsoft.Json", info.PackageReferences[0].Name);
        Assert.Equal("13.0.1", info.PackageReferences[0].Version);
        Assert.Equal("Serilog", info.PackageReferences[1].Name);
    }

    [Fact]
    public void ParseXml_ReturnsNullPackageName_WhenNeitherSet()
    {
        var xml = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var info = CsprojParser.ParseXml(xml);

        Assert.Null(info.GetPackageName());
    }
}
