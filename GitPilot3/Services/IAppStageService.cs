using System;
using GitPilot3.Models;

namespace GitPilot3.Services;

public interface IAppStageService
{
    GitRepository GetCurrentRepository();
    void SaveCurrentRepository(GitRepository currentRepository);
}
