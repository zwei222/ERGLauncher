using ERGLauncher.Core.Services;
using Avalonia.Media.Imaging;

namespace ERGLauncher.Core.Models;

public sealed class AddProductModel : ModelBase, IAddProductModel
{
    private readonly IFileService fileService;
    private string name = string.Empty;
    private string? iconPath;
    private Bitmap? icon;
    private string path = string.Empty;

    public AddProductModel(IFileService fileService, IResourceService? resourceService = null, IThemeService? themeService = null)
        : base(resourceService, themeService)
    {
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.Height = 400;
        this.Width = 300;
    }

    public string Name { get => this.name; set => this.SetProperty(ref this.name, value); }
    public string? IconPath { get => this.iconPath; set => this.SetProperty(ref this.iconPath, value); }
    public Bitmap? Icon { get => this.icon; private set => this.SetProperty(ref this.icon, value); }
    public string Path { get => this.path; set => this.SetProperty(ref this.path, value); }

    public async ValueTask SelectIconAsync(string filePath, CancellationToken cancellationToken = default)
    {
        this.IconPath = filePath;
        this.Icon = await this.fileService.CreateBitmapAsync(filePath, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask SelectFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        this.Path = filePath;
        if (!string.IsNullOrWhiteSpace(this.IconPath))
        {
            return;
        }

        this.IconPath = await this.fileService.ExtractAssociatedIconAsync(filePath, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(this.IconPath))
        {
            return;
        }

        this.Icon = await this.fileService.CreateBitmapAsync(this.IconPath, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<Product> AddProductAsync(CancellationToken cancellationToken = default) =>
        new()
        {
            Name = this.Name,
            Icon = this.Icon,
            IconPath = await this.fileService.CopyIconFileAsync(this.IconPath, cancellationToken).ConfigureAwait(false),
            Path = this.Path,
        };

    public void LoadProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        this.Name = product.Name;
        this.IconPath = product.IconPath;
        this.Icon = product.Icon;
        this.Path = product.Path;
    }
}
