using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GitPilot3.Models;
using LibGit2Sharp;

namespace GitPilot3.UserControlles;

public partial class LocalbrancheFlyout : UserControl
{
    public EventHandler? OnMergeBranchClicked;
    public EventHandler? OnPullClicked;
    public EventHandler? OnCopyBranchNameClicked;
    public EventHandler? OnCreateBranchHereClicked;
    public EventHandler? OnDeleteClicked;

    public string LoadFromBranchName { get; set; } = string.Empty;
    public string CurrentBranchName { get; set; } = string.Empty;

    
    public LocalbrancheFlyout()
    {
        InitializeComponent();
    }

    private void OnMergeBranchClick(object? sender, RoutedEventArgs e)
    {
        OnMergeBranchClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnPullClick(object? sender, RoutedEventArgs e)
    {
        OnPullClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnCopyBranchNameClick(object? sender, RoutedEventArgs e)
    {
        OnCopyBranchNameClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnCreateBranchHereClick(object? sender, RoutedEventArgs e)
    {
        OnCreateBranchHereClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        OnDeleteClicked?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateFlyout(string loadFromBranchName, string currentBranchName)
    {
        LoadFromBranchName = loadFromBranchName;
        CurrentBranchName = currentBranchName;
        UpdateMergeButtonText();
    }

    private void UpdateMergeButtonText()
    {
        var mergeButton = this.FindControl<Button>("MergeBranchButton");
        if (mergeButton != null)
        {
            mergeButton.Content = $"Merge '{LoadFromBranchName}' into '{CurrentBranchName}'";
        }
        if (!LoadFromBranchName.Equals(CurrentBranchName))
        {
            CreateBranchHereButton.IsEnabled = false;
        }
        else
        {
            DeleteButton.IsEnabled = false;
        }
    }
}