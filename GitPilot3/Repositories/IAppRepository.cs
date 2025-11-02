using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitPilot3.Models;

namespace GitPilot3.Repositories;

public interface IAppRepository
{
    Task<List<UserProfile>?> GetAllUserProfilesAsync();
    AppStage GetAppStage();
    void SaveAppStage(AppStage appStage);
    Task SaveUserProfilesAsync(List<UserProfile> profiles);
}
