using System;

namespace GitPilot3.Services;

public class ErrorMessageHandler
{
    public event EventHandler<string>? ErrorMessageShown;
    public event EventHandler<string>? SuccessMessageShown;
    private readonly object _lock = new();
    public void ShowErrorMessage(string message)
    {
        lock (_lock)
        {
            ErrorMessageShown?.Invoke(this, message);
        }
    }

    internal void ShowSuccessMessage(string message)
    {
        lock (_lock)
        {
            SuccessMessageShown?.Invoke(this, message);
        }
    }

}
