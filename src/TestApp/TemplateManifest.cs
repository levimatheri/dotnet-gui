﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestApp;
public record class TemplateManifest(
    string Identity,
    string GroupIdentity,
    string Name,
    string Author,
    string[] Classifications,
    string Description,
    string[] ShortName,
    TemplateTags? Tags)
{
    [JsonConverter(typeof(StringToStringArrayConverter))]
    public string[] ShortName { get; init; } = ShortName;
}

public record class TemplateTags(string? Language, string Type);

public record class TemplateIdeHostManifest(string? Icon, string? LearnMoreLink);

public record CompositeTemplateManifest(
    string PackageName,
    string Version,
    string? Base64Icon,
    bool IsBuiltIn,
    TemplateManifest TemplateManifest,
    TemplateIdeHostManifest? IdeHostManifest)
{
    public string[]? Languages { get; init; }
}
