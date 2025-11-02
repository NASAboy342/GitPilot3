using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using GitPilot3.Services;
using GitPilot3.UserControlles;
using Microsoft.Extensions.DependencyInjection;

namespace GitPilot3;

public partial class ProfileManagementWindow : Window
{
    private readonly ProfileMenagement _profileManagement;
    private readonly AddAccount _addAccount;
    private readonly EditAccount _editAccount;
    private readonly ErrorMessageHandler _errorMessageHandler;

    public ProfileManagementWindow(ProfileMenagement profileManagement, AddAccount addAccount, EditAccount editAccount, ErrorMessageHandler errorMessageHandler)
    {
        _profileManagement = profileManagement;
        _addAccount = addAccount;
        _editAccount = editAccount;
        _errorMessageHandler = errorMessageHandler;
        InitializeComponent();
        _profileManagement.AddAccountButtonClickedEvent += (s, e) => SetCurrentViewToAddAccountView();
        _addAccount.AddAccountCompleted += async (s, e) => await OnAddAccountCompleted();
        _addAccount.AddAccountCanceled += (s, e) => SetCurrentViewToProfileManagement();
        _profileManagement.EditAccountButtonClickedEvent += (s, e) => SetCurrentViewToEditAccountView();
        _editAccount.EditAccountSaved += (s, e) => OnSaveEditAccount();
        _editAccount.EditAccountCanceled += (s, e) => SetCurrentViewToProfileManagement();
    }

    private async Task OnSaveEditAccount()
    {
        try
        {
            SetCurrentViewToProfileManagement();
            await _profileManagement.LoadProfiles();
        }
        catch (Exception ex)
        {
            _errorMessageHandler.ShowErrorMessage("Failed to save account changes: " + ex.Message);
        }
    }

    private async Task SetCurrentViewToEditAccountView()
    {
        var mainBorder = this.FindControl<Border>("MainBorder");
        if (mainBorder != null)
        {
            await _editAccount.LoadEditingProfile();
            mainBorder.Child = _editAccount;
        }
    }

    private async Task OnAddAccountCompleted()
    {
        SetCurrentViewToProfileManagement();
        await _profileManagement.LoadProfiles();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        SetCurrentViewToProfileManagement();
    }

    private void SetCurrentViewToAddAccountView()
    {
        var mainBorder = this.FindControl<Border>("MainBorder");
        if (mainBorder != null)
        {
            mainBorder.Child = _addAccount;
        }
    }

    private void SetCurrentViewToProfileManagement()
    {
        var mainBorder = this.FindControl<Border>("MainBorder");
        if (mainBorder != null)
        {
            mainBorder.Child = _profileManagement;
        }
    }
}