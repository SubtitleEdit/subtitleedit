using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// http://www.whatwg.org/specs/web-apps/current-work/webvtt.html
    /// </summary>
    public class WebVTT : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesMiddle = new Regex(@"^-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesShort = new Regex(@"^-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);

        private static readonly Dictionary<string, Color> DefaultColorClasses = new Dictionary<string, Color>
        {
            {
                "white", Color.FromArgb(255, 255, 255)
            },
            {
                "lime", Color.FromArgb(0, 255, 0)
            },
            {
                "cyan", Color.FromArgb(0,255,255)
            },
            {
                "red", Color.FromArgb(255,0,0)
            },
            {
                "yellow", Color.FromArgb(255,255,0)
            },
            {
                "magenta", Color.FromArgb(255,0,255)
            },
            {
                "blue", Color.FromArgb(0,0,255)
            },
            {
                "black", Color.FromArgb(0,0,0)
            },
        };

        public override string Extension => ".vtt";

        public override string Name => "WebVTT";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string timeCodeFormatHours = "{0:00}:{1:00}:{2:00}.{3:000}"; // hh:mm:ss.mmm
            const string paragraphWriteFormat = "{0} --> {1}{2}{5}{3}{4}{5}";

            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT");
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string start = string.Format(timeCodeFormatHours, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                string end = string.Format(timeCodeFormatHours, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                string positionInfo = GetPositionInfoFromAssTag(p);

                string style = string.Empty;
                if (!string.IsNullOrEmpty(p.Extra) && subtitle.Header == "WEBVTT")
                {
                    style = p.Extra;
                }

                sb.AppendLine(string.Format(paragraphWriteFormat, start, end, positionInfo, FormatText(p), style, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        internal static string GetPositionInfoFromAssTag(Paragraph p)
        {
            string positionInfo = string.Empty;

            if (p.Text.StartsWith("{\\a", StringComparison.Ordinal))
            {
                string position = null; // horizontal
                if (p.Text.StartsWith("{\\an1}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an7}", StringComparison.Ordinal)) // advanced sub station alpha
                {
                    position = "20%"; //left
                }
                else if (p.Text.StartsWith("{\\an3}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an6}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal)) // advanced sub station alpha
                {
                    position = "80%"; //right
                }

                string line = null;
                if (p.Text.StartsWith("{\\an7}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an8}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an9}", StringComparison.Ordinal)) // advanced sub station alpha
                {
                    line = "20%"; //top
                }
                else if (p.Text.StartsWith("{\\an4}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an5}", StringComparison.Ordinal) || p.Text.StartsWith("{\\an6}", StringComparison.Ordinal)) // advanced sub station alpha
                {
                    line = "50%"; //middle
                }

                if (!string.IsNullOrEmpty(position))
                {
                    positionInfo = " position:" + position;
                }
                if (!string.IsNullOrEmpty(line))
                {
                    positionInfo += " line:" + line;
                }
            }

            return positionInfo;
        }

        internal static string FormatText(Paragraph p)
        {
            string text = Utilities.RemoveSsaTags(p.Text);
            while (text.Contains(Environment.NewLine + Environment.NewLine))
            {
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            text = ColorHtmlToWebVtt(text);
            return text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            string positionInfo = string.Empty;
            bool hadEmptyLine = false;
            int numbers = 0;
            for (var index = 0; index < lines.Count; index++)
            {
                string line = lines[index];
                string next = string.Empty;
                if (index < lines.Count - 1)
                {
                    next = lines[index + 1];
                }

                var s = line;
                bool isTimeCode = line.Contains("-->");
                if (isTimeCode && RegexTimeCodesMiddle.IsMatch(s))
                {
                    s = "00:" + s; // start is without hours, end is with hours
                }

                if (isTimeCode && RegexTimeCodesShort.IsMatch(s))
                {
                    s = "00:" + s.Replace("--> ", "--> 00:");
                }

                if (isTimeCode && RegexTimeCodes.IsMatch(s.TrimStart()))
                {
                    if (p != null)
                    {
                        p.Text = p.Text.TrimEnd();
                        subtitle.Paragraphs.Add(p);
                    }

                    try
                    {
                        var parts = s.TrimStart().Replace("-->", "@").Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                        p = new Paragraph
                        {
                            StartTime = GetTimeCodeFromString(parts[0]),
                            EndTime = GetTimeCodeFromString(parts[1])
                        };
                        positionInfo = GetPositionInfo(s);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                        p = null;
                    }

                    hadEmptyLine = false;
                }
                else if (subtitle.Paragraphs.Count == 0 && line.Trim() == "WEBVTT")
                {
                    subtitle.Header = "WEBVTT";
                }
                else if (p != null && hadEmptyLine && Utilities.IsInteger(line.RemoveChar('-')) &&
                         (RegexTimeCodesMiddle.IsMatch(next) ||
                          RegexTimeCodesShort.IsMatch(next) ||
                          RegexTimeCodes.IsMatch(next)))
                {
                    numbers++;
                }
                else if (p != null)
                {
                    string text = positionInfo + line.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        hadEmptyLine = true;
                    }

                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = text + Environment.NewLine;
                    }
                    else
                    {
                        p.Text += text + Environment.NewLine;
                    }

                    positionInfo = string.Empty;
                }
            }

            if (p != null)
            {
                p.Text = p.Text.TrimEnd();
                subtitle.Paragraphs.Add(p);
            }

            if (subtitle.Paragraphs.Count > 5 &&
                numbers >= subtitle.Paragraphs.Count - 1 &&
                lines[0] == "WEBVTT FILE")
            {
                // let format WebVTTFileWithLineNumber take the subtitle
                _errorCount = subtitle.Paragraphs.Count + 1;
                return;
            }

            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = ColorWebVttToHtml(paragraph.Text);
            }

            subtitle.Renumber();
        }

        internal static string GetPositionInfo(string s)
        {
            //position: x --- 0% = left, 100%=right (horizontal)
            //line: x --- 0 or -16 or 0%=top, 16 or -1 or 100% = bottom (vertical)
            var pos = GetTag(s, "position:");
            var line = GetTag(s, "line:");
            var positionInfo = string.Empty;
            bool hAlignLeft = false;
            bool hAlignRight = false;
            bool vAlignTop = false;
            bool vAlignMiddle = false;

            if (!string.IsNullOrEmpty(pos) && pos.EndsWith('%'))
            {
                if (double.TryParse(pos.TrimEnd('%'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                {
                    if (number < 25)
                    {
                        hAlignLeft = true;
                    }
                    else if (number > 75)
                    {
                        hAlignRight = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(line))
            {
                line = line.Trim();
                if (line.EndsWith('%'))
                {
                    if (double.TryParse(line.TrimEnd('%'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                    {
                        if (number < 25)
                        {
                            vAlignTop = true;
                        }
                        else if (number < 75)
                        {
                            vAlignMiddle = true;
                        }
                    }
                }
                else
                {
                    if (double.TryParse(line, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                    {
                        if (number >= 0 && number <= 7)
                        {
                            vAlignTop = true; // Positive numbers indicate top down
                        }
                        else if (number > 7 && number < 11)
                        {
                            vAlignMiddle = true;
                        }
                    }
                }
            }

            if (hAlignLeft)
            {
                if (vAlignTop)
                {
                    return "{\\an7}";
                }
                if (vAlignMiddle)
                {
                    return "{\\an4}";
                }
                return "{\\an1}";
            }

            if (hAlignRight)
            {
                if (vAlignTop)
                {
                    return "{\\an9}";
                }
                if (vAlignMiddle)
                {
                    return "{\\an6}";
                }
                return "{\\an3}";
            }

            if (vAlignTop)
            {
                return "{\\an8}";
            }

            if (vAlignMiddle)
            {
                return "{\\an5}";
            }

            return positionInfo;
        }

        private static string GetTag(string s, string tag)
        {
            var pos = s.IndexOf(tag, StringComparison.Ordinal);
            if (pos >= 0)
            {
                var v = s.Substring(pos + tag.Length).Trim();
                var end = v.IndexOf("%,", StringComparison.Ordinal);
                if (end >= 0)
                {
                    v = v.Remove(end + 1);
                }
                end = v.IndexOf(' ');
                if (end >= 0)
                {
                    v = v.Remove(end);
                }
                return v;
            }
            return null;
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            var regexWebVttColorMulti = new Regex(@"<c.[a-z0-9_\.]*>", RegexOptions.Compiled);
            var regexRemoveCTags = new Regex(@"\</?c([a-zA-Z\._\d]*)\>", RegexOptions.Compiled);
            var regexRemoveTimeCodes = new Regex(@"\<\d+:\d+:\d+.\d+\>", RegexOptions.Compiled); // <00:00:10.049>
            var regexTagsPlusWhiteSpace = new Regex(@"(\{\\an\d\})[\s\r\n]+", RegexOptions.Compiled); // <00:00:10.049>

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.Contains('<'))
                {
                    string text = p.Text
                        .Replace("<c.arabic>", string.Empty).Replace("</c.arabic>", string.Empty)
                        .Replace("<c.hebrew>", string.Empty).Replace("</c.hebrew>", string.Empty)
                        .Replace("<c.simplifiedchinese>", string.Empty).Replace("</c.simplifiedchinese>", string.Empty)
                        .Replace("<c.traditionalchinese>", string.Empty).Replace("</c.traditionalchinese>", string.Empty)
                        .Replace("<c.thai>", string.Empty).Replace("</c.thai>", string.Empty)
                        .Replace("<c.korean>", string.Empty).Replace("</c.korean>", string.Empty)
                        .Replace("<c.Japanese>", string.Empty).Replace("</c.Japanese>", string.Empty)
                        .Replace("&rlm;", "\u202B")
                        .Replace("&lrm;", "\u202A");

                    text = System.Net.WebUtility.HtmlDecode(text);

                    var match = regexWebVttColorMulti.Match(text);
                    while (match.Success)
                    {
                        var tag = match.Value.Substring(3, match.Value.Length - 4);
                        tag = FindBestColorTagOrDefault(tag);
                        if (tag == null)
                        {
                            break;
                        }
                        var fontString = "<font color=\"" + tag + "\">";
                        fontString = fontString.Trim('"').Trim('\'');
                        text = text.Remove(match.Index, match.Length).Insert(match.Index, fontString);
                        var endIndex = text.IndexOf("</c>", match.Index, StringComparison.OrdinalIgnoreCase);
                        if (endIndex >= 0)
                        {
                            text = text.Remove(endIndex, 4).Insert(endIndex, "</font>");
                        }
                        match = RegexWebVttColor.Match(text);
                    }

                    text = RemoveTag("v", text);
                    text = RemoveTag("rt", text);
                    text = RemoveTag("ruby", text);
                    text = RemoveTag("span", text);
                    text = regexRemoveCTags.Replace(text, string.Empty).Trim();
                    text = regexRemoveTimeCodes.Replace(text, string.Empty).Trim();
                    text = regexTagsPlusWhiteSpace.Replace(text, "$1");
                    p.Text = text;
                }
            }
        }

        private string FindBestColorTagOrDefault(string tag)
        {
            var tags = tag.Split('.').ToList();
            tags.Reverse();
            foreach (var s in tags)
            {
                var l = s.ToLowerInvariant();
                if (DefaultColorClasses.Keys.Contains(l))
                {
                    return l;
                }

                if (l.StartsWith("color") && l.Length > 6 && Utilities.IsHex(l.Remove(0, 5))) // e.g. color008000
                {
                    return "#" + l.Remove(0, 5);
                }
            }
            return null;
        }

        private static readonly Regex RegexWebVttColor = new Regex(@"<c.[a-z]*>", RegexOptions.Compiled);
        private static readonly Regex RegexWebVttColorHex = new Regex(@"<c.[a-z]*\d+>", RegexOptions.Compiled);

        internal static string ColorWebVttToHtml(string text)
        {
            var res = RunColorRegEx(text, RegexWebVttColor);
            res = RunColorRegEx(res, RegexWebVttColorHex);
            return res;
        }

        private static string RunColorRegEx(string input, Regex regex)
        {
            var res = input;
            var match = regex.Match(res);
            while (match.Success)
            {
                var value = match.Value.Substring(3, match.Value.Length - 4);
                if (match.Value.StartsWith("<c.color", StringComparison.Ordinal))
                {
                    value = "#" + match.Value.Substring(3 + 5, match.Value.Length - 4 - 5);
                }

                if (value != "arabic")
                {
                    var fontString = "<font color=\"" + value + "\">";
                    fontString = fontString.Trim('"').Trim('\'');
                    res = res.Remove(match.Index, match.Length).Insert(match.Index, fontString);
                    var endIndex = res.IndexOf("</c>", match.Index, StringComparison.OrdinalIgnoreCase);
                    if (endIndex >= 0)
                    {
                        res = res.Remove(endIndex, 4).Insert(endIndex, "</font>");
                    }
                }

                match = regex.Match(res, match.Index + 1);
            }
            return res;
        }

        private static readonly Regex RegexHtmlColor = new Regex("<font color=\"[a-z]*\">", RegexOptions.Compiled);
        private static readonly Regex RegexHtmlColor2 = new Regex("<font color=[a-z]*>", RegexOptions.Compiled);
        private static readonly Regex RegexHtmlColor3 = new Regex("<font color=\"#[ABCDEFabcdef\\d]*\">", RegexOptions.Compiled);

        private static string ColorHtmlToWebVtt(string text)
        {
            var res = text.Replace("</font>", "</c>");
            var match = RegexHtmlColor.Match(res);
            while (match.Success)
            {
                var fontString = "<c." + match.Value.Substring(13, match.Value.Length - 15) + ">";
                fontString = fontString.Trim('"').Trim('\'');
                res = res.Remove(match.Index, match.Length).Insert(match.Index, fontString);
                match = RegexHtmlColor.Match(res);
            }

            match = RegexHtmlColor2.Match(res);
            while (match.Success)
            {
                var fontString = "<c." + match.Value.Substring(12, match.Value.Length - 13) + ">";
                fontString = fontString.Trim('"').Trim('\'');
                res = res.Remove(match.Index, match.Length).Insert(match.Index, fontString);
                match = RegexHtmlColor2.Match(res);
            }

            match = RegexHtmlColor3.Match(res);
            while (match.Success)
            {
                var tag = match.Value.Substring(14, match.Value.Length - 16);
                var fontString = "<c.color" + tag + ">";
                var closeColor = GetCloseColor(tag);
                if (closeColor != null)
                {
                    fontString = "<c." + closeColor + ">";
                }
                fontString = fontString.Trim('"').Trim('\'');
                res = res.Remove(match.Index, match.Length).Insert(match.Index, fontString);
                match = RegexHtmlColor3.Match(res);
            }
            return res;
        }

        private static string GetCloseColor(string tag)
        {
            try
            {
                var c = ColorTranslator.FromHtml("#" + tag.Trim('#'));
                int maxDiff = 25;
                foreach (var kvp in DefaultColorClasses)
                {
                    if (Math.Abs(kvp.Value.R - c.R) <= maxDiff &&
                        Math.Abs(kvp.Value.G - c.G) <= maxDiff &&
                        Math.Abs(kvp.Value.B - c.B) <= maxDiff)
                    {
                        return kvp.Key;
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public static List<string> GetVoices(Subtitle subtitle)
        {
            var list = new List<string>();
            if (subtitle?.Paragraphs != null)
            {
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    string s = p.Text;
                    var startIndex = s.IndexOf("<v ", StringComparison.Ordinal);
                    while (startIndex >= 0)
                    {
                        int endIndex = s.IndexOf('>', startIndex);
                        if (endIndex > startIndex)
                        {
                            string voice = s.Substring(startIndex + 2, endIndex - startIndex - 2).Trim();
                            if (!list.Contains(voice))
                            {
                                list.Add(voice);
                            }
                        }

                        if (startIndex == s.Length - 1)
                        {
                            startIndex = -1;
                        }
                        else
                        {
                            startIndex = s.IndexOf("<v ", startIndex + 1, StringComparison.Ordinal);
                        }
                    }
                }
            }
            return list;
        }

        public static string RemoveTag(string tag, string text)
        {
            var res = text;
            int indexOfTag = res.IndexOf("<" + tag + " ", StringComparison.Ordinal);
            if (indexOfTag >= 0)
            {
                int indexOfEnd = res.IndexOf('>', indexOfTag);
                if (indexOfEnd > 0)
                {
                    res = res.Remove(indexOfTag, indexOfEnd - indexOfTag + 1);
                    res = res.Replace("</" + tag + ">", string.Empty);
                }
            }
            return res;
        }

        internal static TimeCode GetTimeCodeFromString(string time)
        {
            // hh:mm:ss.mmm
            var timeCode = time.Trim().Split(':', '.', ' ');
            return new TimeCode(int.Parse(timeCode[0]),
                                int.Parse(timeCode[1]),
                                int.Parse(timeCode[2]),
                                int.Parse(timeCode[3]));
        }
    }
}
