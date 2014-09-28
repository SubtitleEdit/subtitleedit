using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// http://www.whatwg.org/specs/web-apps/current-work/webvtt.html
    /// </summary>
    public class WebVTT : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesMiddle = new Regex(@"^-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesShort = new Regex(@"^-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".vtt"; }
        }

        public override string Name
        {
            get { return "WebVTT"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string timeCodeFormatNoHours = "{0:00}:{1:00}.{2:000}"; // mm:ss.cc
            const string timeCodeFormatHours = "{0:00}:{1:00}:{2:00}.{3:000}"; // hh:mm:ss.cc
            const string paragraphWriteFormat = "{0} --> {1}{4}{2}{3}{4}";

            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT");
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string start = string.Format(timeCodeFormatNoHours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                string end = string.Format(timeCodeFormatNoHours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);

                if (p.StartTime.Hours > 0 || p.EndTime.Hours > 0)
                {
                    start = string.Format(timeCodeFormatHours, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                    end = string.Format(timeCodeFormatHours, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                }

                string style = string.Empty;
                if (!string.IsNullOrEmpty(p.Extra) && subtitle.Header == "WEBVTT")
                    style = p.Extra;
                sb.AppendLine(string.Format(paragraphWriteFormat, start, end, FormatText(p), style, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        private static string FormatText(Paragraph p)
        {
            string text = p.Text;
            while (text.Contains(Environment.NewLine + Environment.NewLine))
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            return text;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            bool textDone = true;
            foreach (string line in lines)
            {
                string s = line;
                bool isTimeCode = line.Contains("-->");
                if (isTimeCode && RegexTimeCodesMiddle.IsMatch(s))
                {
                    s = "00:" + s; // start is without hours, end is with hours
                }
                if (isTimeCode && RegexTimeCodesShort.IsMatch(s))
                {
                    s = "00:" + s.Replace("--> ", "--> 00:");
                }

                if (isTimeCode && RegexTimeCodes.IsMatch(s))
                {
                    textDone = false;
                    if (p != null)
                    {
                        subtitle.Paragraphs.Add(p);
                        p = null;
                    }
                    try
                    {
                        string[] parts = s.Replace("-->", "@").Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                        p = new Paragraph();
                        p.StartTime = GetTimeCodeFromString(parts[0]);
                        p.EndTime = GetTimeCodeFromString(parts[1]);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                        p = null;
                    }
                }
                else if (subtitle.Paragraphs.Count == 0 && line.Trim() == "WEBVTT")
                {
                    subtitle.Header = "WEBVTT";
                }
                else if (p != null && !string.IsNullOrWhiteSpace(line))
                {
                    string text = line.Trim();
                    if (!textDone)
                        p.Text = (p.Text + Environment.NewLine + text).Trim();
                }
                else if (line.Length == 0)
                {
                    textDone = true;
                }
            }
            if (p != null)
                subtitle.Paragraphs.Add(p);
            subtitle.Renumber(1);
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.Contains('<'))
                {
                    string text = p.Text;
                    text = RemoveTag("v", text);
                    text = RemoveTag("rt", text);
                    text = RemoveTag("ruby", text);
                    text = RemoveTag("c", text);
                    text = RemoveTag("span", text);
                    p.Text = text;
                }
            }
        }

        public static List<string> GetVoices(Subtitle subtitle)
        {
            var list = new List<string>();
            if (subtitle != null && subtitle.Paragraphs != null)
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
                                list.Add(voice);
                        }

                        if (startIndex == s.Length - 1)
                            startIndex = -1;
                        else
                            startIndex = s.IndexOf("<v ", startIndex + 1, StringComparison.Ordinal);
                    }
                }

            }
            return list;
        }

        public static string RemoveTag(string tag, string text)
        {
            int indexOfTag = text.IndexOf("<" + tag + " ", StringComparison.Ordinal);
            if (indexOfTag >= 0)
            {
                int indexOfEnd = text.IndexOf('>', indexOfTag);
                if (indexOfEnd > 0)
                {
                    text = text.Remove(indexOfTag, indexOfEnd - indexOfTag + 1);
                    text = text.Replace("</" + tag + ">", string.Empty);
                }

            }
            return text;
        }

        private static TimeCode GetTimeCodeFromString(string time)
        {
            // hh:mm:ss.mmm
            string[] timeCode = time.Trim().Split(':', '.', ' ');
            return new TimeCode(int.Parse(timeCode[0]),
                                int.Parse(timeCode[1]),
                                int.Parse(timeCode[2]),
                                int.Parse(timeCode[3]));
        }

    }
}
