using Amalgam.Core.Configuration;

namespace Amalgam.Tests;

public class DirectoryScannerTests
{
    [Fact]
    public void Scan_EmptyDirectory_ReturnsEmptyConfig()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        try
        {
            var config = DirectoryScanner.Scan(tmpDir);
            Assert.Empty(config.Repositories);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Scan_DetectsMicroservice()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var repoDir = Path.Combine(tmpDir, "my-service");
        Directory.CreateDirectory(repoDir);
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));
        File.WriteAllText(Path.Combine(repoDir, "my-service.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(repoDir, "Program.cs"), "// entry point");
        try
        {
            var config = DirectoryScanner.Scan(tmpDir);

            Assert.Single(config.Repositories);
            Assert.Equal("my-service", config.Repositories[0].Name);
            Assert.Equal(RepositoryType.Microservice, config.Repositories[0].Type);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Scan_DetectsLibrary()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var repoDir = Path.Combine(tmpDir, "my-lib");
        Directory.CreateDirectory(repoDir);
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));
        File.WriteAllText(Path.Combine(repoDir, "my-lib.csproj"), "<Project />");
        try
        {
            var config = DirectoryScanner.Scan(tmpDir);

            Assert.Single(config.Repositories);
            Assert.Equal(RepositoryType.Library, config.Repositories[0].Type);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Scan_DetectsDashboard()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var repoDir = Path.Combine(tmpDir, "dashboard");
        Directory.CreateDirectory(repoDir);
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));
        File.WriteAllText(Path.Combine(repoDir, "package.json"), "{\"dependencies\":{\"react\":\"^18.0.0\"}}");
        try
        {
            var config = DirectoryScanner.Scan(tmpDir);

            Assert.Single(config.Repositories);
            Assert.Equal(RepositoryType.Dashboard, config.Repositories[0].Type);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Scan_DetectsPlugin()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var repoDir = Path.Combine(tmpDir, "my-plugin");
        Directory.CreateDirectory(repoDir);
        Directory.CreateDirectory(Path.Combine(repoDir, ".git"));
        File.WriteAllText(Path.Combine(repoDir, "package.json"), "{\"name\":\"my-plugin\"}");
        try
        {
            var config = DirectoryScanner.Scan(tmpDir);

            Assert.Single(config.Repositories);
            Assert.Equal(RepositoryType.Plugin, config.Repositories[0].Type);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Scan_SkipsNonGitDirectories()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var repoDir = Path.Combine(tmpDir, "not-a-repo");
        Directory.CreateDirectory(repoDir);
        File.WriteAllText(Path.Combine(repoDir, "something.csproj"), "<Project />");
        try
        {
            var config = DirectoryScanner.Scan(tmpDir);
            Assert.Empty(config.Repositories);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Scan_NonexistentDirectory_ReturnsEmptyConfig()
    {
        var config = DirectoryScanner.Scan("/nonexistent/path");
        Assert.Empty(config.Repositories);
    }
}
