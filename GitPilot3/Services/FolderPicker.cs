using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace GitPilot3.Services;

public class FolderPicker : IFolderPicker
{
    private readonly Window _parentWindow;

    public FolderPicker(Window parentWindow)
    {
        _parentWindow = parentWindow;
    }

    public async Task<string?> ShowDialogAsync()
    {
        // Get the storage provider from the parent window
        var storageProvider = _parentWindow.StorageProvider;

        // Configure the folder picker options
        var options = new FolderPickerOpenOptions
        {
            Title = "Select Repository Folder",
            AllowMultiple = false
        };

        // Show the folder picker dialog
        var result = await storageProvider.OpenFolderPickerAsync(options);

        // Return the selected folder path or null if cancelled
        if (result.Count > 0)
        {
            return result[0].Path.LocalPath;
        }

        return null;
    }

    // Synchronous version (not recommended, but available if needed)
    public string? ShowDialog()
    {
        // This is a blocking call - not ideal for UI
        var task = ShowDialogAsync();
        task.Wait();
        return task.Result;
    }
}
