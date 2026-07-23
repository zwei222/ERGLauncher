using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERGLauncher.Core.Serialization;

/// <summary>
/// Reads and writes the legacy Utf8Json culture object shape.
/// </summary>
public sealed class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    public override bool HandleNull => true;

    public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return CultureInfo.CurrentUICulture;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Culture must be an object containing a Name property.");
        }

        string? name = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Invalid culture object.");
            }

            var propertyName = reader.GetString();
            if (!reader.Read())
            {
                throw new JsonException("Incomplete culture object.");
            }

            if (propertyName == nameof(CultureInfo.Name))
            {
                name = reader.TokenType == JsonTokenType.String ? reader.GetString() : throw new JsonException("Culture Name must be a string.");
            }
            else
            {
                reader.Skip();
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new JsonException("Culture object must contain a non-empty Name property.");
        }

        try
        {
            return CultureInfo.GetCultureInfo(name);
        }
        catch (CultureNotFoundException exception)
        {
            throw new JsonException($"Unknown culture '{name}'.", exception);
        }
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
