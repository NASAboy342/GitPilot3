using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GitPilot3.UserControlles;

public partial class CommonOptionsForFlyout : UserControl
{
    private List<string> Options { get; set; } = new List<string>()
    {
        "Option 1",
        "Option 2",
        "Option 3"
    };
    public EventHandler<string>? OptionSelected;
    public CommonOptionsForFlyout()
    {
        InitializeComponent();
    }

    public void SetOptions(List<string> options)
    {
        Options = options;
        SetOptions();
    }

    private void SetOptions()
    {
        OptionsContainer.Children.Clear();

        foreach (var option in Options)
        {
            var button = new Button
            {
                Content = option,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };
            button.Click += (sender, e) => OnOptionSelected(option);
            OptionsContainer.Children.Add(button);
        }
    }

    private void OnOptionSelected(string option)
    {
        OptionSelected?.Invoke(this, option);
    }
}