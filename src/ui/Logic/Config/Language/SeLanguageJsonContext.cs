using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

// Translations are deserialized on the startup path, before the first window is built.
// SeLanguage is a ~66-class / ~3300-property graph and each language file is ~185 KB, so
// the reflection-based serializer spent ~200-350 ms building metadata for it on every
// launch by a non-English user. Source generation moves that work to compile time.
// UnmappedMemberHandling.Skip keeps older/newer translation files loadable.
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(SeLanguage))]
internal partial class SeLanguageJsonContext : JsonSerializerContext
{
}
