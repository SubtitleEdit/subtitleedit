﻿using Nikse.SubtitleEdit.Core.Common;
using System;
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

        public const string NameOfFormat = "WebVTT File with#";
        public override string Name => NameOfFormat;

        public override string ToText(Subtitle subtitle, string title)
        {
            const string timeCodeFormatNoHours = "{0:00}:{1:00}.{2:000}"; // mm:ss.cc
            const string timeCodeFormatHours = "{0:00}:{1:00}:{2:00}.{3:000}"; // hh:mm:ss.cc
            const string paragraphWriteFormat = "{0} --> {1}{2}{5}{3}{4}{5}";

            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT FILE");
            sb.AppendLine();
            var count = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                var start = string.Format(timeCodeFormatNoHours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                var end = string.Format(timeCodeFormatNoHours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                var positionInfo = WebVTT.GetPositionInfoFromAssTag(p);

                if (p.StartTime.Hours > 0 || p.EndTime.Hours > 0)
                {
                    start = string.Format(timeCodeFormatHours, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds);
                    end = string.Format(timeCodeFormatHours, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds);
                }

                var style = string.Empty;
                if (!string.IsNullOrEmpty(p.Extra) && subtitle.Header == "WEBVTT FILE")
                {
                    style = p.Extra;
                }

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
            var positionInfo = string.Empty;
            var hadEmptyLine = false;
            var numbers = 0;
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var next = string.Empty;
                var isNextTimeCode = false;
                if (index < lines.Count - 1)
                {
                    next = lines[index + 1];
                    isNextTimeCode = next.Contains("-->");
                }

                var s = line;
                var isTimeCode = line.Contains("-->");
                if (isTimeCode && RegexTimeCodesMiddle.IsMatch(s))
                {
                    s = "00:" + s; // start is without hours, end is with hours
                }

                if (isTimeCode && RegexTimeCodesShort.IsMatch(s))
                {
                    s = "00:" + s.Replace("--> ", "--> 00:");
                }

                if (isNextTimeCode && Utilities.IsNumber(s))
                {
                    numbers++;
                }

                if (isNextTimeCode && Utilities.IsNumber(s) && p?.Text.Length > 0)
                {
                    // skip number
                }
                else if (isTimeCode && RegexTimeCodes.IsMatch(s))
                {
                    if (p != null)
                    {
                        p.Text = p.Text.TrimEnd();
                        subtitle.Paragraphs.Add(p);
                    }

                    try
                    {
                        var parts = s.Replace("-->", "@").Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                        p = new Paragraph
                        {
                            StartTime = WebVTT.GetTimeCodeFromString(parts[0]),
                            EndTime = WebVTT.GetTimeCodeFromString(parts[1])
                        };
                        positionInfo = WebVTT.GetPositionInfo(s);
                        p.Extra = WebVTT.GetPositionInfoRaw(s);
                        p.Region = WebVTT.GetRegion(s);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                        p = null;
                    }
                    hadEmptyLine = false;
                }
                else if (subtitle.Paragraphs.Count == 0 && line.Trim() == "WEBVTT FILE")
                {
                    subtitle.Header = "WEBVTT FILE";
                }
                else if (p != null && hadEmptyLine && Utilities.IsInteger(line.RemoveChar('-')) &&
                         (RegexTimeCodesMiddle.IsMatch(next) ||
                          RegexTimeCodesShort.IsMatch(next) ||
                          RegexTimeCodes.IsMatch(next)))
                {
                    // line number
                }
                else if (p != null)
                {
                    var text = positionInfo + line.Trim();
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

            if (subtitle.Header == null && subtitle.Header != "WEBVTT FILE")
            {
                subtitle.Paragraphs.Clear();
                _errorCount++;
            }
            if (p != null)
            {
                p.Text = p.Text.TrimEnd();
                subtitle.Paragraphs.Add(p);
            }

            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = WebVTT.ColorWebVttToHtml(paragraph.Text);
                paragraph.Text = System.Net.WebUtility.HtmlDecode(paragraph.Text);
            }

            if (numbers == 0 || numbers < subtitle.Paragraphs.Count / 2 && !new WebVTT().IsMine(lines, fileName))
            {
                _errorCount = subtitle.Paragraphs.Count + 1;
            }

            subtitle.Renumber();
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            new WebVTT().RemoveNativeFormatting(subtitle, newFormat);
        }
    }
}
