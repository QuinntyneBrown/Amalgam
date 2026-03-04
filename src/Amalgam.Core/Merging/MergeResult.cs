namespace Amalgam.Core.Merging;

public class MergeResult
{
    public string StagingPath { get; set; } = string.Empty;
    public int MergedFileCount { get; set; }
    public List<string> Warnings { get; set; } = new();
}
