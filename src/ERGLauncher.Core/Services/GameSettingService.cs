using System.Text;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Serialization;

namespace ERGLauncher.Core.Services;

public sealed class GameSettingService : IGameSettingService
{
    private const string GameSettingsFileName = "gameSettings.json";
    private readonly IFileService fileService;
    private readonly string settingFilePath;
    private readonly string defaultIconFilePath;

    public GameSettingService(IFileService fileService)
    {
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.settingFilePath = Path.Combine(fileService.GetSettingsDirectoryPath(), GameSettingsFileName);
        this.defaultIconFilePath = fileService.GetDefaultIconFilePath();
    }

    public async ValueTask<RootItem> LoadSettingAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(this.settingFilePath))
        {
            return new RootItem([]);
        }

        var json = await File.ReadAllTextAsync(this.settingFilePath, cancellationToken).ConfigureAwait(false);
        var rootItem = SettingJsonSerializer.DeserializeGameSettings(json) ?? new RootItem([]);
        foreach (var brand in rootItem.Brands)
        {
            brand.Icon = await this.LoadIconAsync(brand.IconPath, cancellationToken).ConfigureAwait(false);
            foreach (var product in brand.Products)
            {
                product.Icon = await this.LoadIconAsync(product.IconPath, cancellationToken).ConfigureAwait(false);
                product.BrandName = brand.Name;
            }
        }

        return rootItem;
    }

    public async ValueTask SaveSettingAsync(RootItem? rootItem, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(this.fileService.GetSettingsDirectoryPath());
        var json = rootItem is null ? "null" : SettingJsonSerializer.Serialize(rootItem);
        await File.WriteAllTextAsync(this.settingFilePath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<Brand> CreateBrandItemAsync(string name, string? iconFilePath, CancellationToken cancellationToken = default)
    {
        var brand = new Brand([]) { Name = name, IconPath = iconFilePath };
        brand.Icon = await this.LoadIconAsync(iconFilePath, cancellationToken).ConfigureAwait(false);
        return brand;
    }

    public async ValueTask<Product> CreateProductItemAsync(
        string name,
        string? iconFilePath,
        string brandName,
        string gameFilePath,
        CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = name,
            IconPath = iconFilePath,
            BrandName = brandName,
            Path = gameFilePath,
        };
        product.Icon = await this.LoadIconAsync(iconFilePath, cancellationToken).ConfigureAwait(false);
        return product;
    }

    private ValueTask<Avalonia.Media.Imaging.Bitmap?> LoadIconAsync(string? iconFilePath, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrWhiteSpace(iconFilePath) ? this.defaultIconFilePath : iconFilePath;
        return this.fileService.CreateBitmapAsync(path, cancellationToken);
    }
}
