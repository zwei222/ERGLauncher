using System.Globalization;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Models;

public sealed class SettingModel : ModelBase, ISettingModel
{
    private CultureInfo? selectedLanguage;
    private Theme selectedTheme;

    public SettingModel(IResourceService resourceService, IThemeService themeService)
        : base(resourceService, themeService)
    {
        this.Height = 300;
        this.Width = 500;
        this.selectedLanguage = resourceService.CurrentCulture;
        this.selectedTheme = themeService.CurrentTheme;
    }

    public CultureInfo? SelectedLanguage
    {
        get => this.selectedLanguage;
        set => this.SetProperty(ref this.selectedLanguage, value);
    }

    public Theme SelectedTheme
    {
        get => this.selectedTheme;
        set => this.SetProperty(ref this.selectedTheme, value);
    }

    public async ValueTask ApplyAsync(CancellationToken cancellationToken = default)
    {
        this.ChangeCulture(this.SelectedLanguage);
        await this.ChangeThemeAsync(this.SelectedTheme, cancellationToken).ConfigureAwait(false);
    }
}
