using System.Globalization;
using ERGLauncher.Core.Models;
using ERGLauncher.Core.Serialization;

namespace ERGLauncher.Core.Tests.Serialization;

public sealed class LegacyAppSettingsJsonTests
{
    [Test]
    public async Task Deserialize_reads_legacy_culture_object_and_numeric_theme()
    {
        const string json = """{"Culture":{"Name":"ja-JP"},"Theme":2}""";

        var settings = SettingJsonSerializer.DeserializeAppSettings(json);

        await Assert.That(settings).IsNotNull();
        await Assert.That(settings!.Culture.Name).IsEqualTo("ja-JP");
        await Assert.That(settings.Theme).IsEqualTo(Theme.Dark);
    }

    [Test]
    public async Task Deserialize_null_culture_uses_current_ui_culture()
    {
        var settings = SettingJsonSerializer.DeserializeAppSettings("""{"Culture":null,"Theme":0}""");

        await Assert.That(settings!.Culture).IsEqualTo(CultureInfo.CurrentUICulture);
    }

    [Test]
    public async Task Serialize_writes_legacy_property_names_and_shapes()
    {
        var settings = new AppSettings
        {
            Culture = CultureInfo.GetCultureInfo("en-US"),
            Theme = Theme.Light,
        };

        var json = SettingJsonSerializer.Serialize(settings);

        await Assert.That(json).IsEqualTo("""{"Culture":{"Name":"en-US"},"Theme":1}""");
    }
}
