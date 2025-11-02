using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GitPilot3.Models;
using GitPilot3.Services;

namespace GitPilot3.UserControlles;

public partial class AddAccount : UserControl
{
    public EventHandler? AddAccountCompleted;
    public EventHandler? AddAccountCanceled;

    private readonly IUserProfileService _userProfileService;

    public AddAccount(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
        InitializeComponent();
    }

    private void OnClickAddAccount(object? sender, RoutedEventArgs e)
    {
        var usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
        var emailTextBox = this.FindControl<TextBox>("EmailTextBox");
        var tokenTextBox = this.FindControl<TextBox>("TokenTextBox");

        var newProfile = new UserProfile()
        {
            Username = usernameTextBox?.Text ?? string.Empty,
            Email = emailTextBox?.Text ?? string.Empty,
            Password = tokenTextBox?.Text ?? string.Empty,
            Color = new RGBColor().GetRandomColor()
        };

        _userProfileService.AddUserProfile(newProfile);

        AddAccountCompleted?.Invoke(this, EventArgs.Empty);
    }
    private void OnClickCancel(object? sender, RoutedEventArgs e)
    {
        AddAccountCanceled?.Invoke(this, EventArgs.Empty);
    }
}