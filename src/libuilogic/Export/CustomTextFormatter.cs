using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public static partial class CustomTextFormatter
{
    public const string EnglishDoNotModify = "[Do not modify]";
    private static readonly Regex CurlyCodePattern = new Regex("{\\d+[,:]*[A-Z\\d-]*}", RegexOptions.Compiled);

    // GetTimeCode runs three times per paragraph; the static Regex.IsMatch/Replace overloads
    // it used went through the global regex cache (a lock + key hash) on every call.
    [GeneratedRegex("z+")]
    private static partial Regex ZRunRegexGen();
    private static readonly Regex ZRunRegex = ZRunRegexGen();

    [GeneratedRegex("s+")]
    private static partial Regex SRunRegexGen();
    private static readonly Regex SRunRegex = SRunRegexGen();

    public static string GenerateCustomText(CustomFormatTemplate customFormat, List<Paragraph> subtitles, string title, string videoFileName)
    {
        var formatNewLine = customFormat.FormatNewLine ?? Environment.NewLine;

        var sb = new StringBuilder();
        sb.Append(GetHeaderOrFooter(title, videoFileName, subtitles, customFormat.FormatHeader));
        var template = GetParagraphTemplate(customFormat.FormatParagraph);
        var isXml = customFormat.FormatHeader.Contains("<?xml version=", StringComparison.OrdinalIgnoreCase);
        for (var i = 0; i < subtitles.Count; i++)
        {
            var p = subtitles[i];
            var start = GetTimeCode(p.StartTime, customFormat.FormatTimeCode);
            var end = GetTimeCode(p.EndTime, customFormat.FormatTimeCode);

            var gap = string.Empty;
            var next = i + 1 < subtitles.Count ? subtitles[i + 1] : null;
            if (next != null)
            {
                gap = GetTimeCode(new TimeCode(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds), customFormat.FormatTimeCode);
            }

            var text = p.Text;
            if (isXml)
            {
                text = text.Replace("<", "&lt;")
                           .Replace(">", "&gt;")
                           .Replace("&", "&amp;");
            }
            text = GetText(text, formatNewLine);

            // libse Paragraph has no OriginalText; CLI doesn't carry a translation pair.
            var originalText = string.Empty;
            var paragraph = GetParagraph(template, start, end, text, originalText, i, p.Actor, p.Duration, gap, customFormat.FormatTimeCode, p, videoFileName);
            sb.Append(paragraph);
        }
        sb.Append(GetHeaderOrFooter(title, videoFileName, subtitles, customFormat.FormatFooter));
        return sb.ToString();
    }

    public static string GetHeaderOrFooter(string title, string videoFileName, List<Paragraph> subtitles, string template)
    {
        template = template.Replace("{title}", title);
        template = template.Replace("{media-file-name-full}", videoFileName);
        template = template.Replace("{media-file-name}", string.IsNullOrEmpty(videoFileName) ? videoFileName : System.IO.Path.GetFileNameWithoutExtension(videoFileName));
        template = template.Replace("{media-file-name-with-ext}", string.IsNullOrEmpty(videoFileName) ? videoFileName : System.IO.Path.GetFileName(videoFileName));
        template = template.Replace("{#lines}", subtitles.Count.ToString(CultureInfo.InvariantCulture));
        if (template.Contains("{#total-words}"))
        {
            template = template.Replace("{#total-words}", CalculateTotalWords(subtitles).ToString(CultureInfo.InvariantCulture));
        }
        if (template.Contains("{#total-characters}"))
        {
            template = template.Replace("{#total-characters}", CalculateTotalCharacters(subtitles).ToString(CultureInfo.InvariantCulture));
        }

        template = template.Replace("{tab}", "\t");
        return template;
    }

    public static string GetParagraphTemplate(string template)
    {
        var s = template.Replace("{start}", "{0}");
        s = s.Replace("{end}", "{1}");
        s = s.Replace("{text}", "{2}");
        s = s.Replace("{original-text}", "{3}");
        s = s.Replace("{text-csv}", "{23}");
        s = s.Replace("{number}", "{4}");
        s = s.Replace("{number:", "{4:");
        s = s.Replace("{number,", "{4,");
        s = s.Replace("{number-1}", "{5}");
        s = s.Replace("{number-1:", "{5:");
        s = s.Replace("{duration}", "{6}");
        s = s.Replace("{actor}", "{7}");
        s = s.Replace("{actor-colon-space}", "{21}");
        s = s.Replace("{actor-upper-brackets-space}", "{22}");
        s = s.Replace("{text-line-1}", "{8}");
        s = s.Replace("{text-line-2}", "{9}");
        s = s.Replace("{cps-comma}", "{10}");
        s = s.Replace("{cps-period}", "{11}");
        s = s.Replace("{text-length}", "{12}");
        s = s.Replace("{text-length-br0}", "{13}");
        s = s.Replace("{text-length-br1}", "{14}");
        s = s.Replace("{text-length-br2}", "{15}");
        s = s.Replace("{gap}", "{16}");
        s = s.Replace("{bookmark}", "{17}");
        s = s.Replace("{media-file-name}", "{18}");
        s = s.Replace("{media-file-name-full}", "{19}");
        s = s.Replace("{media-file-name-with-ext}", "{20}");
        s = s.Replace("{tab}", "\t");
        return s;
    }

    public static string GetText(string text, string newLine)
    {
        if (!string.IsNullOrEmpty(newLine) && newLine != EnglishDoNotModify)
        {
            newLine = newLine.Replace("{newline}", Environment.NewLine);
            newLine = newLine.Replace("{tab}", "\t");
            newLine = newLine.Replace("{lf}", "\n");
            newLine = newLine.Replace("{cr}", "\r");
            return text.Replace(Environment.NewLine, newLine);
        }
        return text;
    }

    public static string GetTimeCode(TimeCode timeCode, string template)
    {
        var isNegative = timeCode.TotalMilliseconds < 0;
        var result = template;

        // Replace a leading run of s's/z's with total seconds/milliseconds,
        // e.g. "ss.zzz" => "61.160" and "zzz" => "61160" (SE 4.x semantics)
        result = ReplaceLeadingTotal(result, timeCode);

        // Replace fractional seconds (z's)
        result = ReplaceMilliseconds(result, timeCode);

        // Replace seconds (s's)
        result = ReplaceSeconds(result, timeCode);

        // Replace standard time components
        result = ReplaceStandardComponents(result, timeCode);

        // Add negative sign if needed
        if (isNegative)
        {
            result = "-" + result;
        }

        return result;
    }

    /// <summary>
    /// Template starts with a run of 's' or 'z' that means a total (total seconds
    /// or total milliseconds) rather than a clock component: "ss.zzz", "zzz", "s".
    /// A single leading 's'/'z' only counts when it is the whole template, so
    /// literal text like "s: hh:mm:ss" is left alone.
    /// </summary>
    internal static bool HasLeadingTotalRun(string template)
    {
        return GetLeadingTotalRunLength(template) > 0;
    }

    private static int GetLeadingTotalRunLength(string template)
    {
        if (template.Length == 0)
        {
            return 0;
        }

        var c = template[0];
        if (c != 's' && c != 'z')
        {
            return 0;
        }

        var run = 1;
        while (run < template.Length && template[run] == c)
        {
            run++;
        }

        return run >= 2 || run == template.Length ? run : 0;
    }

    private static string ReplaceLeadingTotal(string template, TimeCode timeCode)
    {
        var run = GetLeadingTotalRunLength(template);
        if (run == 0)
        {
            return template;
        }

        var rest = template.Substring(run);
        long total;
        if (template[0] == 'z')
        {
            total = (long)Math.Round(Math.Abs(timeCode.TotalMilliseconds), MidpointRounding.AwayFromZero);
        }
        else if (rest.Contains('z'))
        {
            // A fractional part follows, so the seconds must not be rounded up:
            // 61.960 as "ss.zzz" is "61.960", not "62.960"
            total = (long)Math.Floor(Math.Abs(timeCode.TotalSeconds));
        }
        else
        {
            total = (long)Math.Round(Math.Abs(timeCode.TotalSeconds), MidpointRounding.AwayFromZero);
        }

        return total.ToString(CultureInfo.InvariantCulture).PadLeft(run, '0') + rest;
    }

    private static string ReplaceMilliseconds(string template, TimeCode timeCode)
    {
        if (template.IndexOf('z') < 0)
        {
            return template;
        }

        // Any remaining z-run is a fraction of a second (a leading total run was already replaced)
        return ZRunRegex.Replace(template, match => FormatFractionalSeconds(timeCode, match.Value.Length));
    }

    private static string FormatFractionalSeconds(TimeCode timeCode, int desiredLength)
    {
        // The milliseconds component is the fraction of the second, exact to three digits;
        // computing "TotalSeconds - floor" instead loses precision (61.160 => "61.159...")
        var fracString = Math.Abs(timeCode.Milliseconds).ToString("000", CultureInfo.InvariantCulture);
        return desiredLength <= fracString.Length
            ? fracString.Substring(0, desiredLength)
            : fracString.PadRight(desiredLength, '0');
    }

    private static string ReplaceSeconds(string template, TimeCode timeCode)
    {
        if (template.IndexOf('s') < 0)
        {
            return template;
        }

        // Any remaining s-run is the seconds clock component (a leading total run was already replaced)
        var seconds = Math.Abs(timeCode.Seconds);
        return SRunRegex.Replace(template, match => seconds.ToString().PadLeft(match.Value.Length, '0'));
    }

    private static string ReplaceStandardComponents(string template, TimeCode timeCode)
    {
        // Process longer patterns first to avoid partial matches. Each component's value is
        // only formatted when its letter occurs in the template (the replaced digits cannot
        // introduce new pattern letters, so grouping "hh" with "h" is safe), and every
        // TimeCode getter re-runs a TimeSpan conversion - so read them at most once.
        var result = template;
        if (result.IndexOf('h') >= 0)
        {
            var hours = Math.Abs(timeCode.Hours);
            result = result.Replace("hh", hours.ToString("00"))
                           .Replace("h", hours.ToString());
        }

        if (result.IndexOf('m') >= 0)
        {
            var minutes = Math.Abs(timeCode.Minutes);
            result = result.Replace("mm", minutes.ToString("00"))
                           .Replace("m", minutes.ToString());
        }

        if (result.IndexOf('f') >= 0)
        {
            if (template == "ff")
            {
                // The whole template is "ff" = total frames
                return SubtitleFormat.MillisecondsToFrames(Math.Abs(timeCode.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture);
            }

            var framesInSecond = SubtitleFormat.MillisecondsToFramesMaxFrameRate(Math.Abs(timeCode.Milliseconds));
            result = result.Replace("ff", framesInSecond.ToString("00"))
                           .Replace("f", framesInSecond.ToString(CultureInfo.InvariantCulture));
        }

        return result;
    }

    internal static string GetParagraph(string template, string start, string end, string text, string originalText, int number, string actor, TimeCode duration, string gap, string timeCodeTemplate, Paragraph p, string videoFileName)
    {
        var cps = p.GetCharactersPerSecond();
        var d = duration.ToString();
        if (timeCodeTemplate == "ff" || timeCodeTemplate == "f")
        {
            d = SubtitleFormat.MillisecondsToFrames(duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }

        if (HasLeadingTotalRun(timeCodeTemplate))
        {
            // Totals ("zzz", "ss", "ss.zzz", ...): the duration renders just like a time code
            d = GetTimeCode(duration, timeCodeTemplate);
        }
        else if (timeCodeTemplate.EndsWith("ss.ff", StringComparison.Ordinal))
        {
            if (duration.Minutes > 0 && timeCodeTemplate.EndsWith("mm:ss.ff"))
            {
                d = $"{duration.Minutes:00}:{duration.Seconds:00}.{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
            else
            {
                d = $"{duration.Seconds:00}.{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
            }
        }
        else if (timeCodeTemplate.EndsWith("ss:ff", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss,ff", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00},{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss;ff", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00};{SubtitleFormat.MillisecondsToFramesMaxFrameRate(duration.Milliseconds):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss.zzz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}.{duration.Milliseconds:000}";
        }
        else if (timeCodeTemplate.EndsWith("ss:zzz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}:{duration.Milliseconds:000}";
        }
        else if (timeCodeTemplate.EndsWith("ss,zzz", StringComparison.Ordinal))
        {
            if (duration.Minutes > 0 && timeCodeTemplate.EndsWith("mm:ss,zzz"))
            {
                d = $"{duration.Minutes:00}:{duration.Seconds:00},{duration.Milliseconds:000}";
            }
            else
            {
                d = $"{duration.Seconds:00},{duration.Milliseconds:000}";
            }
        }
        else if (timeCodeTemplate.EndsWith("ss;zzz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00};{duration.Milliseconds:000}";
        }
        else if (timeCodeTemplate.EndsWith("ss.zz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}.{Math.Round(duration.Milliseconds / 10.0):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss:zz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00}:{Math.Round(duration.Milliseconds / 10.0):00}";
        }
        else if (timeCodeTemplate.EndsWith("ss,zz", StringComparison.Ordinal))
        {
            if (duration.Minutes > 0 && timeCodeTemplate.EndsWith("mm:ss,zz"))
            {
                d = $"{duration.Minutes:00}:{duration.Seconds:00},{Math.Round(duration.Milliseconds / 10.0):00}";
            }
            else
            {
                d = $"{duration.Seconds:00},{Math.Round(duration.Milliseconds / 10.0):00}";
            }
        }
        else if (timeCodeTemplate.EndsWith("ss;zz", StringComparison.Ordinal))
        {
            d = $"{duration.Seconds:00};{Math.Round(duration.Milliseconds / 10.0):00}";
        }

        var lines = text.SplitToLines();
        var line1 = string.Empty;
        var line2 = string.Empty;
        if (lines.Count > 0)
        {
            line1 = lines[0];
        }

        if (lines.Count > 1)
        {
            line2 = lines[1];
        }

        var s = template;
        var replaceStart = GetReplaceChar(s);
        var replaceEnd = GetReplaceChar(s + replaceStart);
        var actorColonSpace = string.IsNullOrEmpty(actor) ? string.Empty : $"{actor}: ";
        var actorUppercaseBracketsSpace = string.IsNullOrEmpty(actor) ? string.Empty : $"[{actor.ToUpperInvariant()}] ";
        s = PreBeginCurly(s, replaceStart);
        s = PreEndCurly(s, replaceEnd);
        var textLengthNoLineBreaks = p.Text.RemoveChar('\r', '\n').Length;
        s = string.Format(s, start, end, text, originalText, number + 1, number, d, actor, line1, line2,
                          cps.ToString(CultureInfo.InvariantCulture).Replace(".", ","),
                          cps.ToString(CultureInfo.InvariantCulture),
                          text.Length,
                          textLengthNoLineBreaks,
                          textLengthNoLineBreaks + lines.Count - 1,
                          textLengthNoLineBreaks + (lines.Count - 1) * 2,
                          gap,
                          p.Bookmark == string.Empty ? "*" : p.Bookmark,
                          string.IsNullOrEmpty(videoFileName) ? string.Empty : System.IO.Path.GetFileNameWithoutExtension(videoFileName),
                          videoFileName,
                          string.IsNullOrEmpty(videoFileName) ? string.Empty : System.IO.Path.GetFileName(videoFileName),
                          actorColonSpace,
                          actorUppercaseBracketsSpace,
                          CsvEscape(text)
                          );
        s = PostCurly(s, replaceStart, replaceEnd);
        return s;
    }

    private static string CsvEscape(string s)
    {
        if (s.Contains('"'))
        {
            s = s.Replace("\"", "\"\"");
        }

        s = $"\"{s}\"";

        return s;
    }

    private static readonly string[] ReplaceCharCandidates = { "@", "¤", "%", "=", "+", "æ", "Æ", "`", "*", ";" };

    private static string GetReplaceChar(string s)
    {
        foreach (var c in ReplaceCharCandidates)
        {
            if (!s.Contains(c[0]))
            {
                return c;
            }
        }

        return string.Empty;
    }

    private static string PreBeginCurly(string s, string replaceStart)
    {
        if (string.IsNullOrEmpty(replaceStart))
        {
            return s;
        }

        var indices = GetCurlyBeginIndexesReversed(s);
        for (var i = 0; i < indices.Count; i++)
        {
            var idx = indices[i];
            s = s.Remove(idx, 1);
            s = s.Insert(idx, replaceStart);
        }

        return s;
    }

    private static string PreEndCurly(string s, string replaceEnd)
    {
        if (string.IsNullOrEmpty(replaceEnd))
        {
            return s;
        }

        var indices = GetCurlyEndIndexesReversed(s);
        for (var i = 0; i < indices.Count; i++)
        {
            var idx = indices[i];
            s = s.Remove(idx, 1);
            s = s.Insert(idx, replaceEnd);
        }

        return s;
    }

    private static string PostCurly(string s, string replaceStart, string replaceEnd)
    {
        if (!string.IsNullOrEmpty(replaceStart))
        {
            s = s.Replace(replaceStart, "{");
        }

        if (!string.IsNullOrEmpty(replaceEnd))
        {
            s = s.Replace(replaceEnd, "}");
        }

        return s;
    }

    private static List<int> GetCurlyBeginIndexesReversed(string s)
    {
        var matchIndices = new HashSet<int>();
        foreach (var match in CurlyCodePattern.EnumerateMatches(s))
        {
            matchIndices.Add(match.Index);
        }

        var list = new List<int>();
        for (var i = s.Length - 1; i >= 0; i--)
        {
            var c = s[i];
            if (c == '{' && !matchIndices.Contains(i))
            {
                list.Add(i);
            }
        }

        return list;
    }

    private static List<int> GetCurlyEndIndexesReversed(string s)
    {
        var matchIndices = new HashSet<int>();
        foreach (var match in CurlyCodePattern.EnumerateMatches(s))
        {
            matchIndices.Add(match.Index + match.Length - 1);
        }

        var list = new List<int>();
        for (var i = s.Length - 1; i >= 0; i--)
        {
            var c = s[i];
            if (c == '}' && !matchIndices.Contains(i))
            {
                list.Add(i);
            }
        }

        return list;
    }

    private static int CalculateTotalWords(List<Paragraph> subtitles)
    {
        var wordCount = 0;
        foreach (var p in subtitles)
        {
            wordCount += p.Text.CountWords();
        }

        return wordCount;
    }

    private static int CalculateTotalCharacters(List<Paragraph> subtitles)
    {
        decimal characterCount = 0;
        foreach (var p in subtitles)
        {
            characterCount += p.Text.CountCharacters(false);
        }

        return (int)characterCount;
    }
}
