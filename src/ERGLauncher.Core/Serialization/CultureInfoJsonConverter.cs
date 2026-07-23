using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERGLauncher.Core.Serialization;

public sealed class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    public override bool HandleNull => true;

    public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return CultureInfo.CurrentUICulture;
        }

        using var document = JsonDocument.ParseValue(ref reader);
        if (!document.RootElement.TryGetProperty(nameof(CultureInfo.Name), out var nameElement))
        {
            throw new JsonException("Culture must contain a Name property.");
        }

        var name = nameElement.GetString();
        return name is null ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(name);
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString(nameof(CultureInfo.Name), value.Name);
        writer.WriteEndObject();
    }
}
