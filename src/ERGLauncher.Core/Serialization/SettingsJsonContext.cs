using System.Text.Json.Serialization;
using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(RootItem))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;
