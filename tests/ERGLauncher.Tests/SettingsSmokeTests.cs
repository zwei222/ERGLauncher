using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Tests;

public sealed class SettingsSmokeTests
{
    [Test]
    public async Task RunAsync_LoadsLegacySettingsAndPersistsCrudAcrossRestart()
    {
        using var directory = new TemporaryDirectory();
        var settingsDirectory = Path.Combine(directory.Path, "settings");
        Directory.CreateDirectory(settingsDirectory);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "appSettings.json"), Path.Combine(settingsDirectory, "appSettings.json"));
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "gameSettings.json"), Path.Combine(settingsDirectory, "gameSettings.json"));

        var exitCode = await SettingsSmoke.RunAsync(directory.Path);
        var restartedAppSettings = await new AppSettingService(new FileService(directory.Path)).LoadAppSettingAsync();
        var restartedGameSettings = await new GameSettingService(new FileService(directory.Path)).LoadSettingAsync();

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(restartedAppSettings).IsNotNull();
        await Assert.That(restartedAppSettings!.Culture.Name).IsEqualTo("en-US");
        await Assert.That(restartedAppSettings.Theme).IsEqualTo(Theme.Light);
        var smokeBrand = restartedGameSettings.Brands.Single(brand => brand.Name == SettingsSmoke.BrandName);
        await Assert.That(smokeBrand.Products.Single().Name).IsEqualTo(SettingsSmoke.UpdatedProductName);
        await Assert.That(smokeBrand.Products.Any(product => product.Name == SettingsSmoke.DeletedProductName)).IsFalse();
    }
}
