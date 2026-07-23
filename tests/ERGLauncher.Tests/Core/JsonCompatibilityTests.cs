using System.Text;
using System.Text.Json;
using ERGLauncher.Core;
using ERGLauncher.Core.Serialization;
using TUnit.Assertions;
using TUnit.Core;

namespace ERGLauncher.Tests.Core;

public sealed class JsonCompatibilityTests
{
    [Test]
    public async Task LegacyAppSettingsDeserializeWithCultureObjectAndNumericTheme()
    {
        var bytes = await ReadFixtureAsync("appSettings.json");

        var settings = JsonSerializer.Deserialize(bytes, AppSettingsJsonContext.Default.AppSettings);

        await Assert.That(settings).IsNotNull();
        await Assert.That(settings!.Culture.Name).IsEqualTo("ja-JP");
        await Assert.That(settings.Theme).IsEqualTo(Theme.Dark);
    }

    [Test]
    public async Task AppSettingsSerializeWithLegacyKeyStructure()
    {
        var settings = new AppSettings
        {
            Culture = System.Globalization.CultureInfo.GetCultureInfo("ja-JP"),
            Theme = Theme.Dark,
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(settings, AppSettingsJsonContext.Default.AppSettings);
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;

        await Assert.That(root.TryGetProperty("Culture", out var culture)).IsTrue();
        await Assert.That(culture.GetProperty("Name").GetString()).IsEqualTo("ja-JP");
        await Assert.That(root.GetProperty("Theme").GetInt32()).IsEqualTo(2);
        await Assert.That(root.TryGetProperty("culture", out _)).IsFalse();
        await Assert.That(bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble)).IsFalse();
    }

    [Test]
    public async Task LegacyGameSettingsDeserializeEntireItemTree()
    {
        var bytes = await ReadFixtureAsync("gameSettings.json");

        var root = JsonSerializer.Deserialize(bytes, GameSettingsJsonContext.Default.RootItem);

        await Assert.That(root).IsNotNull();
        await Assert.That(root!.Name).IsEqualTo("Root");
        var brand = root.Brands.Single();
        await Assert.That(brand.Name).IsEqualTo("Example Brand");
        var product = brand.Products.Single();
        await Assert.That(product.Name).IsEqualTo("Example Game");
        await Assert.That(product.Path).IsEqualTo(@"C:\Games\Example\game.exe");
    }

    [Test]
    public async Task GameSettingsSerializeWithPascalCaseAndWithoutIcon()
    {
        var product = new Product
        {
            Name = "Example Game",
            IconPath = "Assets/example-game.png",
            BrandName = "Example Brand",
            Path = @"C:\Games\Example\game.exe",
        };
        var rootItem = new RootItem([
            new Brand([product])
            {
                Name = "Example Brand",
                IconPath = "Assets/example-brand.png",
            },
        ])
        {
            Name = "Root",
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(rootItem, GameSettingsJsonContext.Default.RootItem);
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        var brand = root.GetProperty("Brands")[0];
        var serializedProduct = brand.GetProperty("Products")[0];

        await Assert.That(root.TryGetProperty("Name", out _)).IsTrue();
        await Assert.That(root.TryGetProperty("Icon", out _)).IsFalse();
        await Assert.That(brand.TryGetProperty("Icon", out _)).IsFalse();
        await Assert.That(serializedProduct.TryGetProperty("Icon", out _)).IsFalse();
        await Assert.That(serializedProduct.GetProperty("BrandName").GetString()).IsEqualTo("Example Brand");
    }

    private static Task<byte[]> ReadFixtureAsync(string fileName) =>
        File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));
}
