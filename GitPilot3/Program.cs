using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using GitPilot3.Services;
using System;
using GitPilot3.Repositories;
using GitPilot3.UserControlles;

namespace GitPilot3;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        // Create service collection
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        return AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register your services here
        services.AddSingleton<IGitRepositoryService, GitRepositoryService>();
        services.AddSingleton<IUserProfileService, UserProfileService>();
        services.AddSingleton<IAppRepository, AppRepository>();
        services.AddSingleton<IAppStageService, AppStageService>();
        services.AddSingleton<ErrorMessageHandler, ErrorMessageHandler>();
        services.AddSingleton<IGraphComponentService, GraphComponentService>();

        services.AddTransient<MainWindow>();
        services.AddTransient<ProfileManagementWindow>();
        services.AddTransient<ProfileMenagement>();
        services.AddTransient<AddAccount>();
        services.AddTransient<EditAccount>();
        services.AddTransient<LocalbrancheFlyout>();
        services.AddTransient<CommonSimpleInput>();
        services.AddTransient<CommonConfirmation>();
        services.AddTransient<CommonOptionsForFlyout>();
        // Add more services as needed when you create them
    }
}