using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Services;

public interface IGameSettingService
{
    ValueTask<RootItem> LoadSettingAsync(CancellationToken cancellationToken = default);

    ValueTask SaveSettingAsync(RootItem? rootItem, CancellationToken cancellationToken = default);

    ValueTask<Brand> CreateBrandItemAsync(string name, string? iconFilePath, CancellationToken cancellationToken = default);

    ValueTask<Product> CreateProductItemAsync(
        string name,
        string? iconFilePath,
        string brandName,
        string gameFilePath,
        CancellationToken cancellationToken = default);
}
