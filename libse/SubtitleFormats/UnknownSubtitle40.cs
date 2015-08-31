﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle40 : SubtitleFormat
    {
        // 0:01 – 0:03
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d – \d+:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 40"; }
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
            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0} – {1}{2}{3}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text)));
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:50
            int seconds = time.Seconds;
            if (time.Milliseconds >= 500)
                seconds++;
            return string.Format("{0}:{1:00}", time.Hours * 60 + time.Minutes, time.Seconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            char[] splitChar = { ':' };
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string[] temp = line.Split('–');
                    string start = temp[0].Trim();
                    string end = temp[1].Trim();

                    string[] startParts = start.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 2 && endParts.Length == 2)
                    {
                        p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text.TrimEnd() + Environment.NewLine + line;
                    if (p.Text.Length > 500)
                        return;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            var minutes = int.Parse(parts[0]);
            var seconds = int.Parse(parts[1]);
            return new TimeCode(0, minutes, seconds, 0);
        }

    }
}
