using Amalgam.Core.Configuration;
using Amalgam.Core.Merging;

namespace Amalgam.Tests.Merging;

public class FolderMergeServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FolderMergeService _service;

    public FolderMergeServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "amalgam-test-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _service = new FolderMergeService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Merge_CopiesFilesFromMultipleSources()
    {
        var repoPath = Path.Combine(_tempDir, "repo");
        var src1 = Path.Combine(repoPath, "src1");
        var src2 = Path.Combine(repoPath, "src2");
        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(src2);
        File.WriteAllText(Path.Combine(src1, "a.txt"), "file-a");
        File.WriteAllText(Path.Combine(src2, "b.txt"), "file-b");

        var repo = new RepositoryConfig
        {
            Name = "test-repo",
            Path = repoPath,
            Merge = new MergeConfig { Sources = new List<string> { "src1", "src2" } }
        };

        var result = _service.Merge(repo, _tempDir);

        Assert.Equal(2, result.MergedFileCount);
        Assert.True(File.Exists(Path.Combine(result.StagingPath, "a.txt")));
        Assert.True(File.Exists(Path.Combine(result.StagingPath, "b.txt")));
    }

    [Fact]
    public void Merge_OverlappingFiles_LastSourceWins()
    {
        var repoPath = Path.Combine(_tempDir, "repo");
        var src1 = Path.Combine(repoPath, "src1");
        var src2 = Path.Combine(repoPath, "src2");
        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(src2);
        File.WriteAllText(Path.Combine(src1, "shared.txt"), "from-src1");
        File.WriteAllText(Path.Combine(src2, "shared.txt"), "from-src2");

        var repo = new RepositoryConfig
        {
            Name = "test-repo",
            Path = repoPath,
            Merge = new MergeConfig { Sources = new List<string> { "src1", "src2" } }
        };

        var result = _service.Merge(repo, _tempDir);

        var content = File.ReadAllText(Path.Combine(result.StagingPath, "shared.txt"));
        Assert.Equal("from-src2", content);
    }

    [Fact]
    public void Merge_OverlappingFiles_ProducesWarning()
    {
        var repoPath = Path.Combine(_tempDir, "repo");
        var src1 = Path.Combine(repoPath, "src1");
        var src2 = Path.Combine(repoPath, "src2");
        Directory.CreateDirectory(src1);
        Directory.CreateDirectory(src2);
        File.WriteAllText(Path.Combine(src1, "shared.txt"), "from-src1");
        File.WriteAllText(Path.Combine(src2, "shared.txt"), "from-src2");

        var repo = new RepositoryConfig
        {
            Name = "test-repo",
            Path = repoPath,
            Merge = new MergeConfig { Sources = new List<string> { "src1", "src2" } }
        };

        var result = _service.Merge(repo, _tempDir);

        Assert.Single(result.Warnings);
        Assert.Contains("Overlapping", result.Warnings[0]);
    }

    [Fact]
    public void Merge_CopiesTargetToStagingRoot()
    {
        var repoPath = Path.Combine(_tempDir, "repo");
        var src1 = Path.Combine(repoPath, "src", "generated");
        Directory.CreateDirectory(src1);
        File.WriteAllText(Path.Combine(src1, "MyProject.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(src1, "Class1.cs"), "class1");

        var repo = new RepositoryConfig
        {
            Name = "test-repo",
            Path = repoPath,
            Merge = new MergeConfig
            {
                Sources = new List<string> { "src/generated" },
                Target = "src/generated/MyProject.csproj"
            }
        };

        var result = _service.Merge(repo, _tempDir);

        Assert.True(File.Exists(Path.Combine(result.StagingPath, "MyProject.csproj")));
    }

    [Fact]
    public void Merge_NoMergeConfig_ReturnRepoPath()
    {
        var repo = new RepositoryConfig
        {
            Name = "test-repo",
            Path = "/some/path",
            Merge = null
        };

        var result = _service.Merge(repo, _tempDir);

        Assert.Equal("/some/path", result.StagingPath);
        Assert.Equal(0, result.MergedFileCount);
    }

    [Fact]
    public void CleanAll_RemovesMergedDirectory()
    {
        var mergedDir = Path.Combine(_tempDir, ".amalgam", "merged", "test");
        Directory.CreateDirectory(mergedDir);
        File.WriteAllText(Path.Combine(mergedDir, "file.txt"), "content");

        _service.CleanAll(_tempDir);

        Assert.False(Directory.Exists(Path.Combine(_tempDir, ".amalgam", "merged")));
    }

    [Fact]
    public void CleanAll_NoDirectory_DoesNotThrow()
    {
        var exception = Record.Exception(() => _service.CleanAll(_tempDir));
        Assert.Null(exception);
    }

    [Fact]
    public void GetEffectivePath_WithMerge_ReturnsStagingPath()
    {
        var repo = new RepositoryConfig
        {
            Name = "test-repo",
            Path = "/original/path",
            Merge = new MergeConfig { Sources = new List<string> { "src1" } }
        };

        var effectivePath = _service.GetEffectivePath(repo, _tempDir);

        Assert.Equal(Path.Combine(_tempDir, ".amalgam", "merged", "test-repo"), effectivePath);
    }

    [Fact]
    public void GetEffectivePath_WithoutMerge_ReturnsRepoPath()
    {
        var repo = new RepositoryConfig
        {
            Name = "test-repo",
            Path = "/original/path",
            Merge = null
        };

        var effectivePath = _service.GetEffectivePath(repo, _tempDir);

        Assert.Equal("/original/path", effectivePath);
    }
}
