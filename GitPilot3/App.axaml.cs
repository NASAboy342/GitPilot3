using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GitPilot3;

public partial class App : Application
{
    private readonly IServiceProvider? _serviceProvider;

    public App() { } // Parameterless constructor for designer

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _serviceProvider?.GetService<MainWindow>() 
                                ?? throw new InvalidOperationException("MainWindow service not found.");
        }

        base.OnFrameworkInitializationCompleted();
    }
}