using System.Globalization;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Tests.Models;

public sealed class PresentationModelTests
{
    [Test]
    public async Task Icon_properties_use_Avalonia_bitmap()
    {
        var brandIconType = typeof(IAddBrandModel).GetProperty(nameof(IAddBrandModel.Icon))!.PropertyType;
        var productIconType = typeof(IAddProductModel).GetProperty(nameof(IAddProductModel.Icon))!.PropertyType;

        await Assert.That(brandIconType.FullName).IsEqualTo("Avalonia.Media.Imaging.Bitmap");
        await Assert.That(productIconType.FullName).IsEqualTo("Avalonia.Media.Imaging.Bitmap");
    }

    [Test]
    public async Task SettingModel_applies_selected_culture_and_theme()
    {
        var resources = new ResourceService(initialCulture: CultureInfo.GetCultureInfo("en-US"));
        var themes = new ThemeService();
        var model = new SettingModel(resources, themes)
        {
            SelectedLanguage = CultureInfo.GetCultureInfo("ja-JP"),
            SelectedTheme = Theme.Dark,
        };

        await model.ApplyAsync();

        await Assert.That(resources.CurrentCulture.Name).IsEqualTo("ja-JP");
        await Assert.That(themes.CurrentTheme).IsEqualTo(Theme.Dark);
    }

    [Test]
    public async Task AddBrandModel_copies_selected_icon_and_creates_brand()
    {
        using var directory = new TemporaryDirectory();
        var source = System.IO.Path.Combine(directory.Path, "brand.png");
        await File.WriteAllBytesAsync(
            source,
            Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="));
        var model = new AddBrandModel(new FileService(System.IO.Path.Combine(directory.Path, "app")))
        {
            Name = "Studio",
            IconPath = source,
        };

        var brand = await model.AddBrandAsync();

        await Assert.That(brand.Name).IsEqualTo("Studio");
        await Assert.That(brand.Icon).IsNull();
        await Assert.That(File.Exists(brand.IconPath!)).IsTrue();
    }

    [Test]
    public async Task AddProductModel_select_file_is_cross_platform_safe()
    {
        using var directory = new TemporaryDirectory();
        var executable = System.IO.Path.Combine(directory.Path, "game");
        await File.WriteAllTextAsync(executable, string.Empty);
        var model = new AddProductModel(new FileService(directory.Path));

        await model.SelectFileAsync(executable);

        await Assert.That(model.Path).IsEqualTo(executable);
        if (!OperatingSystem.IsWindows())
        {
            await Assert.That(model.Icon).IsNull();
        }
    }
}
