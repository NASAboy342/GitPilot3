using System;
using System.Collections.Generic;

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
    public List<string> ParentShas { get; set; } = new List<string>();
}
