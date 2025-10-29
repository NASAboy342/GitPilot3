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
        if (string.IsNullOrEmpty(repositoryPath))
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

        if (libgitCommit.Parents.Any())
        {
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
        var status = libgitRepository.RetrieveStatus();
        var changes = libgitRepository.Diff.Compare<TreeChanges>(libgitRepository.Head.Tip.Tree, DiffTargets.Index);

        foreach (var entry in status)
        {
            if (entry.State.HasFlag(FileStatus.NewInIndex) ||
                entry.State.HasFlag(FileStatus.ModifiedInIndex) ||
                entry.State.HasFlag(FileStatus.DeletedFromIndex) ||
                entry.State.HasFlag(FileStatus.RenamedInIndex) ||
                entry.State.HasFlag(FileStatus.TypeChangeInIndex))
            {
                var change = changes.FirstOrDefault(c => c.Path == entry.FilePath);
                var patch = libgitRepository.Diff.Compare<Patch>(null, DiffTargets.Index, new[] { entry.FilePath });
                var patchEntry = patch.FirstOrDefault(p => p.Path == entry.FilePath);

                var fileChange = new GitCommitFileChange();
                fileChange.FilePath = entry.FilePath;
                fileChange.ChangeType = change?.Status.ToString() ?? "";
                fileChange.Additions = patchEntry?.LinesAdded ?? 0;
                fileChange.Deletions = patchEntry?.LinesDeleted ?? 0;
                fileChange.DiffContent = patchEntry?.Patch ?? "";
                fileChange.IsStaged = true;

                var blob = libgitRepository.Index[entry.FilePath]?.Id;
                fileChange.FileContent = blob != null ? libgitRepository.Lookup<LibGit2Sharp.Blob>(blob)?.GetContentText() : "";

                commitDetail.FilesChanged.Add(fileChange);
            }
        }
    }

    private static void AppendUnstagedChanges(Repository libgitRepository, GitCommitDetail commitDetail)
    {
        var status = libgitRepository.RetrieveStatus();
        var changes = libgitRepository.Diff.Compare<TreeChanges>(libgitRepository.Head.Tip.Tree, DiffTargets.WorkingDirectory);

        foreach (var entry in status)
        {
            if (entry.State.HasFlag(FileStatus.NewInWorkdir) ||
                entry.State.HasFlag(FileStatus.ModifiedInWorkdir) ||
                entry.State.HasFlag(FileStatus.DeletedFromWorkdir) ||
                entry.State.HasFlag(FileStatus.RenamedInWorkdir) ||
                entry.State.HasFlag(FileStatus.TypeChangeInWorkdir))
            {
                var change = changes.FirstOrDefault(c => c.Path == entry.FilePath);
                var patch = libgitRepository.Diff.Compare<Patch>(libgitRepository.Head.Tip.Tree, DiffTargets.WorkingDirectory, new[] { entry.FilePath });
                var patchEntry = patch.FirstOrDefault(p => p.Path == entry.FilePath);

                var fileChange = new GitCommitFileChange();
                fileChange.FilePath = entry.FilePath;
                fileChange.ChangeType = change?.Status.ToString() ?? "";
                fileChange.Additions = patchEntry?.LinesAdded ?? 0;
                fileChange.Deletions = patchEntry?.LinesDeleted ?? 0;
                fileChange.DiffContent = patchEntry?.Patch ?? "";
                fileChange.IsStaged = false;

                var blob = libgitRepository.Head.Tip.Tree[entry.FilePath]?.Target.Id;
                fileChange.FileContent = blob != null ? libgitRepository.Lookup<LibGit2Sharp.Blob>(blob)?.GetContentText() : "";

                commitDetail.FilesChanged.Add(fileChange);
            }
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

    public async Task ValidateIfCommitIsPossibleAsync(string path, GitRepository currentRepository)
    {
        if (!currentRepository.CommitDetail.FilesChanged.Any(f => f.IsStaged))
        {
            throw new InvalidOperationException("No staged files to commit.");
        }
    }

    public async Task<CommitRequest> GetCommitRequestAsync(string path, GitRepository currentRepository, string? message, string? description, UserProfile currentUserProfile)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Commit message cannot be empty.");
        }
        var commitRequest = new CommitRequest
        {
            Message = message,
            Description = description ?? "",
            Author = currentUserProfile.Username,
            Email = currentUserProfile.Email
        };
        return commitRequest;
    }

    public Task CommitAsync(string path, GitRepository currentRepository, CommitRequest commitRequest)
    {
        var libgitRepository = new Repository(path);
        var author = new Signature(commitRequest.Author, commitRequest.Email, DateTimeOffset.Now);
        var message = commitRequest.Message + "\n\n" + commitRequest.Description;

        Commit commit = libgitRepository.Commit(message, author, author);
        return Task.CompletedTask;
    }

    public async Task PushAsync(string path, GitRepository currentRepository, UserProfile userProfile)
    {
        var libgitRepository = new Repository(path);
        var currentBranch = currentRepository.LocalBranches.FirstOrDefault(b => b.IsCurrent);
        if (currentBranch == null){
            throw new InvalidOperationException("No current branch found to push.");
        }

        var options = new PushOptions
        {
            CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials
                {
                    Username = userProfile.Username,
                    Password = userProfile.Password
                }
        };

        libgitRepository.Network.Push(libgitRepository.Branches[currentBranch.Name], options);
    }
}