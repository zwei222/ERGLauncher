using System.Globalization;
using System.Text.Json.Serialization;
using ERGLauncher.Core.Serialization;

namespace ERGLauncher.Core;

/// <summary>
/// Application settings persisted in appSettings.json.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Gets or sets the UI culture.
    /// </summary>
    [JsonConverter(typeof(CultureInfoJsonConverter))]
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    /// <summary>
    /// Gets or sets the requested theme.
    /// </summary>
    public Theme Theme { get; set; }
}
