using System;
using System.Collections.Generic;
using GitPilot3.Models;

namespace GitPilot3.Services;

public interface IAppStageService
{
    List<OpenedRepository> GetAllOpenedRepositories();
    GitRepository GetCurrentRepository();
    void RemoveRepositoryFromOpenedList(OpenedRepository openedRepo);
    void SaveCurrentRepository(GitRepository currentRepository);
}
