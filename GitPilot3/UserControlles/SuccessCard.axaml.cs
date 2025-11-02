using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GitPilot3.UserControlles;

public partial class SuccessCard : UserControl
{
    public EventHandler? CloseSuccessCardClicked;
    public int DelayTimeInSecond { get; set; } = 8;
    public SuccessCard(string message = "")
    {
        InitializeComponent();
        var successTitleTextBlock = this.FindControl<TextBlock>("SuccessTitleTextBlock");
        if (successTitleTextBlock != null)
        {
            successTitleTextBlock.Text = string.IsNullOrEmpty(message) ? "Generic Success" : message;
        }
        StartCountDownToAutoClose();
    }

    private async Task StartCountDownToAutoClose()
    {
        await Task.Delay(TimeSpan.FromSeconds(DelayTimeInSecond));
        CloseSuccessCardClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnClickCloseSuccessCard(object? sender, RoutedEventArgs e)
    {
        CloseSuccessCardClicked?.Invoke(this, EventArgs.Empty);
    }
}