using System.Text.Json.Serialization;

namespace ERGLauncher.Core.Serialization;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(RootItem))]
public sealed partial class GameSettingsJsonContext : JsonSerializerContext;
