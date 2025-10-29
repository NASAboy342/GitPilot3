using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GitPilot3.Services;

namespace GitPilot3.UserControlles;

public partial class ProfileMenagement : UserControl
{
    private readonly IUserProfileService _userProfileService;
    public ProfileMenagement()
    {
        _userProfileService = new UserProfileService();
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        LoadProfiles();
    }

    private async Task LoadProfiles()
    {
        var allProfiles = await _userProfileService.GetAllUserProfilesAsync();
        var profiles = await _userProfileService.GetCurrentUserProfileAsync();
        var usernameTextBlock = this.FindControl<TextBlock>("NameTextBlock");
        var emailTextBlock = this.FindControl<TextBlock>("EmailTextBlock");
        if (usernameTextBlock != null)
            usernameTextBlock.Text = "Username: " + profiles.Username;
        if (emailTextBlock != null)
            emailTextBlock.Text = "Email: " + profiles.Email;
    }
}