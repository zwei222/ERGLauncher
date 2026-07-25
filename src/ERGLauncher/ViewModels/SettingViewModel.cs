extern alias MigratedCore;

using System;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewTheme = ERGLauncher.Core.Theme;
using CoreTheme = MigratedCore::ERGLauncher.Core.Models.Theme;
using IResourceService = MigratedCore::ERGLauncher.Core.Services.IResourceService;
using IThemeService = MigratedCore::ERGLauncher.Core.Services.IThemeService;

namespace ERGLauncher.ViewModels;

/// <summary>
/// View model for the application settings dialog. Applies language and theme
/// selections through the migrated core resource/theme services.
/// </summary>
public sealed partial class SettingViewModel : DialogViewModelBase
{
    private readonly IResourceService resourceService;
    private readonly IThemeService themeService;

    [ObservableProperty]
    private CultureInfo? selectedLanguage;

    [ObservableProperty]
    private ViewTheme selectedTheme;

    public SettingViewModel(IResourceService resourceService, IThemeService themeService)
    {
        this.resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        this.themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        selectedLanguage = resourceService.CurrentCulture;
        selectedTheme = (ViewTheme)(int)themeService.CurrentTheme;
        OkAsyncCommand = new AsyncRelayCommand(OkAsync, () => !IsBusy);
        ApplyAsyncCommand = new AsyncRelayCommand(ApplyAsync, () => !IsBusy);
    }

    public IAsyncRelayCommand OkAsyncCommand { get; }

    public IAsyncRelayCommand ApplyAsyncCommand { get; }

    protected override void OnBusyStateChanged()
    {
        OkAsyncCommand.NotifyCanExecuteChanged();
        ApplyAsyncCommand.NotifyCanExecuteChanged();
    }

    private async Task OkAsync()
    {
        await ApplyAsync().ConfigureAwait(true);
        RaiseRequestClose(true);
    }

    private async Task ApplyAsync()
    {
        using var busy = BeginBusy();
        resourceService.ChangeCulture(SelectedLanguage);
        await themeService.ChangeThemeAsync((CoreTheme)(int)SelectedTheme).ConfigureAwait(true);
    }
}
