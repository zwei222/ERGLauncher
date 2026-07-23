using ERGLauncher.Core.Services;
using Avalonia.Media.Imaging;

namespace ERGLauncher.Core.Models;

public sealed class AddBrandModel : ModelBase, IAddBrandModel
{
    private readonly IFileService fileService;
    private string name = string.Empty;
    private string? iconPath;
    private Bitmap? icon;

    public AddBrandModel(IFileService fileService, IResourceService? resourceService = null, IThemeService? themeService = null)
        : base(resourceService, themeService)
    {
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.Height = 300;
        this.Width = 300;
    }

    public string Name { get => this.name; set => this.SetProperty(ref this.name, value); }
    public string? IconPath { get => this.iconPath; set => this.SetProperty(ref this.iconPath, value); }
    public Bitmap? Icon { get => this.icon; private set => this.SetProperty(ref this.icon, value); }

    public async ValueTask SelectIconAsync(string filePath, CancellationToken cancellationToken = default)
    {
        this.IconPath = filePath;
        this.Icon = await this.fileService.CreateBitmapAsync(filePath, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<Brand> AddBrandAsync(CancellationToken cancellationToken = default) =>
        new([])
        {
            Icon = this.Icon,
            IconPath = await this.fileService.CopyIconFileAsync(this.IconPath, cancellationToken).ConfigureAwait(false),
            Name = this.Name,
        };

    public void LoadBrand(Brand brand)
    {
        ArgumentNullException.ThrowIfNull(brand);
        this.Name = brand.Name;
        this.Icon = brand.Icon;
        this.IconPath = brand.IconPath;
    }
}
