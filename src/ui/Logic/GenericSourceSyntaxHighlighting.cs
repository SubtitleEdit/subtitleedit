using Avalonia.Media;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Basic coloring for the text formats without rules of their own - MicroDVD, LRC, SubViewer, SCC,
/// the CSV family and the hundreds of "unknown" formats. It knows no format: it picks out what
/// nearly all of them have in common, time codes and markup, and leaves everything else as text.
/// </summary>
public partial class GenericSourceSyntaxHighlighting : ISourceSyntaxHighlighter
{
    private static Color TimeColor => SubRipSourceSyntaxHighlighting.TimeColor;
    private static Color TimeSeparatorColor => SubRipSourceSyntaxHighlighting.TimeSeparatorColor;
    private static Color FrameColor => SubRipSourceSyntaxHighlighting.NumberColor;
    private static Color ElementColor => SubtitleSyntaxTokenizer.ElementColor;
    private static Color CharsColor => SubtitleSyntaxTokenizer.CharsColor;
    private static Color ValuesColor => SubtitleSyntaxTokenizer.ValuesColor;

    // A time code needs hours or a fraction, so "at 10:30" in the text is left alone:
    // "0:00:01.50", "00:00:01:12" (frames), "00:00:01;12" (drop frame), "01:02.500", "[00:12.34]".
    [GeneratedRegex(@"(?<![\d:])(?:\d{1,2}:\d{2}:\d{2}(?:[.,:;]\d{1,3})?|\d{1,3}:\d{2}[.,]\d{2,3})(?![\d:])")]
    private static partial Regex TimeCodeRegex();

    [GeneratedRegex(@"-->|->")]
    private static partial Regex ArrowRegex();

    // Frame numbers: MicroDVD "{25}{75}", MPL2 "[10][30]".
    [GeneratedRegex(@"\{\d+\}|\[\d+\]")]
    private static partial Regex FrameRegex();

    // MicroDVD control codes: "{y:i}", "{c:$0000FF}".
    [GeneratedRegex(@"\{([A-Za-z]):([^}]*)\}")]
    private static partial Regex ControlCodeRegex();

    // A real tag somewhere on the line: "<i>", "</font>", "{\an8}". The tag colorizer reads every
    // word after a bare "<" as an attribute, so "if a < b then" must not reach it.
    [GeneratedRegex(@"</?[A-Za-z][^<>]*>|\{\\")]
    private static partial Regex TagRegex();

    public void HighlightLine(string lineText, SourceSyntaxLineStyler styler)
    {
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        // Markup first: a time code or frame number inside a tag still reads as one.
        if (TagRegex().IsMatch(lineText))
        {
            SubRipSourceSyntaxHighlighting.ColorizeHtmlAndAssTags(lineText, styler);
        }

        foreach (Match code in ControlCodeRegex().Matches(lineText))
        {
            styler.Apply(code.Index, 1, CharsColor);
            styler.Apply(code.Groups[1].Index, 1, ElementColor);
            styler.Apply(code.Groups[1].Index + 1, 1, CharsColor);
            styler.Apply(code.Groups[2].Index, code.Groups[2].Length, ValuesColor);
            styler.Apply(code.Index + code.Length - 1, 1, CharsColor);
        }

        foreach (Match frame in FrameRegex().Matches(lineText))
        {
            styler.Apply(frame.Index, frame.Length, FrameColor, bold: true, defaultFont: true);
        }

        var hasTimeCode = false;
        foreach (Match time in TimeCodeRegex().Matches(lineText))
        {
            styler.Apply(time.Index, time.Length, TimeColor, bold: true, defaultFont: true);
            hasTimeCode = true;
        }

        // An arrow only means "until" between time codes; in text it is just an arrow.
        if (hasTimeCode)
        {
            foreach (Match arrow in ArrowRegex().Matches(lineText))
            {
                styler.Apply(arrow.Index, arrow.Length, TimeSeparatorColor, bold: true, defaultFont: true);
            }
        }
    }
}
