using Avalonia.Media;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for LRC lyrics (.lrc): the [ar:]/[ti:]/[offset:] header tags, the
/// "[mm:ss.xx]" line time stamps - several may share one line - and the "&lt;mm:ss.xx&gt;" word
/// time stamps of enhanced LRC. Colors are the SubRip time set and the ASSA palette, so both
/// carry a light-mode variant.
/// </summary>
public partial class LrcSourceSyntaxHighlighting : ISourceSyntaxHighlighter
{
    private static Color TimeColor => SubRipSourceSyntaxHighlighting.TimeColor;
    private static Color TagNameColor => AssaSourceSyntaxHighlighting.PropertyColor;
    private static Color TagValueColor => AssaSourceSyntaxHighlighting.ValueColor;
    private static Color CommentColor => AssaSourceSyntaxHighlighting.CommentColor;
    private static Color CharsColor => SubtitleSyntaxTokenizer.CharsColor;

    // "[00:12.34]", "[00:12.345]" (Lrc3DigitsMs), "[00:12]" and "[00:12:34]" as some players write.
    private const string TimePattern = @"\d+:\d{2}(?:[.:]\d{2,3})?";

    // A header tag fills the whole line: "[ar:Artist]", "[offset:+500]".
    [GeneratedRegex(@"^(\s*)\[([A-Za-z#]+)(:)(.*)\]\s*$")]
    private static partial Regex HeaderTagRegex();

    // The line time stamps, stacked at the start of the line: "[00:12.00][01:30.00]Chorus".
    [GeneratedRegex(@"\G\s*\[(" + TimePattern + @")\]")]
    private static partial Regex LineTimeRegex();

    // Enhanced LRC word time stamps: "<00:12.50>Word".
    [GeneratedRegex(@"<(" + TimePattern + ")>")]
    private static partial Regex WordTimeRegex();

    public void HighlightLine(string lineText, SourceSyntaxLineStyler styler)
    {
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        var position = 0;
        var hasLineTime = false;
        for (var match = LineTimeRegex().Match(lineText); match.Success; match = LineTimeRegex().Match(lineText, position))
        {
            ColorBracketed(match, styler, bold: true);
            position = match.Index + match.Length;
            hasLineTime = true;
        }

        if (!hasLineTime)
        {
            var header = HeaderTagRegex().Match(lineText);
            if (header.Success)
            {
                var open = header.Groups[1].Length;
                styler.Apply(open, 1, CharsColor);
                styler.Apply(header.Groups[2].Index, header.Groups[2].Length, TagNameColor, bold: true);
                styler.Apply(header.Groups[3].Index, 1, CharsColor);
                styler.Apply(header.Groups[4].Index, header.Groups[4].Length, TagValueColor);
                styler.Apply(header.Groups[4].Index + header.Groups[4].Length, 1, CharsColor);
                return;
            }

            // Some files carry "#" comment lines, which players skip.
            if (lineText.TrimStart().StartsWith('#'))
            {
                styler.Apply(0, lineText.Length, CommentColor);
                return;
            }
        }

        foreach (Match wordTime in WordTimeRegex().Matches(lineText, position))
        {
            ColorBracketed(wordTime, styler, bold: false);
        }
    }

    /// <summary>Brackets in the punctuation color, the time inside in the time color.</summary>
    private static void ColorBracketed(Match match, SourceSyntaxLineStyler styler, bool bold)
    {
        var time = match.Groups[1];
        styler.Apply(time.Index - 1, 1, CharsColor);
        styler.Apply(time.Index, time.Length, TimeColor, bold: bold, defaultFont: true);
        styler.Apply(time.Index + time.Length, 1, CharsColor);
    }
}
