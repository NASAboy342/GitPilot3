using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitPilot3.Models;

namespace GitPilot3.Services;

public interface IUserProfileService
{
    Task<List<UserProfile>> GetAllUserProfilesAsync();
    Task<UserProfile> GetCurrentUserProfileAsync();
}
