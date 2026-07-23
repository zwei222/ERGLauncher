using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Tests.Services;

public sealed class GameSettingServiceTests
{
    [Test]
    public async Task LoadReturnsEmptyRootWhenSettingsFileDoesNotExist()
    {
        using var directory = new TemporaryDirectory();
        var service = new GameSettingService(new FileService(directory.Path));

        var root = await service.LoadSettingAsync();

        await Assert.That(root.Brands).IsEmpty();
    }

    [Test]
    public async Task LoadReturnsEmptyRootWhenSettingsFileContainsNull()
    {
        using var directory = new TemporaryDirectory();
        var settingsDirectory = System.IO.Path.Combine(directory.Path, "settings");
        Directory.CreateDirectory(settingsDirectory);
        await File.WriteAllTextAsync(System.IO.Path.Combine(settingsDirectory, "gameSettings.json"), "null");
        var service = new GameSettingService(new FileService(directory.Path));

        var root = await service.LoadSettingAsync();

        await Assert.That(root.Brands).IsEmpty();
    }

    [Test]
    public async Task SaveThenLoadRoundTripsItemTree()
    {
        using var directory = new TemporaryDirectory();
        var service = new GameSettingService(new FileService(directory.Path));
        var root = new RootItem([
            new Brand([
                new Product { Name = "Title", BrandName = "Studio", Path = "game" },
            ]) { Name = "Studio" },
        ]) { Name = "Root" };

        await service.SaveSettingAsync(root);
        var loaded = await service.LoadSettingAsync();

        await Assert.That(loaded.Name).IsEqualTo("Root");
        await Assert.That(loaded.Brands.Single().Products.Single().Name).IsEqualTo("Title");
        await Assert.That(loaded.Brands.Single().Products.Single().BrandName).IsEqualTo("Studio");
    }

    [Test]
    public async Task CreateBrandItemBuildsNamedEmptyBrand()
    {
        using var directory = new TemporaryDirectory();
        var service = new GameSettingService(new FileService(directory.Path));

        var brand = await service.CreateBrandItemAsync("Studio", null);

        await Assert.That(brand.Name).IsEqualTo("Studio");
        await Assert.That(brand.Products).IsEmpty();
        await Assert.That(brand.Icon).IsNull();
    }

    [Test]
    public async Task CreateProductItemCopiesAllPersistentValues()
    {
        using var directory = new TemporaryDirectory();
        var service = new GameSettingService(new FileService(directory.Path));

        var product = await service.CreateProductItemAsync("Title", null, "Studio", "/games/title");

        await Assert.That(product.Name).IsEqualTo("Title");
        await Assert.That(product.BrandName).IsEqualTo("Studio");
        await Assert.That(product.Path).IsEqualTo("/games/title");
        await Assert.That(product.Icon).IsNull();
    }
}
