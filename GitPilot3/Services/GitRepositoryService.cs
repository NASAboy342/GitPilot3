using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitPilot3.Models;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace GitPilot3.Services;

public class GitRepositoryService : IGitRepositoryService
{
    public async Task<List<GitBranch>> GetRemoteBranchesAsync(string? repositoryPath)
    {
        var libgitRepository = new Repository(repositoryPath);
        var remoteBranches = new List<GitBranch>();
        foreach (var branch in libgitRepository.Branches)
        {
            if (branch.IsRemote)
            {
                remoteBranches.Add(new GitBranch
                {
                    IsRemote = true,
                    Name = branch.FriendlyName,
                    InComming = 0, // Placeholder for actual logic
                    OutGoing = 0,  // Placeholder for actual logic
                    Color = new GitBranch.RGBColor().GetRandomColor()
                });
            }
        }
        return remoteBranches;
    }

    public async Task<List<GitBranch>> GetLocalBranchesAsync(string? repositoryPath)
    {
        var libgitRepository = new Repository(repositoryPath);
        var localBranches = new List<GitBranch>();
        foreach (var branch in libgitRepository.Branches)
        {
            if (!branch.IsRemote)
            {
                localBranches.Add(new GitBranch
                {
                    IsRemote = false,
                    Name = branch.FriendlyName,
                    IsCurrent = branch.IsCurrentRepositoryHead,
                    InComming = 0, // Placeholder for actual logic
                    OutGoing = 0,  // Placeholder for actual logic
                    Color = new GitBranch.RGBColor().GetRandomColor()
                });
            }
        }
        return localBranches;
    }

    public async Task<GitRepository> LoadRepositoryAsync(string? repositoryPath)
    {
        if(string.IsNullOrEmpty(repositoryPath))
        {
            return new GitRepository();
        }
        var libgitRepository = new Repository(repositoryPath);
        var gitRepository = new GitRepository
        {
            Name = GetRepositoryNameFromPath(repositoryPath),
            Path = libgitRepository.Info.Path
        };
        return gitRepository;
    }

    private string GetRepositoryNameFromPath(string? repositoryPath)
    {
        if (string.IsNullOrEmpty(repositoryPath))
        {
            return "Unknown Repository";
        }
        return System.IO.Path.GetFileName(repositoryPath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
    }

    public async Task CheckoutBranchAsync(GitRepository currentRepository, string name)
    {
        var libgitRepository = new Repository(currentRepository.Path);
        var branch = libgitRepository.Branches[name];
        if (branch == null)
            return;
        Commands.Checkout(libgitRepository, branch);
    }

    public async Task<List<GitCommit>> GetCommitsAsync(string repositoryPath)
    {
        var libgitRepository = new Repository(repositoryPath);
        var commits = new List<GitCommit>();
        AddIfHaveWorkInProgress(libgitRepository, commits);
        commits.AddRange(libgitRepository.Branches
            .SelectMany(b => b.Commits.Take(50))
            .DistinctBy(c => c.Sha)
            .Select(c => new GitCommit
            {
                Sha = c.Sha,
                Message = c.MessageShort,
                Description = c.Message,
                AuthorName = c.Author.Name,
                AuthorDate = c.Author.When,
                BranchName = libgitRepository.Branches.FirstOrDefault(b => b.Tip.Sha == c.Sha)?.FriendlyName ?? "",
            }).OrderByDescending(c => c.AuthorDate).ToList());
        return commits;
    }

    private void AddIfHaveWorkInProgress(Repository libgitRepository, List<GitCommit> commits)
    {
        var isHasWorkInProgress = libgitRepository.RetrieveStatus()
            .Any(s => s.State != FileStatus.Ignored && s.State != FileStatus.Unaltered);
        if (isHasWorkInProgress)
        {
            commits.Add(new GitCommit
            {
                Sha = "WIP",
                Message = "Work In Progress",
                AuthorName = "",
                AuthorDate = DateTimeOffset.Now,
                BranchName = libgitRepository.Head.FriendlyName,
                ChangedFilesCount = libgitRepository.RetrieveStatus()
                    .Count(s => s.State != FileStatus.Ignored && s.State != FileStatus.Unaltered),
                IsWorkInProgress = true
            });
        }
    }

    public async Task<GitCommitDetail> GetCommitDetailsAsync(string path, GitCommit commit)
    {
        if (commit.IsWorkInProgress)
            return GetWIPDetails(path, commit);
        else
            return GetCommitDetailsFromGit(path, commit);
    }

    private GitCommitDetail GetCommitDetailsFromGit(string path, GitCommit commit)
    {
        var libgitRepository = new Repository(path);
        var libgitCommit = libgitRepository.Lookup<LibGit2Sharp.Commit>(commit.Sha);
        if (libgitCommit == null)
            return new GitCommitDetail();

        var commitDetail = new GitCommitDetail();
        commitDetail.Commit = commit;

        if(libgitCommit.Parents.Any()){
            var changes = libgitRepository.Diff.Compare<TreeChanges>(libgitCommit.Parents.First().Tree, libgitCommit.Tree);

            foreach (var change in changes)
            {
                var patch = libgitRepository.Diff.Compare<Patch>(libgitCommit.Parents.First().Tree, libgitCommit.Tree, new[] { change.Path });
                var patchEntry = patch.FirstOrDefault(p => p.Path == change.Path);
                var fileChange = new GitCommitFileChange();
                fileChange.FilePath = change.Path;
                fileChange.ChangeType = change.Status.ToString();
                fileChange.Additions = patchEntry?.LinesAdded ?? 0;
                fileChange.Deletions = patchEntry?.LinesDeleted ?? 0;
                fileChange.DiffContent = patchEntry?.Patch ?? "";
                fileChange.IsStaged = true;

                var blob = libgitCommit.Tree[change.Path]?.Target.Id;
                fileChange.FileContent = blob != null ? libgitRepository.Lookup<LibGit2Sharp.Blob>(blob)?.GetContentText() : "";
                commitDetail.FilesChanged.Add(fileChange);
            }
        }
        return commitDetail;
    }

    private static GitCommitDetail GetWIPDetails(string path, GitCommit commit)
    {
        var libgitRepository = new Repository(path);
        var commitDetail = new GitCommitDetail();
        commitDetail.Commit = commit;
        AppendUnstagedChanges(libgitRepository, commitDetail);
        AppendStagedChanges(libgitRepository, commitDetail);

        return commitDetail;
    }

    private static void AppendStagedChanges(Repository libgitRepository, GitCommitDetail commitDetail)
    {
        var changes = libgitRepository.Diff.Compare<TreeChanges>(null, DiffTargets.Index);

        foreach (var change in changes)
        {
            var patch = libgitRepository.Diff.Compare<Patch>(null, DiffTargets.Index, new[] { change.Path });
            var patchEntry = patch.FirstOrDefault(p => p.Path == change.Path);

            var fileChange = new GitCommitFileChange();
            fileChange.FilePath = change.Path;
            fileChange.ChangeType = change.Status.ToString();
            fileChange.Additions = patchEntry?.LinesAdded ?? 0;
            fileChange.Deletions = patchEntry?.LinesDeleted ?? 0;
            fileChange.DiffContent = patchEntry?.Patch ?? "";
            fileChange.IsStaged = true;

            var blob = libgitRepository.Index[change.Path]?.Id;
            fileChange.FileContent = blob != null ? libgitRepository.Lookup<LibGit2Sharp.Blob>(blob)?.GetContentText() : "";

            commitDetail.FilesChanged.Add(fileChange);
        }
    }

    private static void AppendUnstagedChanges(Repository libgitRepository, GitCommitDetail commitDetail)
    {
        var changes = libgitRepository.Diff.Compare<TreeChanges>(libgitRepository.Head.Tip.Tree, DiffTargets.WorkingDirectory);

        foreach (var change in changes)
        {
            var patch = libgitRepository.Diff.Compare<Patch>(libgitRepository.Head.Tip.Tree, DiffTargets.WorkingDirectory, new[] { change.Path });
            var patchEntry = patch.FirstOrDefault(p => p.Path == change.Path);

            var fileChange = new GitCommitFileChange();
            fileChange.FilePath = change.Path;
            fileChange.ChangeType = change.Status.ToString();
            fileChange.Additions = patchEntry?.LinesAdded ?? 0;
            fileChange.Deletions = patchEntry?.LinesDeleted ?? 0;
            fileChange.DiffContent = patchEntry?.Patch ?? "";
            fileChange.IsStaged = false;

            var blob = libgitRepository.Head.Tip.Tree[change.Path]?.Target.Id;
            fileChange.FileContent = blob != null ? libgitRepository.Lookup<LibGit2Sharp.Blob>(blob)?.GetContentText() : "";

            commitDetail.FilesChanged.Add(fileChange);
        }
    }

    public Task StageFilesAsync(string path, List<string> unstageFilePaths)
    {
        var libgitRepository = new Repository(path);
        Commands.Stage(libgitRepository, unstageFilePaths);
        return Task.CompletedTask;
    }

    public Task UnStageFilesAsync(string path, List<string> list)
    {
        var libgitRepository = new Repository(path);
        Commands.Unstage(libgitRepository, list);
        return Task.CompletedTask;
    }
}