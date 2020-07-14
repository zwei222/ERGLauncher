using System.Globalization;
using Utf8Json;

namespace ERGLauncher.Core.JsonFormatters
{
    /// <summary>
    /// Json formatter for <see cref="System.Globalization.CultureInfo" />.
    /// </summary>
    public class CultureInfoJsonFormatter : IJsonFormatter<CultureInfo>
    {
        /// <summary>
        /// Serialize the value.
        /// </summary>
        /// <param name="writer">Json writer</param>
        /// <param name="value">The value to serialized</param>
        /// <param name="formatterResolver">Json formatter resolver</param>
        public void Serialize(ref JsonWriter writer, CultureInfo value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return;
            }

            writer.WriteBeginObject();
            writer.WritePropertyName(nameof(value.Name));
            writer.WriteString(value.Name);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Deserialize the value.
        /// </summary>
        /// <param name="reader">Json reader</param>
        /// <param name="formatterResolver">Json formatter resolver</param>
        /// <returns>Deserialized value</returns>
        public CultureInfo Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return CultureInfo.CurrentUICulture;
            }

            reader.ReadIsBeginObject();
            reader.ReadPropertyName();

            var name = reader.ReadString();

            reader.ReadIsEndObject();

            return new CultureInfo(name);
        }
    }
}
