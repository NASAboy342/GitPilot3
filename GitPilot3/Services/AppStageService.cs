using System;
using System.Collections.Generic;
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
        UpdateLastOpenedRepository(appStage, currentRepository);
        _appRepository.SaveAppStage(appStage);
    }

    private void UpdateLastOpenedRepository(AppStage appStage, GitRepository currentRepository)
    {
        if (currentRepository == null) return;
        var openedRepo = appStage.OpenedRepositories.Find(r => r.Path == currentRepository.Path);;
        if (openedRepo != null)
        {
            openedRepo.LastOpened = DateTime.Now;
        }
        else
        {
            appStage.OpenedRepositories.Add(new OpenedRepository
            {
                Path = currentRepository.Path,
                Name = currentRepository.Name,
                LastOpened = DateTime.Now
            });
        }
    }

    private AppStage GetAppStage()
    {
        return _appRepository.GetAppStage();
    }

    public List<OpenedRepository> GetAllOpenedRepositories()
    {
        var appStage = GetAppStage();
        return appStage.OpenedRepositories ?? new List<OpenedRepository>();
    }
}
