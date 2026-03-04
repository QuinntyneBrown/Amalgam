using Amalgam.Core.Configuration;

namespace Amalgam.Core.Merging;

public class FolderMergeService
{
    public MergeResult Merge(RepositoryConfig repo, string workingDirectory)
    {
        var result = new MergeResult();

        if (repo.Merge == null || repo.Merge.Sources.Count == 0)
        {
            result.StagingPath = repo.Path;
            return result;
        }

        var stagingPath = Path.Combine(workingDirectory, ".amalgam", "merged", repo.Name);
        result.StagingPath = stagingPath;

        // Clear staging dir if it already exists
        if (Directory.Exists(stagingPath))
            Directory.Delete(stagingPath, true);

        Directory.CreateDirectory(stagingPath);

        var seenFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in repo.Merge.Sources)
        {
            var sourcePath = Path.Combine(repo.Path, source);
            if (!Directory.Exists(sourcePath))
                continue;

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(sourcePath, file);
                var destPath = Path.Combine(stagingPath, relativePath);

                if (seenFiles.Contains(relativePath))
                {
                    result.Warnings.Add($"Overlapping file '{relativePath}' from source '{source}' overwrites previous version");
                }
                seenFiles.Add(relativePath);

                var destDir = Path.GetDirectoryName(destPath);
                if (destDir != null)
                    Directory.CreateDirectory(destDir);

                File.Copy(file, destPath, true);
                result.MergedFileCount++;
            }
        }

        // If Target is set, copy the target file to staging root
        if (repo.Merge.Target != null)
        {
            string? targetFullPath = null;

            foreach (var source in repo.Merge.Sources)
            {
                var candidate = Path.Combine(repo.Path, source);
                // Check if the target is within this source (relative to repo.Path)
                var targetRelativeToRepo = repo.Merge.Target;
                var targetAbsolute = Path.Combine(repo.Path, targetRelativeToRepo);

                if (File.Exists(targetAbsolute) && targetAbsolute.StartsWith(Path.GetFullPath(candidate), StringComparison.OrdinalIgnoreCase))
                {
                    targetFullPath = targetAbsolute;
                    break;
                }
            }

            if (targetFullPath == null)
            {
                // Try direct path relative to repo
                var directPath = Path.Combine(repo.Path, repo.Merge.Target);
                if (File.Exists(directPath))
                    targetFullPath = directPath;
            }

            if (targetFullPath != null)
            {
                var targetFileName = Path.GetFileName(targetFullPath);
                var destPath = Path.Combine(stagingPath, targetFileName);
                File.Copy(targetFullPath, destPath, true);
            }
        }

        return result;
    }

    public void CleanAll(string workingDirectory)
    {
        var mergedPath = Path.Combine(workingDirectory, ".amalgam", "merged");
        if (Directory.Exists(mergedPath))
            Directory.Delete(mergedPath, true);
    }

    public string GetEffectivePath(RepositoryConfig repo, string workingDirectory)
    {
        if (repo.Merge != null && repo.Merge.Sources.Count > 0)
            return Path.Combine(workingDirectory, ".amalgam", "merged", repo.Name);

        return repo.Path;
    }
}
