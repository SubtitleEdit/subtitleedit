using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Shared.MediaInfoView;

/// <summary>
/// Syntax highlighting for media file information output
/// </summary>
public partial class MediaInfoSyntaxHighlighting : DocumentColorizingTransformer
{
    // Color scheme for media info
    private static readonly IBrush HeaderBrush = new SolidColorBrush(Color.Parse("#569CD6")); // Blue for headers (File name, Duration, etc.)
    private static readonly IBrush ValueBrush = new SolidColorBrush(Color.Parse("#CE9178")); // Orange for values
    private static readonly IBrush TrackNumberBrush = new SolidColorBrush(Color.Parse("#B5CEA8")); // Light green for track numbers
    private static readonly IBrush TrackTypeBrush = new SolidColorBrush(Color.Parse("#4EC9B0")); // Cyan for track types (Video, Audio, Other)
    private static readonly IBrush CodecBrush = new SolidColorBrush(Color.Parse("#DCDCAA")); // Yellow for codec names
    private static readonly IBrush TechnicalBrush = new SolidColorBrush(Color.Parse("#9CDCFE")); // Light blue for technical specs
    private static readonly IBrush SeparatorBrush = new SolidColorBrush(Color.Parse("#808080")); // Gray for separators and delimiters
    private static readonly Typeface BoldTypeface = new(FontFamily.Default, weight: FontWeight.Bold);

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