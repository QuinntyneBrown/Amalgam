using Amalgam.Core.Configuration;

namespace Amalgam.Tests;

public class ConfigValidatorTests
{
    [Fact]
    public void Validate_MissingPath_ReturnsError()
    {
        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "svc", Type = RepositoryType.Library, Path = "/nonexistent/path" }
            }
        };

        var errors = ConfigValidator.Validate(config);

        Assert.Single(errors);
        Assert.Contains("path does not exist", errors[0]);
    }

    [Fact]
    public void Validate_DuplicateNames_ReturnsError()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        try
        {
            var config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new() { Name = "svc", Type = RepositoryType.Library, Path = tmpDir },
                    new() { Name = "svc", Type = RepositoryType.Library, Path = tmpDir }
                }
            };

            var errors = ConfigValidator.Validate(config);

            Assert.Contains(errors, e => e.Contains("Duplicate repository name"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Validate_MultipleDashboards_ReturnsError()
    {
        var tmpDir1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tmpDir2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir1);
        Directory.CreateDirectory(tmpDir2);
        try
        {
            var config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new() { Name = "dash1", Type = RepositoryType.Dashboard, Path = tmpDir1 },
                    new() { Name = "dash2", Type = RepositoryType.Dashboard, Path = tmpDir2 }
                }
            };

            var errors = ConfigValidator.Validate(config);

            Assert.Contains(errors, e => e.Contains("At most one dashboard"));
        }
        finally
        {
            Directory.Delete(tmpDir1, true);
            Directory.Delete(tmpDir2, true);
        }
    }

    [Fact]
    public void Validate_MicroserviceMissingCsproj_ReturnsError()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        try
        {
            var config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new() { Name = "svc", Type = RepositoryType.Microservice, Path = tmpDir }
                }
            };

            var errors = ConfigValidator.Validate(config);

            Assert.Contains(errors, e => e.Contains("no .csproj file found"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Validate_ReturnsAllErrors_NotJustFirst()
    {
        var config = new AmalgamConfig
        {
            Repositories = new List<RepositoryConfig>
            {
                new() { Name = "a", Type = RepositoryType.Library, Path = "/nonexistent1" },
                new() { Name = "a", Type = RepositoryType.Library, Path = "/nonexistent2" },
                new() { Name = "b", Type = RepositoryType.Dashboard, Path = "/nonexistent3" },
                new() { Name = "c", Type = RepositoryType.Dashboard, Path = "/nonexistent4" }
            }
        };

        var errors = ConfigValidator.Validate(config);

        // Should have path errors + duplicate name + multiple dashboards
        Assert.True(errors.Count >= 3, $"Expected at least 3 errors, got {errors.Count}: {string.Join("; ", errors)}");
    }

    [Fact]
    public void Validate_ValidConfig_NoErrors()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        File.WriteAllText(Path.Combine(tmpDir, "svc.csproj"), "<Project />");
        try
        {
            var config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new() { Name = "svc", Type = RepositoryType.Microservice, Path = tmpDir }
                }
            };

            var errors = ConfigValidator.Validate(config);

            Assert.Empty(errors);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Validate_MergeEmptySources_ReturnsError()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        try
        {
            var config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new()
                    {
                        Name = "svc", Type = RepositoryType.Library, Path = tmpDir,
                        Merge = new MergeConfig { Sources = new List<string>() }
                    }
                }
            };

            var errors = ConfigValidator.Validate(config);

            Assert.Contains(errors, e => e.Contains("merge config has no sources"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Validate_MergeMissingSource_ReturnsError()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tmpDir);
        try
        {
            var config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new()
                    {
                        Name = "svc", Type = RepositoryType.Library, Path = tmpDir,
                        Merge = new MergeConfig { Sources = new List<string> { "nonexistent" } }
                    }
                }
            };

            var errors = ConfigValidator.Validate(config);

            Assert.Contains(errors, e => e.Contains("merge source directory does not exist"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public void Validate_MergeValidSources_NoError()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var srcDir = Path.Combine(tmpDir, "src1");
        Directory.CreateDirectory(srcDir);
        try
        {
            var config = new AmalgamConfig
            {
                Repositories = new List<RepositoryConfig>
                {
                    new()
                    {
                        Name = "svc", Type = RepositoryType.Library, Path = tmpDir,
                        Merge = new MergeConfig { Sources = new List<string> { "src1" } }
                    }
                }
            };

            var errors = ConfigValidator.Validate(config);

            Assert.DoesNotContain(errors, e => e.Contains("merge"));
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }
}
