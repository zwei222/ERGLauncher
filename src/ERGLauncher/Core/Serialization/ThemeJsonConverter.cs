using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERGLauncher.Core.Serialization;

public sealed class ThemeJsonConverter : JsonConverter<Theme>
{
    public override Theme Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Theme value;
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                if (!Enum.TryParse(reader.GetString(), ignoreCase: true, out value))
                {
                    throw new JsonException("Theme must be a known theme name.");
                }

                break;
            case JsonTokenType.Number:
                if (!reader.TryGetInt32(out var number))
                {
                    throw new JsonException("Theme must be a valid integer value.");
                }

                value = (Theme)number;
                break;
            default:
                throw new JsonException("Theme must be a string or integer value.");
        }

        return Enum.IsDefined(value)
            ? value
            : throw new JsonException("Theme must be a defined value.");
    }

    public override void Write(Utf8JsonWriter writer, Theme value, JsonSerializerOptions options)
    {
        if (!Enum.IsDefined(value))
        {
            throw new JsonException("Theme must be a defined value.");
        }

        writer.WriteStringValue(value.ToString());
    }
}
