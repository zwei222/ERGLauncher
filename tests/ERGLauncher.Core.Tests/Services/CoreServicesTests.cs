using System.Globalization;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Tests.Services;

public sealed class CoreServicesTests
{
    [Test]
    public async Task FileService_copies_icons_into_cross_platform_assets_directory()
    {
        using var directory = new TemporaryDirectory();
        var sourcePath = System.IO.Path.Combine(directory.Path, "source.ico");
        await File.WriteAllBytesAsync(sourcePath, [4, 5, 6]);
        var service = new FileService(System.IO.Path.Combine(directory.Path, "app"));

        var copiedPath = await service.CopyIconFileAsync(sourcePath);

        await Assert.That(copiedPath).IsNotNull();
        await Assert.That(System.IO.Path.GetDirectoryName(copiedPath)).IsEqualTo(System.IO.Path.Combine(directory.Path, "app", "Assets"));
        await Assert.That(await File.ReadAllBytesAsync(copiedPath!)).IsEquivalentTo(new byte[] { 4, 5, 6 });
    }

    [Test]
    public async Task ResourceService_changes_culture_without_global_WPF_localization_state()
    {
        var service = new ResourceService(
            (key, culture) => $"{key}:{culture.Name}",
            CultureInfo.GetCultureInfo("en-US"));

        service.ChangeCulture(CultureInfo.GetCultureInfo("ja-JP"));

        await Assert.That(service.CurrentCulture.Name).IsEqualTo("ja-JP");
        await Assert.That(service.GetCultureString("Greeting")).IsEqualTo("Greeting:ja-JP");
    }

    [Test]
    public async Task ThemeService_applies_theme_through_UI_independent_callback()
    {
        Theme? applied = null;
        var service = new ThemeService((theme, _) =>
        {
            applied = theme;
            return ValueTask.CompletedTask;
        });

        await service.ChangeThemeAsync(Theme.Dark);

        await Assert.That(service.CurrentTheme).IsEqualTo(Theme.Dark);
        await Assert.That(applied).IsEqualTo(Theme.Dark);
    }

    [Test]
    public async Task CommunityToolkit_models_raise_property_changed_notifications()
    {
        var product = new Product();
        string? changedProperty = null;
        product.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

        product.Name = "Title";

        await Assert.That(changedProperty).IsEqualTo(nameof(Product.Name));
    }
}
