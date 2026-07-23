using System.Globalization;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Tests.Services;

public sealed class AppSettingServiceTests
{
    [Test]
    public async Task LoadReturnsNullWhenSettingsFileDoesNotExist()
    {
        using var directory = new TemporaryDirectory();
        var service = new AppSettingService(new FileService(directory.Path));

        var settings = await service.LoadAppSettingAsync();

        await Assert.That(settings).IsNull();
    }

    [Test]
    public async Task SaveThenLoadRoundTripsLegacySettingsShape()
    {
        using var directory = new TemporaryDirectory();
        var service = new AppSettingService(new FileService(directory.Path));
        var settings = new AppSettings
        {
            Culture = CultureInfo.GetCultureInfo("ja-JP"),
            Theme = Theme.Dark,
        };

        await service.SaveAppSettingAsync(settings);
        var path = System.IO.Path.Combine(directory.Path, "settings", "appSettings.json");
        var bytes = await File.ReadAllBytesAsync(path);
        var loaded = await service.LoadAppSettingAsync();

        await Assert.That(System.Text.Encoding.UTF8.GetString(bytes))
            .IsEqualTo("""{"Culture":{"Name":"ja-JP"},"Theme":2}""");
        await Assert.That(loaded).IsNotNull();
        await Assert.That(loaded!.Culture.Name).IsEqualTo("ja-JP");
        await Assert.That(loaded.Theme).IsEqualTo(Theme.Dark);
    }
}
