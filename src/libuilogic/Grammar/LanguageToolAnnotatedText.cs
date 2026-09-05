using Nikse.SubtitleEdit.Core.Common;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Grammar;

/// <summary>
/// Builds the annotated document that LanguageTool's /v2/check "data" parameter takes, and maps the
/// offsets it answers with back onto the subtitle lines that went in.
///
/// Formatting tags, music symbols and the line break inside a subtitle are sent as "markup" so
/// LanguageTool neither spell checks them nor reads them as words, while the offsets it returns stay
/// relative to the complete text - markup included - so a replacement can be applied straight to the
/// original line instead of rebuilding it from a cleaned copy.
///
/// Consecutive lines go into one document, joined with a space where the sentence continues into the
/// next line and with a blank line where it does not, so grammar is checked the way the text reads on
/// screen rather than one subtitle at a time.
/// </summary>
public class LanguageToolAnnotatedText
{
    // Same shapes the AI review protocol treats as tags, plus the line break and music symbols.
    private static readonly Regex MarkupRegex = new(@"<[^>]*>|\{\\[^}]*\}|\r\n|\n|[♪♫♬♩]", RegexOptions.Compiled);

    private readonly List<TextSpan> _lineSpans;
    private readonly List<TextSpan> _markupSpans;

    /// <summary>The value for the "data" parameter.</summary>
    public string Json { get; }

    /// <summary>The complete text - markup included - that the returned offsets refer to.</summary>
    public string Text { get; }

    public bool IsEmpty => Text.Trim().Length == 0;

    private LanguageToolAnnotatedText(string json, string text, List<TextSpan> lineSpans, List<TextSpan> markupSpans)
    {
        Json = json;
        Text = text;
        _lineSpans = lineSpans;
        _markupSpans = markupSpans;
    }

    public static LanguageToolAnnotatedText Build(IReadOnlyList<string> lines)
    {
        var text = new StringBuilder();
        var lineSpans = new List<TextSpan>(lines.Count);
        var markupSpans = new List<TextSpan>();

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteStartArray("annotation");

            for (var i = 0; i < lines.Count; i++)
            {
                if (i > 0)
                {
                    // A separator of our own, so it is markup: interpreted as a space while the
                    // sentence runs on, as a paragraph break once it has ended.
                    const string separator = "\n";
                    WriteMarkup(writer, separator, EndsSentence(lines[i - 1]) ? "\n\n" : " ");
                    markupSpans.Add(new TextSpan(text.Length, separator.Length));
                    text.Append(separator);
                }

                var start = text.Length;
                AppendLine(writer, text, markupSpans, lines[i] ?? string.Empty);
                lineSpans.Add(new TextSpan(start, text.Length - start));
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return new LanguageToolAnnotatedText(Encoding.UTF8.GetString(stream.ToArray()), text.ToString(), lineSpans, markupSpans);
    }

    /// <summary>
    /// Maps a match onto the line it belongs to. False when the match spans more than one line or
    /// touches markup - those cannot be applied without mangling a tag or a line break.
    /// </summary>
    public bool TryMapToLine(int offset, int length, out int lineIndex, out int lineOffset)
    {
        lineIndex = -1;
        lineOffset = -1;
        if (length <= 0 || offset < 0 || offset + length > Text.Length)
        {
            return false;
        }

        for (var i = 0; i < _lineSpans.Count; i++)
        {
            var span = _lineSpans[i];
            if (offset < span.Start || offset + length > span.Start + span.Length)
            {
                continue;
            }

            if (OverlapsMarkup(offset, length))
            {
                return false;
            }

            lineIndex = i;
            lineOffset = offset - span.Start;
            return true;
        }

        return false;
    }

    private bool OverlapsMarkup(int offset, int length)
    {
        foreach (var span in _markupSpans)
        {
            if (offset < span.Start + span.Length && span.Start < offset + length)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>True when the line finishes a sentence, i.e. the next line starts a new one.</summary>
    public static bool EndsSentence(string text)
    {
        var s = HtmlUtil.RemoveHtmlTags(text ?? string.Empty, true).TrimEnd();
        if (s.Length == 0)
        {
            return true;
        }

        // skip closing quotes/brackets after the sentence-final mark
        var i = s.Length - 1;
        while (i >= 0 && "\"'’”»)]".IndexOf(s[i]) >= 0)
        {
            i--;
        }

        if (i < 0)
        {
            return true;
        }

        return ".!?…".IndexOf(s[i]) >= 0;
    }

    private static void AppendLine(Utf8JsonWriter writer, StringBuilder text, List<TextSpan> markupSpans, string line)
    {
        var index = 0;
        foreach (Match match in MarkupRegex.Matches(line))
        {
            if (match.Index > index)
            {
                var plain = line.Substring(index, match.Index - index);
                WriteText(writer, plain);
                text.Append(plain);
            }

            // A line break inside a subtitle is a space to the reader, tags and music symbols are nothing.
            var isNewLine = match.Value == "\n" || match.Value == "\r\n";
            WriteMarkup(writer, match.Value, isNewLine ? " " : null);
            markupSpans.Add(new TextSpan(text.Length, match.Length));
            text.Append(match.Value);
            index = match.Index + match.Length;
        }

        if (index < line.Length)
        {
            var rest = line.Substring(index);
            WriteText(writer, rest);
            text.Append(rest);
        }
    }

    private static void WriteText(Utf8JsonWriter writer, string value)
    {
        writer.WriteStartObject();
        writer.WriteString("text", value);
        writer.WriteEndObject();
    }

    private static void WriteMarkup(Utf8JsonWriter writer, string value, string? interpretAs)
    {
        writer.WriteStartObject();
        writer.WriteString("markup", value);
        if (interpretAs != null)
        {
            writer.WriteString("interpretAs", interpretAs);
        }

        writer.WriteEndObject();
    }

    private readonly struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }
        public int Length { get; }
    }
}
