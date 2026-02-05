using System;
using System.Collections.Generic;
using Avalonia.Automation;

namespace GitPilot3.Models;

public class GitCommitDetail
{
    public List<GitCommitFileChange> FilesChanged { get; set; } = new List<GitCommitFileChange>();
    public GitCommit Commit { get; set; } = new GitCommit();
}

public class GitCommitFileChange
{
    public string FilePath { get; set; } = "";
    public string FileName => System.IO.Path.GetFileName(FilePath);
    public string ChangeType { get; set; } = "";
    public bool IsEdit { get; set; } = false;
    public bool IsAddition { get; set; } = false;
    public bool IsDeletion { get; set; } = false;
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public string FileContent { get; set; } = "";
    public string DiffContent { get; set; } = "";
    public bool IsStaged { get; set; } = true;
    public bool IsConflicted { get; set; }
}
