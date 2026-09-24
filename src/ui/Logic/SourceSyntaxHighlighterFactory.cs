using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Picks the syntax rules for a subtitle format - shared by the source view and the format preview.
/// </summary>
public static class SourceSyntaxHighlighterFactory
{
    public static ISourceSyntaxHighlighter? ForFormat(string text, SubtitleFormat subtitleFormat)
    {
        if (subtitleFormat is SubRip)
        {
            return new SubRipSourceSyntaxHighlighting();
        }

        if (subtitleFormat is WebVTT || subtitleFormat is WebVTTFileWithLineNumber)
        {
            return new WebVttSourceSyntaxHighlighting();
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

        if (subtitleFormat is Lrc || subtitleFormat is Lrc3DigitsMs || subtitleFormat is LrcNoEndTime)
        {
            return new LrcSourceSyntaxHighlighting();
        }

        // Drop frame derives from ScenaristClosedCaptions
        if (subtitleFormat is ScenaristClosedCaptions)
        {
            return new SccSourceSyntaxHighlighting();
        }

        // Every other text format: time codes and markup, which nearly all of them share
        return new GenericSourceSyntaxHighlighting();
    }
}
