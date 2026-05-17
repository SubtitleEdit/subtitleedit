using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Logic.Plugins;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    WriteIndented = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(PluginManifest))]
[JsonSerializable(typeof(PluginRequest))]
[JsonSerializable(typeof(PluginResponse))]
[JsonSerializable(typeof(PluginIndex))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class PluginJsonContext : JsonSerializerContext
{
}
