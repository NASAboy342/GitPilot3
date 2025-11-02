using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitPilot3.Models;

namespace GitPilot3.Services;

public interface IUserProfileService
{
    Task AddUserProfile(UserProfile newProfile);
    Task<List<UserProfile>> GetAllUserProfilesAsync();
    Task<UserProfile> GetCurrentUserProfileAsync();
    Task SwitchActiveProfile(UserProfile selectedProfile);
    Task UpdateCurrentUserProfile(UserProfile editingProfile);
}
