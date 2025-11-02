using System;
using GitPilot3.Models;
using GitPilot3.Repositories;

namespace GitPilot3.Services;

public class AppStageService : IAppStageService
{
    private readonly IAppRepository _appRepository;
    public AppStageService(IAppRepository appRepository)
    {
        _appRepository = appRepository;
    }

    public GitRepository GetCurrentRepository()
    {
        var appStage = GetAppStage();
        var repository = appStage.CurrentRepository ?? null;
        return repository;
    }

    public void SaveCurrentRepository(GitRepository currentRepository)
    {
        var appStage = GetAppStage();
        appStage.CurrentRepository = currentRepository;
        _appRepository.SaveAppStage(appStage);
    }

    private AppStage GetAppStage()
    {
        return _appRepository.GetAppStage();
    }
}
