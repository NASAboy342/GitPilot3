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
}
