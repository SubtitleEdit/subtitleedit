using Avalonia.Media;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for WebVTT (.vtt). The time codes and cue tags match SubRip's colors, and
/// the blocks SubRip does not have (WEBVTT, NOTE, STYLE, REGION, cue settings, CSS) borrow the
/// ASSA palette, so both carry a light-mode set.
/// </summary>
/// <remarks>
/// Highlighting is per line with only the line above as context, so a block is recognized by its
/// first line: a NOTE is colored on its NOTE line, and CSS declarations inside STYLE are recognized
/// by what they look like and by following a "{" or ";" line.
/// </remarks>
public partial class WebVttSourceSyntaxHighlighting : ISourceSyntaxPreviousLineHighlighter
{
    private static Color HeaderColor => AssaSourceSyntaxHighlighting.SectionColor;
    private static Color KeywordColor => AssaSourceSyntaxHighlighting.KeywordColor;
    private static Color NoteColor => AssaSourceSyntaxHighlighting.CommentColor;
    private static Color PropertyColor => AssaSourceSyntaxHighlighting.PropertyColor;
    private static Color ValueColor => AssaSourceSyntaxHighlighting.ValueColor;
    private static Color NumberColor => SubRipSourceSyntaxHighlighting.NumberColor;
    private static Color TimeColor => SubRipSourceSyntaxHighlighting.TimeColor;
    private static Color TimeSeparatorColor => SubRipSourceSyntaxHighlighting.TimeSeparatorColor;
    private static Color CharsColor => SubtitleSyntaxTokenizer.CharsColor;
    private static Color ElementColor => SubtitleSyntaxTokenizer.ElementColor;
    private static Color ClassColor => SubtitleSyntaxTokenizer.ValuesColor;

    // WebVTT time codes may leave out the hours: "01:02.500" as well as "00:01:02.500".
    private const string TimePattern = @"(?:\d+:)?\d{2}:\d{2}\.\d{3}";

    [GeneratedRegex(@"^(" + TimePattern + @")([ \t]+-->[ \t]+)(" + TimePattern + @")(.*)$")]
    private static partial Regex TimingLineRegex();

    // Cue settings after the end time: "align:start position:10%,line-left line:0".
    [GeneratedRegex(@"([A-Za-z-]+)(:)(\S+)")]
    private static partial Regex CueSettingRegex();

    // Karaoke time stamps inside cue text: "<00:00:01.500>".
    [GeneratedRegex(@"<" + TimePattern + ">")]
    private static partial Regex CueTimestampRegex();

    // The classes of a cue tag: ".yellow.bg_blue" in "<c.yellow.bg_blue>".
    [GeneratedRegex(@"</?[A-Za-z]+((?:\.[\w-]+)+)")]
    private static partial Regex TagClassesRegex();

    [GeneratedRegex(@"&(?:[A-Za-z]+|#\d+|#x[0-9A-Fa-f]+);")]
    private static partial Regex EntityRegex();

    [GeneratedRegex(@"^\s*([A-Za-z-]+)(\s*:\s*)([^;]*)(;?)\s*$")]
    private static partial Regex CssDeclarationRegex();

    /// <summary>Without the line above, the line is treated as the first of a block.</summary>
    public void HighlightLine(string lineText, SourceSyntaxLineStyler styler) => HighlightLine(lineText, string.Empty, styler);

    public void HighlightLine(string lineText, string? previousLine, SourceSyntaxLineStyler styler)
    {
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        var startsBlock = string.IsNullOrWhiteSpace(previousLine);

        if (lineText.StartsWith("WEBVTT", System.StringComparison.Ordinal) &&
            (lineText.Length == 6 || lineText[6] == ' ' || lineText[6] == '\t'))
        {
            styler.Apply(0, 6, HeaderColor, bold: true);
            return;
        }

        if (startsBlock && IsBlockKeyword(lineText, "NOTE"))
        {
            styler.Apply(0, lineText.Length, NoteColor);
            return;
        }

        if (startsBlock && (lineText.TrimEnd() == "STYLE" || lineText.TrimEnd() == "REGION"))
        {
            styler.Apply(0, lineText.Length, KeywordColor, bold: true);
            return;
        }

        if (ColorizeTimingLine(lineText, styler))
        {
            return;
        }

        if (ColorizeCss(lineText, previousLine, styler))
        {
            return;
        }

        // A numeric cue identifier opens a block like an .srt cue number does; under a time code
        // the same digits are cue text.
        if (startsBlock && IsDigits(lineText.Trim()))
        {
            styler.Apply(0, lineText.Length, NumberColor, bold: true, defaultFont: true);
            return;
        }

        ColorizeCueText(lineText, styler);
    }

    private static bool IsBlockKeyword(string lineText, string keyword) =>
        lineText.StartsWith(keyword, System.StringComparison.Ordinal) &&
        (lineText.Length == keyword.Length || lineText[keyword.Length] == ' ' || lineText[keyword.Length] == '\t');

    private static bool IsDigits(string s)
    {
        if (s.Length == 0)
        {
            return false;
        }

        foreach (var c in s)
        {
            if (!char.IsAsciiDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ColorizeTimingLine(string lineText, SourceSyntaxLineStyler styler)
    {
        var match = TimingLineRegex().Match(lineText);
        if (!match.Success)
        {
            return false;
        }

        var start = match.Groups[1];
        var arrow = match.Groups[2];
        var end = match.Groups[3];
        styler.Apply(start.Index, start.Length, TimeColor, bold: true, defaultFont: true);
        styler.Apply(arrow.Index, arrow.Length, TimeSeparatorColor, bold: true, defaultFont: true);
        styler.Apply(end.Index, end.Length, TimeColor, bold: true, defaultFont: true);

        var settings = match.Groups[4];
        foreach (Match setting in CueSettingRegex().Matches(lineText, settings.Index))
        {
            styler.Apply(setting.Groups[1].Index, setting.Groups[1].Length, PropertyColor);
            styler.Apply(setting.Groups[2].Index, 1, CharsColor);
            styler.Apply(setting.Groups[3].Index, setting.Groups[3].Length, ValueColor);
        }

        return true;
    }

    /// <summary>
    /// The CSS of a STYLE block: "::cue(...) {" selectors, and the declarations and closing brace
    /// that follow one. Cue text reading like a declaration ("Time: now;") right under another
    /// such line is the one thing this could mistake.
    /// </summary>
    private static bool ColorizeCss(string lineText, string? previousLine, SourceSyntaxLineStyler styler)
    {
        var trimmed = lineText.TrimStart();
        if (trimmed.StartsWith("::cue", System.StringComparison.Ordinal))
        {
            var offset = lineText.Length - trimmed.Length;
            var selectorEnd = offset + 5;
            styler.Apply(offset, 5, ElementColor);

            var brace = lineText.IndexOf('{', selectorEnd);
            var argumentsEnd = brace >= 0 ? brace : lineText.Length;
            if (argumentsEnd > selectorEnd)
            {
                styler.Apply(selectorEnd, argumentsEnd - selectorEnd, ClassColor);
            }

            if (brace >= 0)
            {
                styler.Apply(brace, lineText.Length - brace, CharsColor);
            }

            return true;
        }

        // Inside a rule: right after the "{", or after a declaration. A cue line that merely ends
        // in ";" ("Wait;") is not a declaration, so the line under it stays cue text.
        var previous = previousLine?.TrimEnd() ?? string.Empty;
        var insideRule = previous.EndsWith('{') ||
                         previous.EndsWith(';') && CssDeclarationRegex().IsMatch(previous);
        if (!insideRule)
        {
            return false;
        }

        if (trimmed.TrimEnd() == "}")
        {
            styler.Apply(0, lineText.Length, CharsColor);
            return true;
        }

        var declaration = CssDeclarationRegex().Match(lineText);
        if (!declaration.Success)
        {
            return false;
        }

        styler.Apply(declaration.Groups[1].Index, declaration.Groups[1].Length, PropertyColor);
        styler.Apply(declaration.Groups[2].Index, declaration.Groups[2].Length, CharsColor);
        styler.Apply(declaration.Groups[3].Index, declaration.Groups[3].Length, ValueColor);
        if (declaration.Groups[4].Length > 0)
        {
            styler.Apply(declaration.Groups[4].Index, 1, CharsColor);
        }

        return true;
    }

    private static void ColorizeCueText(string lineText, SourceSyntaxLineStyler styler)
    {
        // <v Name>, <i>, <c.class> and <lang en> read like HTML tags.
        SubRipSourceSyntaxHighlighting.ColorizeHtmlAndAssTags(lineText, styler);

        foreach (Match classes in TagClassesRegex().Matches(lineText))
        {
            styler.Apply(classes.Groups[1].Index, classes.Groups[1].Length, ClassColor);
        }

        foreach (Match timestamp in CueTimestampRegex().Matches(lineText))
        {
            styler.Apply(timestamp.Index, 1, CharsColor);
            styler.Apply(timestamp.Index + 1, timestamp.Length - 2, TimeColor, defaultFont: true);
            styler.Apply(timestamp.Index + timestamp.Length - 1, 1, CharsColor);
        }

        foreach (Match entity in EntityRegex().Matches(lineText))
        {
            styler.Apply(entity.Index, entity.Length, CharsColor);
        }
    }
}
