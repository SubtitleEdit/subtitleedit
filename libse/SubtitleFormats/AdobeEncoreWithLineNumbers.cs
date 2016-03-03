﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeEncoreWithLineNumbers : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+ \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d ", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Adobe Encore w. line#"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            if (sb.ToString().Contains("#INPOINT OUTPOINT PATH"))
                return false; // Pinnacle Impression
            if (sb.ToString().StartsWith("{{\\rtf1", StringComparison.Ordinal))
                return false;

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 1;
            const string writeFormat = "{0} {1} {2} {3}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:03:15:22 00:03:23:10 This is line one.
                //This is line two.
                sb.AppendLine(string.Format(writeFormat, index, p.StartTime.ToHHMMSSFF(), p.EndTime.ToHHMMSSFF(), HtmlUtil.RemoveHtmlTags(p.Text, true)));
                index++;
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            Match match = null;
            foreach (string line in lines)
            {
                if (line.Length >= 25 && (match = RegexTimeCodes.Match(line)).Success)
                {
                    string temp = line.Substring(line.IndexOf(' ')).Trim();
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        string text = line.Remove(0, match.Length - 1).Trim();
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && !string.IsNullOrWhiteSpace(line))
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text + Environment.NewLine + line;
                }
            }

            subtitle.Renumber();
        }

    }
}
