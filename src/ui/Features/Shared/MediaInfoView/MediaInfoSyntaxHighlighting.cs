using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Nikse.SubtitleEdit.Logic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Shared.MediaInfoView;

/// <summary>
/// Syntax highlighting for media file information output
/// </summary>
public partial class MediaInfoSyntaxHighlighting : DocumentColorizingTransformer
{
    // Dark theme palette (VS Code dark)
    private static readonly IBrush HeaderBrushDark = new SolidColorBrush(Color.Parse("#569CD6"));
    private static readonly IBrush ValueBrushDark = new SolidColorBrush(Color.Parse("#CE9178"));
    private static readonly IBrush TrackNumberBrushDark = new SolidColorBrush(Color.Parse("#B5CEA8"));
    private static readonly IBrush TrackTypeBrushDark = new SolidColorBrush(Color.Parse("#4EC9B0"));
    private static readonly IBrush CodecBrushDark = new SolidColorBrush(Color.Parse("#DCDCAA"));
    private static readonly IBrush TechnicalBrushDark = new SolidColorBrush(Color.Parse("#9CDCFE"));
    private static readonly IBrush SeparatorBrushDark = new SolidColorBrush(Color.Parse("#808080"));

    // Light theme palette (darker, saturated colors for contrast on white)
    private static readonly IBrush HeaderBrushLight = new SolidColorBrush(Color.Parse("#0B5394"));
    private static readonly IBrush ValueBrushLight = new SolidColorBrush(Color.Parse("#A33800"));
    private static readonly IBrush TrackNumberBrushLight = new SolidColorBrush(Color.Parse("#2E7D32"));
    private static readonly IBrush TrackTypeBrushLight = new SolidColorBrush(Color.Parse("#00695C"));
    private static readonly IBrush CodecBrushLight = new SolidColorBrush(Color.Parse("#7A5C00"));
    private static readonly IBrush TechnicalBrushLight = new SolidColorBrush(Color.Parse("#1565C0"));
    private static readonly IBrush SeparatorBrushLight = new SolidColorBrush(Color.Parse("#555555"));

    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

    private static IBrush HeaderBrush => UiTheme.IsDarkThemeEnabled() ? HeaderBrushDark : HeaderBrushLight;
    private static IBrush ValueBrush => UiTheme.IsDarkThemeEnabled() ? ValueBrushDark : ValueBrushLight;
    private static IBrush TrackNumberBrush => UiTheme.IsDarkThemeEnabled() ? TrackNumberBrushDark : TrackNumberBrushLight;
    private static IBrush TrackTypeBrush => UiTheme.IsDarkThemeEnabled() ? TrackTypeBrushDark : TrackTypeBrushLight;
    private static IBrush CodecBrush => UiTheme.IsDarkThemeEnabled() ? CodecBrushDark : CodecBrushLight;
    private static IBrush TechnicalBrush => UiTheme.IsDarkThemeEnabled() ? TechnicalBrushDark : TechnicalBrushLight;
    private static IBrush SeparatorBrush => UiTheme.IsDarkThemeEnabled() ? SeparatorBrushDark : SeparatorBrushLight;

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

    protected override void ColorizeLine(DocumentLine line)
    {
        var lineText = CurrentContext.Document.GetText(line);
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        var valueStartIndex = 0;

        // 1. Handle Field Headers (File name, Resolution, etc.)
        var fieldHeaderMatch = FieldHeaderRegex().Match(lineText);
        if (fieldHeaderMatch.Success && fieldHeaderMatch.Index == 0)
        {
            ChangeLinePart(
                line.Offset,
                line.Offset + fieldHeaderMatch.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(HeaderBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });

            valueStartIndex = fieldHeaderMatch.Length;

            // Apply default value color to the rest of the header line
            ChangeLinePart(
                line.Offset + valueStartIndex,
                line.Offset + line.Length,
                element => element.TextRunProperties.SetForegroundBrush(ValueBrush));

            return;
        }

        // 2. Handle Track Headers (#1 - Video)
        var trackHeaderMatch = TrackHeaderRegex().Match(lineText);
        if (trackHeaderMatch.Success && trackHeaderMatch.Index == 0)
        {
            // Colorize #1
            var numberGroup = trackHeaderMatch.Groups[1];
            ChangeLinePart(line.Offset, line.Offset + numberGroup.Index + numberGroup.Length, element =>
            {
                element.TextRunProperties.SetForegroundBrush(TrackNumberBrush);
                element.TextRunProperties.SetTypeface(BoldTypeface);
            });

            // Colorize " - Video"
            var typeGroup = trackHeaderMatch.Groups[2];
            ChangeLinePart(line.Offset + trackHeaderMatch.Groups[0].Index + numberGroup.Length + 1, line.Offset + typeGroup.Index + typeGroup.Length, element =>
            {
                element.TextRunProperties.SetForegroundBrush(TrackTypeBrush);
                element.TextRunProperties.SetTypeface(BoldTypeface);
            });

            valueStartIndex = trackHeaderMatch.Length;
        }

        // 3. Always process the "Value" part of the line for technical details
        // This allows 640x346 to be colored even if it's on a "Resolution:" line
        ColorizeTrackDetails(line, lineText, valueStartIndex);
        ColorizeHighPriorityTerms(line, lineText, valueStartIndex);
    }

    private void ColorizeTrackDetails(DocumentLine line, string lineText, int startOffset)
    {
        var lineStartOffset = line.Offset + startOffset;
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
                ChangeLinePart(
                    lineStartOffset + firstWordStart,
                    lineStartOffset + firstWordEnd,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(CodecBrush);
                        element.TextRunProperties.SetTypeface(BoldTypeface);
                    });
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
                    ChangeLinePart(
                        lineStartOffset + i,
                        lineStartOffset + endBracket + 1,
                        element =>
                        {
                            element.TextRunProperties.SetForegroundBrush(TechnicalBrush);
                        });
                    i = endBracket;
                }
            }
        }

        // Colorize parentheses content (codec details)
        foreach (Match match in ParenthesesRegex().Matches(remainingText))
        {
            ChangeLinePart(
                lineStartOffset + match.Index,
                lineStartOffset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(TechnicalBrush);
                });
        }

        // Colorize commas as separators
        for (var i = 0; i < remainingText.Length; i++)
        {
            if (remainingText[i] == ',')
            {
                ChangeLinePart(
                    lineStartOffset + i,
                    lineStartOffset + i + 1,
                    element =>
                    {
                        element.TextRunProperties.SetForegroundBrush(SeparatorBrush);
                    });
            }
        }
    }

    private void ColorizeHighPriorityTerms(DocumentLine line, string lineText, int startOffset)
    {
        var lineStartOffset = line.Offset + startOffset;
        var remainingText = startOffset < lineText.Length ? lineText.Substring(startOffset) : string.Empty;

        if (string.IsNullOrEmpty(remainingText))
        {
            return;
        }

        // We apply NumberRegex first so that Specific items can override them
        foreach (Match match in NumberRegex().Matches(remainingText))
        {
            ChangeLinePart(
                lineStartOffset + match.Index,
                lineStartOffset + match.Index + match.Length,
                element => element.TextRunProperties.SetForegroundBrush(ValueBrush));
        }

        // Now override numbers with Resolutions (e.g., 1920x1080)
        foreach (Match match in ResolutionRegex().Matches(remainingText))
        {
            ChangeLinePart(
                lineStartOffset + match.Index,
                lineStartOffset + match.Index + match.Length,
                element =>
                {
                    element.TextRunProperties.SetForegroundBrush(ValueBrush);
                    element.TextRunProperties.SetTypeface(BoldTypeface);
                });
        }

        // Finally, apply Technical Terms
        foreach (Match match in TechnicalTermRegex().Matches(remainingText))
        {
            ChangeLinePart(
                lineStartOffset + match.Index,
                lineStartOffset + match.Index + match.Length,
                element => element.TextRunProperties.SetForegroundBrush(TechnicalBrush));
        }
    }
}