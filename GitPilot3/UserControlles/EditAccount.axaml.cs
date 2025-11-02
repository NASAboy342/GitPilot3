using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GitPilot3.Models;
using GitPilot3.Services;

namespace GitPilot3.UserControlles;

public partial class EditAccount : UserControl
{
    public EventHandler? EditAccountSaved;
    public EventHandler? EditAccountCanceled;
    private UserProfile? _editingProfile;
    private readonly IUserProfileService _userProfileService;

    private readonly ErrorMessageHandler _errorMessageHandler;
    public EditAccount(ErrorMessageHandler errorMessageHandler, IUserProfileService userProfileService)
    {
        _errorMessageHandler = errorMessageHandler;
        _userProfileService = userProfileService;
        InitializeComponent();
        LoadEditingProfile();
    }

    public async Task LoadEditingProfile()
    {
        try
        {
            _editingProfile = await _userProfileService.GetCurrentUserProfileAsync();
            if (_editingProfile == null)
            {
                throw new Exception("No profile found to edit.");
            }
            var usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
            var emailTextBox = this.FindControl<TextBox>("EmailTextBox");
            var tokenTextBox = this.FindControl<TextBox>("TokenTextBox");
            if(usernameTextBox == null || emailTextBox == null || tokenTextBox == null)
            {
                throw new Exception("One or more input fields are missing.");
            }
            
            usernameTextBox.Text = _editingProfile.Username;
            emailTextBox.Text = _editingProfile.Email;
            tokenTextBox.Text = _editingProfile.Password;
        }catch (Exception ex)
        {
            _errorMessageHandler.ShowErrorMessage("Failed to load profile for editing: " + ex.Message);
            EditAccountCanceled?.Invoke(this, EventArgs.Empty);
            return;
        }
    }

    private void OnClickCancel(object? sender, RoutedEventArgs e)
    {
        EditAccountCanceled?.Invoke(this, EventArgs.Empty);
    }

    private async void OnClickSaveEditAccount(object? sender, RoutedEventArgs e)
    {
        try
        {
            var usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
            var emailTextBox = this.FindControl<TextBox>("EmailTextBox");
            var tokenTextBox = this.FindControl<TextBox>("TokenTextBox");
            if (usernameTextBox == null || emailTextBox == null || tokenTextBox == null)
            {
                throw new Exception("One or more input fields are missing.");
            }
            if (string.IsNullOrEmpty(usernameTextBox.Text))
                throw new Exception("Username cannot be empty.");
            if (!string.IsNullOrEmpty(_editingProfile.Email) && string.IsNullOrEmpty(emailTextBox.Text))
                throw new Exception("Email cannot be empty.");
            if (string.IsNullOrEmpty(tokenTextBox.Text))
                throw new Exception("Token cannot be empty.");

            _editingProfile!.Username = usernameTextBox.Text;
            _editingProfile.Email = emailTextBox.Text;
            _editingProfile.Password = tokenTextBox.Text;
            await _userProfileService.UpdateCurrentUserProfile(_editingProfile);
            EditAccountSaved?.Invoke(this, EventArgs.Empty);
            _errorMessageHandler.ShowSuccessMessage("Account changes saved successfully.");
        }
        catch (Exception ex)
        {
            _errorMessageHandler.ShowErrorMessage("Failed to save account changes: " + ex.Message);
            return;
        }
    }
}