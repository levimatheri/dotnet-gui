using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestApp;
public class StringToStringArrayConverter : JsonConverter<string[]>
{
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            return new[] { str! };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var items = new List<string>();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                reader.Read();

                if (reader.TokenType == JsonTokenType.String)
                {
                    items.Add(reader.GetString()!);
                }
            }

            return items.ToArray();
        }
        else
        {
            throw new NotSupportedException("Unexpected data type. Only string or string arrays are supported.");
        }
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }

        writer.WriteEndArray();
    }
}
