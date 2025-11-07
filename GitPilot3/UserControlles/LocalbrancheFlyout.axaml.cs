using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GitPilot3.Models;

namespace GitPilot3.UserControlles;

public partial class LocalbrancheFlyout : UserControl
{
    public EventHandler? MergeBranchClicked;
    public EventHandler? PullClicked;
    public EventHandler? CopyBranchNameClicked;
    public EventHandler? CreateBranchHereClicked;
    public EventHandler? DeleteClicked;
    
    public LocalbrancheFlyout()
    {
        InitializeComponent();
    }

    private void OnMergeBranchClick(object? sender, RoutedEventArgs e)
    {
        MergeBranchClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnPullClick(object? sender, RoutedEventArgs e)
    {
        PullClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnCopyBranchNameClick(object? sender, RoutedEventArgs e)
    {
        CopyBranchNameClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnCreateBranchHereClick(object? sender, RoutedEventArgs e)
    {
        CreateBranchHereClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        DeleteClicked?.Invoke(this, EventArgs.Empty);
    }
}