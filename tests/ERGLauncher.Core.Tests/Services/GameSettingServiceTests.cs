using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Tests.Services;

public sealed class GameSettingServiceTests
{
    [Test]
    public async Task Load_returns_empty_root_when_legacy_file_does_not_exist()
    {
        using var directory = new TemporaryDirectory();
        var service = new GameSettingService(new FileService(directory.Path));

        var root = await service.LoadSettingAsync();

        await Assert.That(root.Brands).IsEmpty();
    }

    [Test]
    public async Task Load_normalizes_brand_name_and_loads_cross_platform_icons()
    {
        using var directory = new TemporaryDirectory();
        var settingsDirectory = System.IO.Path.Combine(directory.Path, "settings");
        var assetsDirectory = System.IO.Path.Combine(directory.Path, "Assets");
        Directory.CreateDirectory(settingsDirectory);
        Directory.CreateDirectory(assetsDirectory);
        await File.WriteAllBytesAsync(
            System.IO.Path.Combine(assetsDirectory, "icon.png"),
            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="));
        await File.WriteAllTextAsync(
            System.IO.Path.Combine(settingsDirectory, "gameSettings.json"),
            """{"Brands":[{"Products":[{"Path":"game","BrandName":"old","Name":"Title","IconPath":null}],"Name":"Studio","IconPath":null}],"Name":"","IconPath":null}""");
        var service = new GameSettingService(new FileService(directory.Path));

        var root = await service.LoadSettingAsync();
        var brand = root.Brands.Single();
        var product = brand.Products.Single();

        await Assert.That(product.BrandName).IsEqualTo("Studio");
        await Assert.That(brand.Icon).IsNotNull();
        await Assert.That(product.Icon).IsNotNull();
    }

    [Test]
    public async Task Save_writes_legacy_game_settings_file_that_can_be_loaded()
    {
        using var directory = new TemporaryDirectory();
        var fileService = new FileService(directory.Path);
        var service = new GameSettingService(fileService);
        var root = new RootItem([
            new Brand([
                new Product { Name = "Title", BrandName = "Studio", Path = "game" },
            ]) { Name = "Studio" },
        ]);

        await service.SaveSettingAsync(root);
        var loaded = await service.LoadSettingAsync();

        await Assert.That(File.Exists(System.IO.Path.Combine(directory.Path, "settings", "gameSettings.json"))).IsTrue();
        await Assert.That(loaded.Brands.Single().Products.Single().Name).IsEqualTo("Title");
    }
}
