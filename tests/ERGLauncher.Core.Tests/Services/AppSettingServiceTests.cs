using System.Globalization;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Tests.Services;

public sealed class AppSettingServiceTests
{
    [Test]
    public async Task Load_returns_null_when_legacy_file_does_not_exist()
    {
        using var directory = new TemporaryDirectory();
        var service = new AppSettingService(new FileService(directory.Path));

        var settings = await service.LoadAppSettingAsync();

        await Assert.That(settings).IsNull();
    }

    [Test]
    public async Task Save_and_load_use_legacy_file_name_and_json_shape()
    {
        using var directory = new TemporaryDirectory();
        var service = new AppSettingService(new FileService(directory.Path));
        var settings = new AppSettings
        {
            Culture = CultureInfo.GetCultureInfo("ja-JP"),
            Theme = Theme.Dark,
        };

        await service.SaveAppSettingAsync(settings);
        var json = await File.ReadAllTextAsync(System.IO.Path.Combine(directory.Path, "settings", "appSettings.json"));
        var loaded = await service.LoadAppSettingAsync();

        await Assert.That(json).IsEqualTo("""{"Culture":{"Name":"ja-JP"},"Theme":2}""");
        await Assert.That(loaded!.Culture.Name).IsEqualTo("ja-JP");
        await Assert.That(loaded.Theme).IsEqualTo(Theme.Dark);
    }
}
