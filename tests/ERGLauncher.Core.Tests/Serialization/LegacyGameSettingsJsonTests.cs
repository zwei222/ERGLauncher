using System.Text.Json;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Serialization;

namespace ERGLauncher.Core.Tests.Serialization;

public sealed class LegacyGameSettingsJsonTests
{
    private const string LegacyJson = """
        {"Brands":[{"Products":[{"Path":"games/title/game.exe","BrandName":"Studio","Name":"Title","IconPath":null}],"Name":"Studio","IconPath":"icons/studio.png"}],"Name":"root","IconPath":null}
        """;

    [Test]
    public async Task Deserialize_reads_legacy_root_brand_and_product_shape()
    {
        var root = SettingJsonSerializer.DeserializeGameSettings(LegacyJson);

        await Assert.That(root).IsNotNull();
        await Assert.That(root!.Name).IsEqualTo("root");
        var brand = root.Brands.Single();
        await Assert.That(brand.Name).IsEqualTo("Studio");
        await Assert.That(brand.IconPath).IsEqualTo("icons/studio.png");
        var product = brand.Products.Single();
        await Assert.That(product.Name).IsEqualTo("Title");
        await Assert.That(product.BrandName).IsEqualTo("Studio");
        await Assert.That(product.Path).IsEqualTo("games/title/game.exe");
    }

    [Test]
    public async Task Serialize_writes_legacy_names_without_runtime_only_properties()
    {
        var root = SettingJsonSerializer.DeserializeGameSettings(LegacyJson)!;

        var json = SettingJsonSerializer.Serialize(root);
        using var document = JsonDocument.Parse(json);
        var product = document.RootElement.GetProperty("Brands")[0].GetProperty("Products")[0];

        await Assert.That(document.RootElement.GetProperty("Name").GetString()).IsEqualTo("root");
        await Assert.That(product.GetProperty("Path").GetString()).IsEqualTo("games/title/game.exe");
        await Assert.That(product.TryGetProperty("Icon", out _)).IsFalse();
    }
}
