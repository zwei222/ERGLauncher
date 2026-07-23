using System.Text.Json;
using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Serialization;

public static class SettingJsonSerializer
{
    public static AppSettings? DeserializeAppSettings(string json) =>
        JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings);

    public static RootItem? DeserializeGameSettings(string json) =>
        JsonSerializer.Deserialize(json, SettingsJsonContext.Default.RootItem);

    public static string Serialize(RootItem settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return JsonSerializer.Serialize(settings, SettingsJsonContext.Default.RootItem);
    }

    public static string Serialize(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return JsonSerializer.Serialize(settings, SettingsJsonContext.Default.AppSettings);
    }
}
