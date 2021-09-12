using Nikse.SubtitleEdit.Core.Common;
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
    /// https://w3c.github.io/webvtt/
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
                "cyan", Color.FromArgb(0, 255, 255)
            },
            {
                "red", Color.FromArgb(255, 0, 0)
            },
            {
                "yellow", Color.FromArgb(255, 255, 0)
            },
            {
                "magenta", Color.FromArgb(255, 0, 255)
            },
            {
                "blue", Color.FromArgb(0, 0, 255)
            },
            {
                "black", Color.FromArgb(0, 0, 0)
            },
        };

        private static readonly string[] KnownLanguages = new[] { "arabic", "hebrew", "simplifiedchinese", "traditionalchinese", "thai", "korean", "Japanese", "hungarian", "czech", "vietnamese" };

        public override string Extension => ".vtt";

        public override string Name => "WebVTT";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string timeCodeFormatHours = "{0:00}:{1:00}:{2:00}.{3:000}"; // hh:mm:ss.mmm
            const string paragraphWriteFormat = "{0} --> {1}{2}{5}{3}{4}{5}";

            var sb = new StringBuilder();
            if (subtitle.Header != null && subtitle.Header.StartsWith("WEBVTT", StringComparison.Ordinal))
            {
                sb.AppendLine(subtitle.Header.Trim());
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("WEBVTT");
                sb.AppendLine();
            }

            foreach (var p in subtitle.Paragraphs)
            {
                string start = string.Format(timeCodeFormatHours, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                string end = string.Format(timeCodeFormatHours, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                string positionInfo = GetPositionInfoFromAssTag(p);

                var style = string.Empty;
                if (subtitle.Header != null && subtitle.Header.StartsWith("WEBVTT", StringComparison.Ordinal))
                {
                    if (!string.IsNullOrEmpty(p.Extra))
                    {
                        style = p.Extra;
                    }

                    if (!string.IsNullOrEmpty(p.Region))
                    {
                        positionInfo = $" region:{p.Region} {positionInfo}".Replace("  ", " ").TrimEnd();
                    }
                }

                sb.AppendLine(string.Format(paragraphWriteFormat, start, end, positionInfo, FormatText(p), style, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        internal static string GetPositionInfoFromAssTag(Paragraph p)
        {
            string positionInfo;
            if (p.Text.StartsWith("{\\an1", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn1;
            }
            else if (p.Text.StartsWith("{\\an3", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn3;
            }
            else if (p.Text.StartsWith("{\\an4", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn4;
            }
            else if (p.Text.StartsWith("{\\an5", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn5;
            }
            else if (p.Text.StartsWith("{\\an6", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn6;
            }
            else if (p.Text.StartsWith("{\\an7", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn7;
            }
            else if (p.Text.StartsWith("{\\an8", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn8;
            }
            else if (p.Text.StartsWith("{\\an9", StringComparison.Ordinal))
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn9;
            }
            else
            {
                positionInfo = Configuration.Settings.SubtitleSettings.WebVttCueAn2;
            }

            return (" " + positionInfo).TrimEnd();
        }

        internal static string FormatText(Paragraph p)
        {
            string text = Utilities.RemoveSsaTags(p.Text);
            while (text.Contains(Environment.NewLine + Environment.NewLine))
            {
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            }

            text = ColorHtmlToWebVtt(text);
            text = EscapeEncodeText(text);
            return text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            string positionInfo = string.Empty;
            bool hadEmptyLine = false;
            int numbers = 0;
            double addSeconds = 0;
            var noteOn = false;
            var styleOn = false;
            var regionOn = false;
            var header = new StringBuilder();
            header.AppendLine("WEBVTT");
            header.AppendLine();
            for (var index = 0; index < lines.Count; index++)
            {
                string line = lines[index];
                string next = string.Empty;
                if (index < lines.Count - 1)
                {
                    next = lines[index + 1];
                }

                if (index == 0 && line.StartsWith("WEBVTT", StringComparison.Ordinal))
                {
                    header.Clear();
                    header.AppendLine(line);
                    header.AppendLine();
                    continue;
                }

                if (index > 0 && string.IsNullOrEmpty(lines[index - 1]) &&
                    (line == "NOTE" || line.StartsWith("NOTE ", StringComparison.Ordinal)))
                {
                    noteOn = true;
                    if (subtitle.Paragraphs.Count == 0)
                    {
                        header.AppendLine();
                        header.AppendLine();
                    }
                }
                else if ((line == "STYLE" || line.StartsWith("STYLE ", StringComparison.Ordinal)) && subtitle.Paragraphs.Count == 0)
                {
                    styleOn = true;
                    header.AppendLine();
                    header.AppendLine();
                }
                else if ((line == "REGION" || line.StartsWith("REGION ", StringComparison.Ordinal)) && subtitle.Paragraphs.Count == 0)
                {
                    regionOn = true;
                    header.AppendLine();
                    header.AppendLine();
                }

                if (styleOn && !string.IsNullOrEmpty(line))
                {
                    header.AppendLine(line);
                    continue;
                }

                if (regionOn && !string.IsNullOrEmpty(line))
                {
                    header.AppendLine(line);
                    continue;
                }

                if (noteOn && !string.IsNullOrEmpty(line))
                {
                    if (subtitle.Paragraphs.Count == 0)
                    {
                        header.AppendLine(line);
                    }

                    continue;
                }

                if (index > 1 && (line.StartsWith("X-TIMESTAMP-MAP=", StringComparison.OrdinalIgnoreCase) || line == "WEBVTT"))
                {
                    // badly formatted web vtt file
                    continue;
                }

                noteOn = false;
                styleOn = false;
                regionOn = false;

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

                if (index == 1 && s.StartsWith("X-TIMESTAMP-MAP=", StringComparison.OrdinalIgnoreCase) &&
                    s.IndexOf("MPEGTS:", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    addSeconds = GetXTimeStampSeconds(s);
                }
                else if (isTimeCode && RegexTimeCodes.IsMatch(s.TrimStart()))
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
                        p.Region = GetRegion(s);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                        p = null;
                    }

                    hadEmptyLine = false;
                }
                else if (p != null && hadEmptyLine &&
                         (RegexTimeCodesMiddle.IsMatch(next) ||
                          RegexTimeCodesShort.IsMatch(next) ||
                          RegexTimeCodes.IsMatch(next)))
                {
                    // can both be number or an "identifier" which can be text
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
                paragraph.Text = EscapeDecodeText(paragraph.Text);
                paragraph.Text = RemoveWeirdReatingHeader(paragraph.Text);
                paragraph.StartTime.TotalMilliseconds += addSeconds * 1000;
                paragraph.EndTime.TotalMilliseconds += addSeconds * 1000;
            }
            subtitle.Renumber();
            if (header.Length > 0)
            {
                subtitle.Header = header
                    .ToString()
                    .Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine)
                    .Trim();
            }
        }

        private string RemoveWeirdReatingHeader(string input)
        {
            var text = input;
            text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
            text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
            if (text.Contains(Environment.NewLine + "WEBVTT"))
            {
                if (text.TrimEnd().EndsWith('}') && text.Contains("STYLE"))
                {
                    text = text.Remove(text.IndexOf(Environment.NewLine + "WEBVTT")).Trim();
                }
            }
            else if (text.TrimEnd().EndsWith(Environment.NewLine + "WEBVTT", StringComparison.Ordinal))
            {
                text = text.Remove(text.LastIndexOf(Environment.NewLine + "WEBVTT")).Trim();
            }
            else if (text.Contains(Environment.NewLine + "STYLE" + Environment.NewLine))
            {
                if (text.TrimEnd().EndsWith("}"))
                {
                    text = text.Remove(text.IndexOf(Environment.NewLine + "STYLE" + Environment.NewLine)).Trim();
                }
            }

            return text;
        }

        private static double GetXTimeStampSeconds(string input)
        {
            if (!Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap)
            {
                return 0;
            }

            var s = input.RemoveChar(' ');

            var subtractSeconds = 0d;
            var startIndex = s.IndexOf("LOCAL:", StringComparison.OrdinalIgnoreCase);
            var localSb = new StringBuilder();
            for (int i = startIndex + 6; i < s.Length; i++)
            {
                var ch = s[i];
                if (char.IsNumber(ch) || ch == ':' || ch == '.')
                {
                    localSb.Append(ch);
                }
                else
                {
                    break;
                }
            }
            var parts = localSb.ToString().Split(':', '.');
            if (parts.Length == 3)
            {
                parts = ("00:" + localSb).Split(':', '.');
            }
            if (parts.Length == 4)
            {
                subtractSeconds = DecodeTimeCodeMsFourParts(parts).TotalSeconds;
            }

            startIndex = s.IndexOf("MPEGTS:", StringComparison.OrdinalIgnoreCase);
            var tsSb = new StringBuilder();
            for (int i = startIndex + 7; i < s.Length; i++)
            {
                var ch = s[i];
                if (char.IsNumber(ch))
                {
                    tsSb.Append(ch);
                }
                else
                {
                    break;
                }
            }

            if (tsSb.Length > 0)
            {
                if (long.TryParse(tsSb.ToString(), out var number))
                {
                    var seconds = (double)number / Configuration.Settings.SubtitleSettings.WebVttTimescale - subtractSeconds;
                    if (seconds > 0 && seconds < 90000) // max 25 hours - or wrong timescale
                    {
                        return seconds;
                    }
                }
            }

            return 0;
        }

        internal static string GetPositionInfo(string s)
        {
            //position: x --- 0% = left, 100% = right (horizontal)
            //line: x --- 0 or -16 or 0% = top, 16 or -1 or 100% = bottom (vertical)
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

        internal static string GetRegion(string s)
        {
            var region = GetTag(s, "region:");
            return region;
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
            var regexRemoveTimeCodes = new Regex(@"\<\d+:\d+:\d+\.\d+\>", RegexOptions.Compiled); // <00:00:10.049>
            var regexTagsPlusWhiteSpace = new Regex(@"(\{\\an\d\})[\s\r\n]+", RegexOptions.Compiled); // <00:00:10.049>

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.Contains('<') || p.Text.Contains('&'))
                {
                    var text = p.Text.Replace("&rlm;", string.Empty).Replace("&lrm;", string.Empty); // or use rlm=\u202B, lrm=\u202A ?
                    foreach (var knownLanguage in KnownLanguages)
                    {
                        text = text.Replace("<c." + knownLanguage + ">", string.Empty).Replace("</c." + knownLanguage + ">", string.Empty);
                    }
                    text = System.Net.WebUtility.HtmlDecode(text);

                    var match = regexWebVttColorMulti.Match(text);                    
                    while (match.Success)
                    {
                        var tag = match.Value.Substring(3, match.Value.Length - 4);
                        tag = FindBestColorTagOrDefault(tag);
                        if (tag == null)
                        {
                            text = text.Replace(match.Value, string.Empty);
                            text = text.Replace(match.Value.Insert(1, "/"), string.Empty);
                            match = regexWebVttColorMulti.Match(text);
                            continue;
                        }
                        var fontString = "<font color=\"" + tag + "\">";
                        fontString = fontString.Trim('"').Trim('\'');
                        text = text.Remove(match.Index, match.Length).Insert(match.Index, fontString);
                        var endIndex = text.IndexOf("</c>", match.Index, StringComparison.OrdinalIgnoreCase);
                        if (endIndex >= 0)
                        {
                            text = text.Remove(endIndex, 4).Insert(endIndex, "</font>");
                        }
                        else
                        {
                            endIndex = text.IndexOf("</c.", match.Index, StringComparison.OrdinalIgnoreCase);
                            if (endIndex >= 0)
                            {
                                var endEndIndex = text.IndexOf('>', endIndex);
                                if (endEndIndex > 0)
                                {
                                    text = text.Remove(endIndex, endEndIndex - endIndex).Insert(endIndex, "</font>");
                                }
                            }
                        }
                        match = regexWebVttColorMulti.Match(text);
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

                if (!KnownLanguages.Contains(value))
                {
                    var fontString = "<font color=\"" + value + "\">";
                    fontString = fontString.Trim('"').Trim('\'');
                    res = res.Remove(match.Index, match.Length).Insert(match.Index, fontString);
                    var endIndex = res.IndexOf("</c>", match.Index, StringComparison.OrdinalIgnoreCase);
                    if (endIndex >= 0)
                    {
                        res = res.Remove(endIndex, 4).Insert(endIndex, "</font>");
                    }
                    else 
                    {
                        var findString = $"</c.{value}>";
                        endIndex = res.IndexOf(findString, match.Index, StringComparison.OrdinalIgnoreCase);
                        if (endIndex >= 0)
                        {
                            res = res.Remove(endIndex, findString.Length).Insert(endIndex, "</font>");
                        }
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

        internal static string EscapeEncodeText(string input)
        {
            if (!input.Contains('<') && !input.Contains('>') && !input.Contains('&'))
            {
                return input;
            }
            var sb = new StringBuilder(input.Length);
            var max = input.Length;
            int i = 0;
            var tagOn = false;
            while (i < max)
            {
                var ch = input[i];
                if (ch == '<')
                {
                    var s = input.Substring(i);
                    if (s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<c>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<v>", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append(s.Substring(0, 3));
                        i += 3;
                    }
                    else if (s.StartsWith("</", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append(s.Substring(0, 2));
                        i += 2;
                        tagOn = true;
                    }
                    else if (s.StartsWith("<ruby", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<font", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<v.", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<v ", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<c.", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<c ", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<lang.", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<lang ", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append(ch);
                        i++;
                        tagOn = true;
                    }
                    else
                    {
                        sb.Append("&lt;");
                        i++;
                    }
                }
                else if (ch == '>')
                {
                    if (tagOn)
                    {
                        sb.Append(ch);
                        i++;
                        tagOn = false;
                    }
                    else
                    {
                        sb.Append("&gt;");
                        i++;
                    }
                }
                else if (ch == '&')
                {
                    var s = input.Substring(i);
                    if (s.StartsWith("&lrm;", StringComparison.OrdinalIgnoreCase) ||
                       s.StartsWith("&amp;", StringComparison.OrdinalIgnoreCase) ||
                       s.StartsWith("&lt;", StringComparison.OrdinalIgnoreCase) ||
                       s.StartsWith("&gt;", StringComparison.OrdinalIgnoreCase) ||
                       s.Length > 3 && s[3] == ';' && char.IsLetter(s[2]) && char.IsLetter(s[1]) ||
                       s.Length > 4 && s[4] == ';' && char.IsLetter(s[3]) && char.IsLetter(s[2]) && char.IsLetter(s[1]))
                    {
                        sb.Append(ch);
                        i++;
                    }
                    else
                    {
                        sb.Append("&amp;");
                        i++;
                    }
                }
                else
                {
                    sb.Append(ch);
                    i++;
                }
            }
            return sb.ToString();
        }

        internal static string EscapeDecodeText(string input)
        {
            return input
                .Replace("&gt;", ">")
                .Replace("&lt;", "<")
                .Replace("&amp;", "&");
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
