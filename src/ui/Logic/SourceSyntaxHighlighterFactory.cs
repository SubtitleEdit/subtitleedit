using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Picks the syntax rules for a subtitle format - shared by the source view and the format preview.
/// </summary>
public static class SourceSyntaxHighlighterFactory
{
    public static ISourceSyntaxHighlighter? ForFormat(string text, SubtitleFormat subtitleFormat)
    {
        // SubRip (.srt) and WebVTT (.vtt) use similar time code formats
        if (subtitleFormat is SubRip ||
            subtitleFormat is WebVTT ||
            subtitleFormat is WebVTTFileWithLineNumber)
        {
            return new SubRipSourceSyntaxHighlighting();
        }

        // Advanced SubStation Alpha (.ass) and SubStation Alpha (.ssa) formats
        if (subtitleFormat is AdvancedSubStationAlpha || subtitleFormat is SubStationAlpha)
        {
            return new AssaSourceSyntaxHighlighting();
        }

        // XML-based formats (e.g., TTML, Netflix DFXP, etc.)
        if (subtitleFormat.Extension == ".xml" ||
            subtitleFormat.AlternateExtensions.Contains(".xml") ||
            text.Contains("<?xml version=") ||
            subtitleFormat is Sami ||
            subtitleFormat is SamiModern ||
            subtitleFormat is SamiYouTube ||
            subtitleFormat is SamiAvDicPlayer)
        {
            return new XmlSourceSyntaxHighlighting();
        }

        // Json-based formats
        if (subtitleFormat.Extension == ".json" ||
            subtitleFormat.AlternateExtensions.Contains(".json"))
        {
            return new JsonSourceSyntaxHighlighting();
        }

        // No syntax highlighting for other formats
        return null;
    }
}
