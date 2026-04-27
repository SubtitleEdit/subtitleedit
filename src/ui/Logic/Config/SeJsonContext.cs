using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Logic.Config;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(Se))]
internal partial class SeJsonContext : JsonSerializerContext
{
}
