using System;

namespace GitPilot3.Models;

public class GitCommit
{
    public string Sha { get; set; } = "";
    public string Message { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public DateTimeOffset AuthorDate { get; set; }
    public string BranchName { get; set; } = "";
    public int ChangedFilesCount { get; set; }
    public bool IsWorkInProgress { get; set; } = false;
    public string Description { get; set; } = "";
}
