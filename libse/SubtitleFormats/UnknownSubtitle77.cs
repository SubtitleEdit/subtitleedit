﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle77 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d\s+->\s+\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 77"; }
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
            const string writeFormat = "{0} -> {1}{2}{3}{2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:50:34:22 -> 00:50:39:13
                //Ich muss dafür sorgen,
                //dass die Epsteins weiterleben
                sb.AppendLine(string.Format(writeFormat, p.StartTime.ToHHMMSSFF(), p.EndTime.ToHHMMSSFF(), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text, true)));
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 -> 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, RegexTimeCodes.Match(line).Length);
                    int indexOfSeparator = temp.IndexOf("->", StringComparison.Ordinal);
                    string start = temp.Substring(0, indexOfSeparator).Trim();
                    string end = temp.Substring(indexOfSeparator + 2).Trim();

                    string[] startParts = start.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
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
                        p.Text = p.Text + Environment.NewLine + line;
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            var hour = int.Parse(parts[0]);
            var minutes = int.Parse(parts[1]);
            var seconds = int.Parse(parts[2]);
            var frames = int.Parse(parts[3]);
            return new TimeCode(hour, minutes, seconds, FramesToMillisecondsMax999(frames));
        }

    }
}