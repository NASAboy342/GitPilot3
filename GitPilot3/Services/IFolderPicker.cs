using System;
using System.Threading.Tasks;

namespace GitPilot3.Services;

public interface IFolderPicker
{
    Task<string?> ShowDialogAsync();
    string? ShowDialog();
}
