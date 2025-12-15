using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GitPilot3.UserControlles;

public partial class CommonSimpleInput : UserControl
{
    public EventHandler<string>? OnOkClicked;
    public EventHandler? OnCancelClicked;
    public bool IsAllowSpaces { get; set; } = true;
    public string InputText
    {
        get => InputTextBox.Text ?? string.Empty;
        set => InputTextBox.Text = value;
    }
    public string PlaceHolderText
    {
        get => InputTextBox.Watermark?.ToString() ?? string.Empty;
        set => InputTextBox.Watermark = value;
    }
    /// <summary>
    /// Creates a simple input dialog with OK and Cancel buttons.
    /// Best supported with Width="500" Height="100"
    /// </summary>
    public CommonSimpleInput()
    {
        InitializeComponent();
    }
    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnCancelClicked?.Invoke(this, EventArgs.Empty);
    }
    private void OnOkClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnOkClicked?.Invoke(this, InputText);
    }
    private void OnTextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        if (!IsAllowSpaces)
            InputText = InputText.Trim();
    }
}