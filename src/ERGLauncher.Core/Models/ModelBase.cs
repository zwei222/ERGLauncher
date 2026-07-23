using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Models;

public abstract class ModelBase : ObservableObject, IModelBase
{
    private readonly IResourceService resourceService;
    private readonly IThemeService themeService;
    private string? title;
    private double height;
    private double width;
    private double top;
    private double left;

    protected ModelBase(IResourceService? resourceService = null, IThemeService? themeService = null)
    {
        this.resourceService = resourceService ?? new ResourceService();
        this.themeService = themeService ?? new ThemeService();
    }

    public string? Title { get => this.title; set => this.SetProperty(ref this.title, value); }
    public double Height { get => this.height; set => this.SetProperty(ref this.height, value); }
    public double Width { get => this.width; set => this.SetProperty(ref this.width, value); }
    public double Top { get => this.top; set => this.SetProperty(ref this.top, value); }
    public double Left { get => this.left; set => this.SetProperty(ref this.left, value); }
    public CultureInfo CurrentCulture => this.resourceService.CurrentCulture;
    public Theme CurrentTheme => this.themeService.CurrentTheme;
    public string? GetCultureString(string key) => this.resourceService.GetCultureString(key);
    protected void ChangeCulture(CultureInfo? culture) => this.resourceService.ChangeCulture(culture);
    protected ValueTask ChangeThemeAsync(Theme theme, CancellationToken cancellationToken = default) =>
        this.themeService.ChangeThemeAsync(theme, cancellationToken);
}
