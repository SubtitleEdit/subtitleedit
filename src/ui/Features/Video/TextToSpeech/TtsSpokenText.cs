using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

internal static class TtsSpokenText
{
    internal static string Clean(string? text) => Utilities.UnbreakLine(HtmlUtil.RemoveHtmlTags(text ?? string.Empty, alsoSsaTags: true));

    internal static string? NormalizeOverride(string? spokenText) =>
        string.IsNullOrWhiteSpace(spokenText) ? null : spokenText.Trim();

    internal static string Resolve(string subtitleText, string? spokenText) =>
        Clean(NormalizeOverride(spokenText) ?? subtitleText);
}
