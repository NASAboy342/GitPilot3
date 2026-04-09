using System;
using System.Collections.Generic;
using GitPilot3.Models;

namespace GitPilot3.Services;

public class LoadingService : ILoadingService
{

    private object _lock = new object();
    private List<LoadingObject> _loadingObjects = new List<LoadingObject>();

    public EventHandler? LoadingStateChanged { get; set; }

    public void StartLoading(string id, string message)
    {
        lock (_lock)
        {
            var loadingObject = new LoadingObject(id, message);
            _loadingObjects.Add(loadingObject);
            LoadingStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void StopLoading(string id)
    {
        lock (_lock)
        {
            _loadingObjects.RemoveAll(lo => lo.Id.Equals(id));
            LoadingStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string GetCurrentLoadingMessage()
    {
        lock (_lock)
        {
            if (_loadingObjects.Count == 0)
            {
                return string.Empty;
            }
            return _loadingObjects[_loadingObjects.Count - 1].Message;
        }
    }

    public string GenerateUniqueId()
    {
        lock (_lock)
        {
            return Guid.NewGuid().ToString().Substring(0, 8); // Generate a short unique ID
        }
    }
}
