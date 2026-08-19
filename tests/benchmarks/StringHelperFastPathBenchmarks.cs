using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Before/after for the fast paths in this branch. "Before" is a copy of the shape that was
/// replaced, so both halves run in one process against one build - no stash baseline, and no
/// chance of the two halves measuring the same code.
/// </summary>
[MemoryDiagnoser]
public class StringHelperFastPathBenchmarks
{
    private const string TextLine = "He never came back that night, and nobody ever asked why.";
    private const string TimeCodeLine = "00:00:12,340 --> 00:00:15,900";
    private const string ColorLine = "<font color=\"#ff0000\">He never came back that night.</font>";
    private const string AssaPosLine = "{\\pos(320,240)\\fad(200,200)}He never came back that night.";
    private const string AssaAlignmentLine = "{\\an8}He never came back that night.";
    private const string JsonBlob =
        "{\"events\":[{\"tStartMs\":12340,\"dDurationMs\":3560,\"segs\":[{\"utf8\":\"He never came back\"}]}]}";

    private static readonly char[] TimeCodeChars =
        { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ':', ',' };

    private static readonly CharLookup TimeCodeLookup = CharLookup.Create(TimeCodeChars);

    // ---------------------------------------------------------- "is this only a time code?"

    [Benchmark]
    public bool IsOnlyTimeCode_Before_TextLine() => string.IsNullOrWhiteSpace(TextLine.RemoveChar(TimeCodeChars));

    [Benchmark]
    public bool IsOnlyTimeCode_After_TextLine() => TextLine.IsOnlyCharsOrWhiteSpace(TimeCodeLookup);

    [Benchmark]
    public bool IsOnlyTimeCode_Before_TimeCode() => string.IsNullOrWhiteSpace(TimeCodeLine.RemoveChar(TimeCodeChars));

    [Benchmark]
    public bool IsOnlyTimeCode_After_TimeCode() => TimeCodeLine.IsOnlyCharsOrWhiteSpace(TimeCodeLookup);

    // ---------------------------------------------------------- HtmlUtil.RemoveColorTags

    private static readonly Regex ColorAttributeRegex =
        new Regex("[ ]*(COLOR|color|Color)=[\"']*[#\\dA-Za-z]*[\"']*[ ]*", RegexOptions.Compiled);

    private static string RemoveColorTagsBefore(string input)
    {
        var r = ColorAttributeRegex;
        var s = input;
        var match = r.Match(s);
        while (match.Success)
        {
            s = s.Remove(match.Index, match.Value.Length).Insert(match.Index, " ");
            if (match.Index > 4)
            {
                if (string.Compare(s, match.Index - 5, "<font >", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    s = s.Remove(match.Index - 5, 7);
                    var endIndex = s.IndexOf("</font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                    if (endIndex >= 0)
                    {
                        s = s.Remove(endIndex, 7);
                    }
                }
                else if (s.Length > match.Index + 1 && s[match.Index + 1] == '>')
                {
                    s = s.Remove(match.Index, 1);
                }
            }

            match = r.Match(s);
        }

        return s.Trim();
    }

    [Benchmark]
    public string RemoveColorTags_Before_TextLine() => RemoveColorTagsBefore(TextLine);

    [Benchmark]
    public string RemoveColorTags_After_TextLine() => HtmlUtil.RemoveColorTags(TextLine);

    [Benchmark]
    public string RemoveColorTags_Before_Tagged() => RemoveColorTagsBefore(ColorLine);

    [Benchmark]
    public string RemoveColorTags_After_Tagged() => HtmlUtil.RemoveColorTags(ColorLine);

    // ---------------------------------------------------------- HtmlUtil.RemoveAssAlignmentTags

    // The literal Replace chain exactly as it was - no needle is built per call, so the
    // cost measured here is only the 45 scans.
    private static string RemoveAlignmentBefore(string s)
    {
        if (s.IndexOf('\\') < 0)
        {
            return s;
        }

        return s.Replace("{\\an1}", string.Empty)
            .Replace("{\\an2}", string.Empty)
            .Replace("{\\an3}", string.Empty)
            .Replace("{\\an4}", string.Empty)
            .Replace("{\\an5}", string.Empty)
            .Replace("{\\an6}", string.Empty)
            .Replace("{\\an7}", string.Empty)
            .Replace("{\\an8}", string.Empty)
            .Replace("{\\an9}", string.Empty)
            .Replace("{an1\\", "{")
            .Replace("{an2\\", "{")
            .Replace("{an3\\", "{")
            .Replace("{an4\\", "{")
            .Replace("{an5\\", "{")
            .Replace("{an6\\", "{")
            .Replace("{an7\\", "{")
            .Replace("{an8\\", "{")
            .Replace("{an9\\", "{")
            .Replace("\\an1\\", "\\\\")
            .Replace("\\an2\\", "\\\\")
            .Replace("\\an3\\", "\\\\")
            .Replace("\\an4\\", "\\\\")
            .Replace("\\an5\\", "\\\\")
            .Replace("\\an6\\", "\\\\")
            .Replace("\\an7\\", "\\\\")
            .Replace("\\an8\\", "\\\\")
            .Replace("\\an9\\", "\\\\")
            .Replace("\\an1}", "}")
            .Replace("\\an2}", "}")
            .Replace("\\an3}", "}")
            .Replace("\\an4}", "}")
            .Replace("\\an5}", "}")
            .Replace("\\an6}", "}")
            .Replace("\\an7}", "}")
            .Replace("\\an8}", "}")
            .Replace("\\an9}", "}")
            .Replace("{\\a1}", string.Empty)
            .Replace("{\\a2}", string.Empty)
            .Replace("{\\a3}", string.Empty)
            .Replace("{\\a4}", string.Empty)
            .Replace("{\\a5}", string.Empty)
            .Replace("{\\a6}", string.Empty)
            .Replace("{\\a7}", string.Empty)
            .Replace("{\\a8}", string.Empty)
            .Replace("{\\a9}", string.Empty);
    }

    [Benchmark]
    public string RemoveAlignment_Before_PosOnly() => RemoveAlignmentBefore(AssaPosLine);

    [Benchmark]
    public string RemoveAlignment_After_PosOnly() => HtmlUtil.RemoveAssAlignmentTags(AssaPosLine);

    [Benchmark]
    public string RemoveAlignment_Before_Aligned() => RemoveAlignmentBefore(AssaAlignmentLine);

    [Benchmark]
    public string RemoveAlignment_After_Aligned() => HtmlUtil.RemoveAssAlignmentTags(AssaAlignmentLine);

    // ---------------------------------------------------------- Utilities.IsNumber

    private static readonly Regex RegexIsNumber = new Regex("^\\d+$", RegexOptions.Compiled);
    private static readonly Regex RegexIsEpisodeNumber = new Regex("^\\d+x\\d+$", RegexOptions.Compiled);

    private static bool IsNumberBefore(string s)
    {
        s = s.Trim('$', '\u00A3', '\u00A5', '%', '*');
        return RegexIsNumber.IsMatch(s) || RegexIsEpisodeNumber.IsMatch(s);
    }

    [Benchmark]
    public bool IsNumber_Before_TextLine() => IsNumberBefore(TextLine);

    [Benchmark]
    public bool IsNumber_After_TextLine() => Utilities.IsNumber(TextLine);

    [Benchmark]
    public bool IsNumber_Before_Number() => IsNumberBefore("1234");

    [Benchmark]
    public bool IsNumber_After_Number() => Utilities.IsNumber("1234");

    // ---------------------------------------------------------- Json.ReadTag

    private static int IndexOfAnyBefore(string s, string[] words, StringComparison comparisonType)
    {
        for (var i = 0; i < words.Length; i++)
        {
            var idx = s.IndexOf(words[i], comparisonType);
            if (idx >= 0)
            {
                return idx;
            }
        }

        return -1;
    }

    // Not a const: the real caller takes the tag as a parameter, and a const here would let
    // the compiler fold the two quoted needles into literals - erasing what is being measured.
    private static readonly string TagName = "tStartMs";

    [Benchmark]
    public int JsonTagLookup_Before()
    {
        var tag = TagName;
        return IndexOfAnyBefore(JsonBlob, new[] { "\"" + tag + "\"", "'" + tag + "'" }, StringComparison.Ordinal);
    }

    // Same code as the private helper the format now uses - both halves measure only the tag
    // lookup, which is the part that changed.
    private static int IndexOfQuotedTag(string s, string tag)
    {
        var index = IndexOfQuoted(s, tag, '"');
        return index >= 0 ? index : IndexOfQuoted(s, tag, '\'');
    }

    private static int IndexOfQuoted(string s, string tag, char quote)
    {
        var from = 0;
        while (from < s.Length)
        {
            var index = s.IndexOf(tag, from, StringComparison.Ordinal);
            if (index < 0 || index + tag.Length >= s.Length)
            {
                return -1;
            }

            if (index > 0 && s[index - 1] == quote && s[index + tag.Length] == quote)
            {
                return index - 1;
            }

            from = index + 1;
        }

        return -1;
    }

    [Benchmark]
    public int JsonTagLookup_After() => IndexOfQuotedTag(JsonBlob, TagName);
}
