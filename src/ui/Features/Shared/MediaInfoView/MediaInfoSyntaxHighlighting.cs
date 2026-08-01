using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Shared.MediaInfoView;

/// <summary>
/// Syntax highlighting for media file information output
/// </summary>
public partial class MediaInfoSyntaxHighlighting : ISourceSyntaxHighlighter
{
    // Dark theme palette (VS Code dark)
    private static readonly Color HeaderColorDark = Color.Parse("#569CD6");
    private static readonly Color ValueColorDark = Color.Parse("#CE9178");
    private static readonly Color TrackNumberColorDark = Color.Parse("#B5CEA8");
    private static readonly Color TrackTypeColorDark = Color.Parse("#4EC9B0");
    private static readonly Color CodecColorDark = Color.Parse("#DCDCAA");
    private static readonly Color TechnicalColorDark = Color.Parse("#9CDCFE");
    private static readonly Color SeparatorColorDark = Color.Parse("#808080");

    // Light theme palette (darker, saturated colors for contrast on white)
    private static readonly Color HeaderColorLight = Color.Parse("#0B5394");
    private static readonly Color ValueColorLight = Color.Parse("#A33800");
    private static readonly Color TrackNumberColorLight = Color.Parse("#2E7D32");
    private static readonly Color TrackTypeColorLight = Color.Parse("#00695C");
    private static readonly Color CodecColorLight = Color.Parse("#7A5C00");
    private static readonly Color TechnicalColorLight = Color.Parse("#1565C0");
    private static readonly Color SeparatorColorLight = Color.Parse("#555555");

    private static Color HeaderColor => UiTheme.IsDarkThemeEnabled() ? HeaderColorDark : HeaderColorLight;
    private static Color ValueColor => UiTheme.IsDarkThemeEnabled() ? ValueColorDark : ValueColorLight;
    private static Color TrackNumberColor => UiTheme.IsDarkThemeEnabled() ? TrackNumberColorDark : TrackNumberColorLight;
    private static Color TrackTypeColor => UiTheme.IsDarkThemeEnabled() ? TrackTypeColorDark : TrackTypeColorLight;
    private static Color CodecColor => UiTheme.IsDarkThemeEnabled() ? CodecColorDark : CodecColorLight;
    private static Color TechnicalColor => UiTheme.IsDarkThemeEnabled() ? TechnicalColorDark : TechnicalColorLight;
    private static Color SeparatorColor => UiTheme.IsDarkThemeEnabled() ? SeparatorColorDark : SeparatorColorLight;

    // Pattern for field headers (e.g., "File name:", "Duration:")
    [GeneratedRegex(@"^(File name|File size|Duration|Resolution|Framerate|Container|Tracks):", RegexOptions.Multiline)]
    private static partial Regex FieldHeaderRegex();

    // Pattern for track headers (e.g., "#1 - Video")
    [GeneratedRegex(@"^#(\d+)\s*-\s*(Video|Audio|Subtitle|Other)", RegexOptions.Multiline)]
    private static partial Regex TrackHeaderRegex();

    // Pattern for numbers (file size, duration, resolution, framerate, bitrate, etc.)
    [GeneratedRegex(@"\b\d+[.,]?\d*\b")]
    private static partial Regex NumberRegex();

    // Pattern for codec names in parentheses (e.g., "(High)", "(LC)")
    [GeneratedRegex(@"\([^)]+\)")]
    private static partial Regex ParenthesesRegex();

    [GeneratedRegex(@"(?i)\b(kb/s|mb|fps|tbr|tbn|tbc|Hz|kHz|SAR|DAR|avc1|XVID|mp4a|progressive|stereo|fltp|yuvj?420p|start|default|attached pic)\b")]
    private static partial Regex TechnicalTermRegex();

    [GeneratedRegex(@"\b\d{2,5}x\d{2,5}\b")]
    private static partial Regex ResolutionRegex();

    public void HighlightLine(string lineText, SourceSyntaxLineStyler styler)
    {
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        var valueStartIndex = 0;

        // 1. Handle Field Headers (File name, Resolution, etc.)
        var fieldHeaderMatch = FieldHeaderRegex().Match(lineText);
        if (fieldHeaderMatch.Success && fieldHeaderMatch.Index == 0)
        {
            styler.Apply(0, fieldHeaderMatch.Length, HeaderColor, bold: true);

            valueStartIndex = fieldHeaderMatch.Length;

            // Apply default value color to the rest of the header line
            styler.Apply(valueStartIndex, lineText.Length - valueStartIndex, ValueColor);

            return;
        }

        // 2. Handle Track Headers (#1 - Video)
        var trackHeaderMatch = TrackHeaderRegex().Match(lineText);
        if (trackHeaderMatch.Success && trackHeaderMatch.Index == 0)
        {
            // Colorize #1
            var numberGroup = trackHeaderMatch.Groups[1];
            styler.Apply(0, numberGroup.Index + numberGroup.Length, TrackNumberColor, bold: true);

            // Colorize " - Video"
            var typeGroup = trackHeaderMatch.Groups[2];
            var typeStart = trackHeaderMatch.Groups[0].Index + numberGroup.Length + 1;
            styler.Apply(typeStart, typeGroup.Index + typeGroup.Length - typeStart, TrackTypeColor, bold: true);

            valueStartIndex = trackHeaderMatch.Length;
        }

        // 3. Always process the "Value" part of the line for technical details
        // This allows 640x346 to be colored even if it's on a "Resolution:" line
        ColorizeTrackDetails(lineText, styler, valueStartIndex);
        ColorizeHighPriorityTerms(lineText, styler, valueStartIndex);
    }

    private static void ColorizeTrackDetails(string lineText, SourceSyntaxLineStyler styler, int startOffset)
    {
        var remainingText = startOffset < lineText.Length ? lineText.Substring(startOffset) : string.Empty;
        if (string.IsNullOrEmpty(remainingText))
        {
            return;
        }

        // First pass: colorize codec names (first word on the line, typically)
        var firstWordStart = 0;
        while (firstWordStart < remainingText.Length && char.IsWhiteSpace(remainingText[firstWordStart]))
            firstWordStart++;

        if (firstWordStart < remainingText.Length)
        {
            var firstWordEnd = firstWordStart;
            while (firstWordEnd < remainingText.Length && !char.IsWhiteSpace(remainingText[firstWordEnd]) && remainingText[firstWordEnd] != '(')
                firstWordEnd++;

            if (firstWordEnd > firstWordStart)
            {
                styler.Apply(startOffset + firstWordStart, firstWordEnd - firstWordStart, CodecColor, bold: true);
            }
        }

        // Colorize square brackets content (SAR, DAR)
        for (var i = 0; i < remainingText.Length; i++)
        {
            if (remainingText[i] == '[')
            {
                var endBracket = remainingText.IndexOf(']', i);
                if (endBracket != -1)
                {
                    styler.Apply(startOffset + i, endBracket + 1 - i, TechnicalColor);
                    i = endBracket;
                }
            }
        }

        // Colorize parentheses content (codec details)
        foreach (Match match in ParenthesesRegex().Matches(remainingText))
        {
            styler.Apply(startOffset + match.Index, match.Length, TechnicalColor);
        }

        // Colorize commas as separators
        for (var i = 0; i < remainingText.Length; i++)
        {
            if (remainingText[i] == ',')
            {
                styler.Apply(startOffset + i, 1, SeparatorColor);
            }
        }
    }

    private static void ColorizeHighPriorityTerms(string lineText, SourceSyntaxLineStyler styler, int startOffset)
    {
        var remainingText = startOffset < lineText.Length ? lineText.Substring(startOffset) : string.Empty;
        if (string.IsNullOrEmpty(remainingText))
        {
            return;
        }

        // We apply NumberRegex first so that Specific items can override them
        foreach (Match match in NumberRegex().Matches(remainingText))
        {
            styler.Apply(startOffset + match.Index, match.Length, ValueColor);
        }

        // Now override numbers with Resolutions (e.g., 1920x1080)
        foreach (Match match in ResolutionRegex().Matches(remainingText))
        {
            styler.Apply(startOffset + match.Index, match.Length, ValueColor, bold: true);
        }

        // Finally, apply Technical Terms
        foreach (Match match in TechnicalTermRegex().Matches(remainingText))
        {
            styler.Apply(startOffset + match.Index, match.Length, TechnicalColor);
        }
    }
}
