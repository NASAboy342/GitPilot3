using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GitPilot3.UserControlles;

public partial class CommonConfirmation : UserControl
{
    public EventHandler? OnYesClicked;
    public EventHandler? OnCancelClicked;
    public string Message
    {
        get => MessageTextBlock.Text ?? string.Empty;
        set => MessageTextBlock.Text = value;
    }
    public CommonConfirmation()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnCancelClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnYesClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnYesClicked?.Invoke(this, EventArgs.Empty);
    }
}