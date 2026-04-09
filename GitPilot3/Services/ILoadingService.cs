using System;

namespace GitPilot3.Services;

public interface ILoadingService
{
    void StartLoading(string id, string message);
    void StopLoading(string id);
    EventHandler? LoadingStateChanged { get; set; }
    string GetCurrentLoadingMessage();
    string GenerateUniqueId();
}
