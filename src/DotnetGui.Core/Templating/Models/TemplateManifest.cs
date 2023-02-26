using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DotnetGui.Core.Templating.Models;
public record class TemplateManifest(
    string Identity,
    string GroupIdentity,
    string Name,
    string Author,
    string[] Classifications,
    string Description,
    string[] ShortName,
    TemplateTags Tags)
{
    [JsonConverter(typeof(StringToStringArrayConverter))]
    public string[] ShortName { get; init; } = ShortName;

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
}

public record class TemplateTags(string Language, string Type);

public record class TemplateIdeHostManifest(string Icon, string LearnMoreLink);

public record CompositeTemplateManifest(
    string PackageName,
    string Version,
    string? Base64Icon,
    bool IsBuiltIn,
    TemplateManifest TemplateManifest,
    TemplateIdeHostManifest IdeHostManifest)
{
    public string[] Languages { get; init; }
}


