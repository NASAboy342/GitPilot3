using System;
using System.Collections.Generic;
using LibGit2Sharp;

namespace GitPilot3.Models;

public class GitRepository
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";

    public List<GitBranch> LocalBranches { get; set; } = new List<GitBranch>();
    public List<GitBranch> RemoteBranches { get; set; } = new List<GitBranch>();
    public List<GitCommit> Commits { get; set; } = new List<GitCommit>();
    public GitCommitDetail CommitDetail { get; set; } = new GitCommitDetail();
    public bool IsAutoRefreshDisabled { get; set; } = false;
}
