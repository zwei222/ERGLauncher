extern alias MigratedCore;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using ERGLauncher.Services;
using ERGLauncher.ViewModels;
using ERGLauncher.Views;
using Microsoft.Extensions.DependencyInjection;
using CoreTheme = MigratedCore::ERGLauncher.Core.Models.Theme;
using AppSettingService = MigratedCore::ERGLauncher.Core.Services.AppSettingService;
using FileService = MigratedCore::ERGLauncher.Core.Services.FileService;
using GameSettingService = MigratedCore::ERGLauncher.Core.Services.GameSettingService;
using ResourceService = MigratedCore::ERGLauncher.Core.Services.ResourceService;
using ThemeService = MigratedCore::ERGLauncher.Core.Services.ThemeService;
using IAppSettingService = MigratedCore::ERGLauncher.Core.Services.IAppSettingService;
using IFileService = MigratedCore::ERGLauncher.Core.Services.IFileService;
using IGameSettingService = MigratedCore::ERGLauncher.Core.Services.IGameSettingService;
using IResourceService = MigratedCore::ERGLauncher.Core.Services.IResourceService;
using IThemeService = MigratedCore::ERGLauncher.Core.Services.IThemeService;
using ICoreDialogService = MigratedCore::ERGLauncher.Core.Services.IDialogService;

namespace ERGLauncher;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            // Core services (migrated). File service resolves settings next to the executable.
            services.AddSingleton<IFileService>(_ => new FileService());
            services.AddSingleton<IResourceService>(_ => new ResourceService(ResolveResourceString));
            services.AddSingleton<IThemeService>(_ => new ThemeService(ApplyThemeAsync));
            services.AddSingleton<IAppSettingService, AppSettingService>();
            services.AddSingleton<IGameSettingService, GameSettingService>();

            // Presentation-layer services.
            services.AddSingleton<ICoreDialogService, AvaloniaDialogService>();
            services.AddSingleton<IFilePickerService, FilePickerService>();
            services.AddSingleton<IViewDialogService, ViewDialogService>();

            // View models.
            services.AddSingleton<MainViewModel>();
            services.AddTransient<AddBrandViewModel>();
            services.AddTransient<AddProductViewModel>();
            services.AddTransient<SettingViewModel>();
            services.AddTransient<MessageViewModel>();

            // Views.
            services.AddSingleton<MainView>();

            serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            });

            var mainView = serviceProvider.GetRequiredService<MainView>();
            mainView.DataContext = serviceProvider.GetRequiredService<MainViewModel>();
            desktop.MainWindow = mainView;
            desktop.Exit += (_, _) => serviceProvider.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string? ResolveResourceString(string key, CultureInfo culture)
    {
        var previous = Properties.Resources.Culture;
        try
        {
            Properties.Resources.Culture = culture;
            return Properties.Resources.ResourceManager.GetString(key, culture);
        }
        finally
        {
            Properties.Resources.Culture = previous;
        }
    }

    private static ValueTask ApplyThemeAsync(CoreTheme theme, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        void Apply()
        {
            if (Current is null)
            {
                return;
            }

            Current.RequestedThemeVariant = theme switch
            {
                CoreTheme.Light => ThemeVariant.Light,
                CoreTheme.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Apply();
        }
        else
        {
            Dispatcher.UIThread.Post(Apply);
        }

        return ValueTask.CompletedTask;
    }
}
