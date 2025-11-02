using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitPilot3.Enums;
using GitPilot3.Models;
using Newtonsoft.Json;

namespace GitPilot3.Repositories;

public class AppRepository : IAppRepository
{
    private readonly string _dataFolderPath;
    private readonly Dictionary<EnumAppDataFileName, string> _dataFileNames;
    private readonly object _folderFileLock = new object();
    public AppRepository()
    {
        _dataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitPilot3");
        _dataFileNames = new Dictionary<EnumAppDataFileName, string>
        {
            { EnumAppDataFileName.UserProfiles, Path.Combine(_dataFolderPath, "user_profiles.json") },
            { EnumAppDataFileName.AppStage, Path.Combine(_dataFolderPath, "app_stage.json") }
        };

        EnsureFilesExist();
    }

    private void EnsureFilesExist()
    {
        lock (_folderFileLock)
        {
            if (!Directory.Exists(_dataFolderPath))
            {
                Directory.CreateDirectory(_dataFolderPath);
            }
            foreach (var filePath in _dataFileNames.Values)
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }
            }
        }
    }

    private T GetData<T>(EnumAppDataFileName fileName)
    {
        lock (_folderFileLock)
        {
            var filePath = _dataFileNames[fileName];
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Data file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            if(string.IsNullOrWhiteSpace(json))
            {
                return Activator.CreateInstance<T>();
            }
            return JsonConvert.DeserializeObject<T>(json) ?? throw new Exception($"Failed to deserialize data from file: {filePath}");
        }
    }

    private void SaveData<T>(EnumAppDataFileName fileName, T data)
    {
        lock (_folderFileLock)
        {
            var filePath = _dataFileNames[fileName];
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }

    public async Task<List<UserProfile>?> GetAllUserProfilesAsync()
    {
        var userProfiles = GetData<List<UserProfile>>(EnumAppDataFileName.UserProfiles);
        return userProfiles;
    }

    public async Task SaveUserProfilesAsync(List<UserProfile> profiles)
    {
        SaveData(EnumAppDataFileName.UserProfiles, profiles);
    }

    public AppStage GetAppStage()
    {
        return GetData<AppStage>(EnumAppDataFileName.AppStage);
    }

    public void SaveAppStage(AppStage appStage)
    {
        SaveData(EnumAppDataFileName.AppStage, appStage);
    }
}
