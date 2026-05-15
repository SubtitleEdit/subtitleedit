using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Logic.Config;

// WriteIndented makes Settings.json human-readable so users can diff older
// versions or hand-edit in a text editor (issue #10936). The file grows by
// ~2x, but it lives outside hot paths and the cost is negligible.
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    WriteIndented = true)]
[JsonSerializable(typeof(Se))]
internal partial class SeJsonContext : JsonSerializerContext
{
}
