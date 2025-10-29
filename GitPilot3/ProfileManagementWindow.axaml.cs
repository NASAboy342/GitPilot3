using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GitPilot3.UserControlles;

namespace GitPilot3;

public partial class ProfileManagementWindow : Window
{
    private readonly ProfileMenagement _profileManagement;
    public ProfileManagementWindow()
    {
        _profileManagement = new ProfileMenagement();
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        SetCurrentViewToProfileManagement();
    }

    private void SetCurrentViewToProfileManagement()
    {
        var mainBorder = this.FindControl<Border>("MainBorder");
        if (mainBorder != null)
            mainBorder.Child = _profileManagement;
    }
}