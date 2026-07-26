using System.Globalization;
using System.Text.Json.Serialization;
using ERGLauncher.Core.Serialization;

namespace ERGLauncher.Core.Models;

public sealed class AppSettings
{
    [JsonConverter(typeof(CultureInfoJsonConverter))]
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    [JsonConverter(typeof(ThemeJsonConverter))]
    public Theme Theme { get; set; }
}
