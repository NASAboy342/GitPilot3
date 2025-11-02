using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitPilot3.Models;
using GitPilot3.Repositories;

namespace GitPilot3.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IAppRepository _appRepository;
    public UserProfileService(IAppRepository appRepository)
    {
        _appRepository = appRepository;
    }

    public async Task AddUserProfile(UserProfile newProfile)
    {
        var profiles = await _appRepository.GetAllUserProfilesAsync();
        if (profiles == null)
        {
            throw new Exception("Failed to retrieve current user profiles.");
        }
        profiles.Add(newProfile);
        await _appRepository.SaveUserProfilesAsync(profiles);
    }

    public async Task<List<UserProfile>> GetAllUserProfilesAsync()
    {
        return await _appRepository.GetAllUserProfilesAsync() ?? new List<UserProfile>();
    }

    public async Task<UserProfile> GetCurrentUserProfileAsync()
    {
        var profiles = await GetAllUserProfilesAsync();
        var currentProfile = profiles.FirstOrDefault(p => p.IsActive) ?? null;
        if (profiles.Any() && currentProfile == null)
        {
            profiles.FirstOrDefault().IsActive = true;
        }
        currentProfile = profiles.FirstOrDefault(p => p.IsActive) ?? throw new Exception("No user profiles available.");
        return currentProfile;
    }

    public async Task SwitchActiveProfile(UserProfile selectedProfile)
    {
        var profiles = await GetAllUserProfilesAsync();
        foreach (var profile in profiles)
        {
            profile.IsActive = profile.Username == selectedProfile.Username;
        }
        await _appRepository.SaveUserProfilesAsync(profiles);
    }

    public async Task UpdateCurrentUserProfile(UserProfile editingProfile)
    {
        var profiles = await _appRepository.GetAllUserProfilesAsync();
        var currentProfile = profiles.FirstOrDefault(p => p.Id == editingProfile.Id && p.IsActive);
        if (currentProfile == null)
        {
            throw new Exception("Profile not found.");
        }
        currentProfile.Username = editingProfile.Username;
        currentProfile.Email = editingProfile.Email;
        currentProfile.Password = editingProfile.Password;
        await _appRepository.SaveUserProfilesAsync(profiles);
    }
}
