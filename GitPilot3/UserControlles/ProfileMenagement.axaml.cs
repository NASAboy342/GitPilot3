using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GitPilot3.Models;
using GitPilot3.Services;

namespace GitPilot3.UserControlles;

public partial class ProfileMenagement : UserControl
{
    private readonly IUserProfileService _userProfileService;
    public EventHandler AddAccountButtonClickedEvent;
    public EventHandler EditAccountButtonClickedEvent;
    private readonly ErrorMessageHandler _errorMessageHandler;
    public ProfileMenagement(IUserProfileService userProfileService, ErrorMessageHandler errorMessageHandler)
    {
        _userProfileService = userProfileService;
        _errorMessageHandler = errorMessageHandler;
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        LoadProfiles();
    }

    public async Task LoadProfiles()
    {
        var allProfiles = await _userProfileService.GetAllUserProfilesAsync();
        var profiles = await _userProfileService.GetCurrentUserProfileAsync();
        SetValueToProfileText(profiles);
        await SetValuesAllProfilesToComboBox(allProfiles);
    }

    private async Task SetValuesAllProfilesToComboBox(List<UserProfile> allProfiles)
    {
        var comboBox = this.FindControl<ComboBox>("ProfileComboBox");
        if (comboBox == null)
            return;
        if (comboBox.Items.Any())
        {
            comboBox.Items.Clear();
        }

        var currentProfile = await _userProfileService.GetCurrentUserProfileAsync();

        foreach (var profile in allProfiles)
        {
            comboBox.Items.Add(GetUserProfileViewItem(profile));
        }
        comboBox.SelectedIndex = allProfiles.FindIndex(p => p.Username == currentProfile.Username);
        comboBox.SelectionChanged += async (s, e) =>
        {
            try
            {
                var selectedIndex = comboBox.SelectedIndex;
                if (selectedIndex < 0)
                    return;
                var allProfiles = await _userProfileService.GetAllUserProfilesAsync();
                if (selectedIndex >= 0 && selectedIndex < allProfiles.Count)
                {
                    var selectedProfile = allProfiles[selectedIndex];
                    await ChangeActiveProfile(allProfiles, selectedProfile);
                    _errorMessageHandler.ShowSuccessMessage($"Switched to profile: {selectedProfile.Username}");
                }
            }
            catch (Exception ex)
            {
                _errorMessageHandler.ShowErrorMessage("Failed to switch profile: " + ex.Message);
                return;
            }
        };
    }

    private async Task ChangeActiveProfile(List<UserProfile> allProfiles, UserProfile selectedProfile)
    {
        if (selectedProfile == null)
            throw new Exception("Selected profile is null.");
        await _userProfileService.SwitchActiveProfile(selectedProfile);
        var currentProfile = await _userProfileService.GetCurrentUserProfileAsync();
        SetValueToProfileText(currentProfile);
    }

    private object? GetUserProfileViewItem(UserProfile profile)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 10
        };
        var profileIcon = new Border
        {
            Width = 32,
            Height = 32,
            Background = new Avalonia.Media.SolidColorBrush(profile.ToAvaloniaColor()),
            Child = new TextBlock
            {
                Text = profile.Username.Length > 0 ? profile.Username[0].ToString().ToUpper() : "?",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Avalonia.Media.Brushes.White
            }
        };
        stackPanel.Children.Add(profileIcon);
        var usernameTextBlock = new TextBlock
        {
            Text = profile.Username,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 20
        };
        stackPanel.Children.Add(usernameTextBlock);
        var emailTextBlock = new TextBlock
        {
            Text = $"<{profile.Email}>",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 14,
            Foreground = Avalonia.Media.Brushes.Gray
        };
        stackPanel.Children.Add(emailTextBlock);
        return stackPanel;

    }

    private void SetValueToProfileText(Models.UserProfile profiles)
    {
        var usernameTextBlock = this.FindControl<TextBlock>("NameTextBlock");
        var emailTextBlock = this.FindControl<TextBlock>("EmailTextBlock");
        if (usernameTextBlock != null)
            usernameTextBlock.Text = "Username: " + profiles.Username;
        if (emailTextBlock != null)
            emailTextBlock.Text = "Email: " + profiles.Email;
    }

    private void OnClickAddAccount(object? sender, RoutedEventArgs e)
    {
        AddAccountButtonClickedEvent?.Invoke(this, EventArgs.Empty);
    }
    private void OnClickEditAccount(object? sender, RoutedEventArgs e)
    {
        EditAccountButtonClickedEvent?.Invoke(this, EventArgs.Empty);
    }
}