using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GitPilot3.UserControlles;

public partial class ErrorCard : UserControl
{
    public EventHandler? CloseErrorCardClicked;
    public int DelayTimeInSecond { get; set; } = 30;
    public ErrorCard(string message = "")
    {
        InitializeComponent();
        var errorTitleTextBlock = this.FindControl<TextBlock>("ErrorTitleTextBlock");
        if (errorTitleTextBlock != null)
        {
            errorTitleTextBlock.Text = string.IsNullOrEmpty(message) ? "Generic Error" : message;
        }
        StartCountDownToAutoClose();
    }

    private async Task StartCountDownToAutoClose()
    {
        await Task.Delay(TimeSpan.FromSeconds(DelayTimeInSecond));
        CloseErrorCardClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnClickCloseErrorCard(object? sender, RoutedEventArgs e)
    {
        CloseErrorCardClicked?.Invoke(this, EventArgs.Empty);
    }
}