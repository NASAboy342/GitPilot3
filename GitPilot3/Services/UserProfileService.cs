using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitPilot3.Models;

namespace GitPilot3.Services;

public class UserProfileService : IUserProfileService
{
    public async Task<List<UserProfile>> GetAllUserProfilesAsync()
    {
        return new List<UserProfile>();
    }

    public async Task<UserProfile> GetCurrentUserProfileAsync()
    {
        return new UserProfile
        {
            Username = "Pin Sopheaktra",
            Email = "sopheaktra.pin@techbodia.com",
            Password = "securepassword123"
        };
    }
}
