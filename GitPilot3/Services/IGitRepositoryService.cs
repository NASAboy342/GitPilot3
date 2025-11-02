using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitPilot3.Models;

namespace GitPilot3.Services;

public interface IGitRepositoryService
{
    Task<List<GitBranch>> GetRemoteBranchesAsync(string? repositoryPath);
    Task<GitRepository> LoadRepositoryAsync(string? repositoryPath);
    Task<List<GitBranch>> GetLocalBranchesAsync(string? repositoryPath);
    Task CheckoutBranchAsync(GitRepository currentRepository, string name);
    Task<List<GitCommit>> GetCommitsAsync(string repositoryPath);
    Task<GitCommitDetail> GetCommitDetailsAsync(string path, GitCommit commit);
    Task StageFilesAsync(string path, List<string> unstageFilePaths);
    Task UnStageFilesAsync(string path, List<string> list);
    Task ValidateIfCommitIsPossibleAsync(string path, GitRepository currentRepository);
    Task<CommitRequest> GetCommitRequestAsync(string path, GitRepository currentRepository, string? text, string? description, UserProfile currentUserProfile);
    Task CommitAsync(string path, GitRepository currentRepository, CommitRequest commitRequest);
    Task PushAsync(string path, GitRepository currentRepository, UserProfile userProfile);
    Task FetchAsync(string path, UserProfile currentUserProfile);
    Task PullAsync(UserProfile currentUserProfile, GitRepository currentRepository);
    Task CreateNewLocalBranchFromRemoteAsync(string path, string name);
}
