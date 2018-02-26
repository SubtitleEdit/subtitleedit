﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// http://www.whatwg.org/specs/web-apps/current-work/webvtt.html
    /// </summary>
    public class WebVTTFileWithLineNumber : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^-?\d+:-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesMiddle = new Regex(@"^-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesShort = new Regex(@"^-?\d+:-?\d+\.-?\d+\s*-->\s*-?\d+:-?\d+\.-?\d+", RegexOptions.Compiled);

        public override string Extension => ".vtt";

        public override string Name => "WebVTT File with#";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string timeCodeFormatNoHours = "{0:00}:{1:00}.{2:000}"; // h:mm:ss.cc
            const string timeCodeFormatHours = "{0}:{1:00}:{2:00}.{3:000}"; // h:mm:ss.cc
            const string paragraphWriteFormat = "{0} --> {1}{2}{5}{3}{4}{5}";

            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT FILE");
            sb.AppendLine();
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string start = string.Format(timeCodeFormatNoHours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                string end = string.Format(timeCodeFormatNoHours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                string positionInfo = WebVTT.GetPositionInfoFromAssTag(p);

                if (p.StartTime.Hours > 0 || p.EndTime.Hours > 0)
                {
                    start = string.Format(timeCodeFormatHours, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                    end = string.Format(timeCodeFormatHours, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                }

                string style = string.Empty;
                if (!string.IsNullOrEmpty(p.Extra) && subtitle.Header == "WEBVTT FILE")
                    style = p.Extra;
                sb.Append(count);
                sb.AppendLine();
                sb.AppendLine(string.Format(paragraphWriteFormat, start, end, positionInfo, WebVTT.FormatText(p), style, Environment.NewLine));
                count++;
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            bool textDone = true;
            string positionInfo = string.Empty;
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
                        p.StartTime = WebVTT.GetTimeCodeFromString(parts[0]);
                        p.EndTime = WebVTT.GetTimeCodeFromString(parts[1]);
                        positionInfo = WebVTT.GetPositionInfo(s);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                        p = null;
                    }
                }
                else if (subtitle.Paragraphs.Count == 0 && line.Trim() == "WEBVTT FILE")
                {
                    subtitle.Header = "WEBVTT FILE";
                }
                else if (p != null && !string.IsNullOrWhiteSpace(line))
                {
                    string text = positionInfo + line.Trim();
                    if (!textDone)
                        p.Text = (p.Text + Environment.NewLine + text).Trim();
                    positionInfo = string.Empty;
                }
                else if (line.Length == 0)
                {
                    textDone = true;
                }
            }
            if (subtitle.Header == null && subtitle.Header != "WEBVTT FILE")
            {
                subtitle.Paragraphs.Clear();
                _errorCount++;
            }
            if (p != null)
                subtitle.Paragraphs.Add(p);
            subtitle.Renumber();
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            new WebVTT().RemoveNativeFormatting(subtitle, newFormat);
        }

    }
}
