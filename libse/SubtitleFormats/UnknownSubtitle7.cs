﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Reported by dipa nuswantara
    /// </summary>
    public class UnknownSubtitle7 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeStart,
            Text,
            TimeEnd,
        }

        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d:\d\d:\d\d:\d\d", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 7"; }
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
            //00:00:54:16 Bisakah kalian diam,Tolong!
            //00:00:56:07
            //00:00:57:16 Benar, tepatnya saya tidak memiliki "Anda
            //sudah mendapat 24 jam" adegan... tapi
            //00:01:02:03

            const string paragraphWriteFormat = "{0} {2}{3}{1}\t";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, p.StartTime.ToHHMMSSFF(), p.EndTime.ToHHMMSSFF(), p.Text, Environment.NewLine));
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            var expecting = ExpectingLine.TimeStart;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            int index = 0;
            foreach (string line in lines)
            {
                if (expecting == ExpectingLine.TimeStart && line.Length >= 12 && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Substring(0, 11).Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (expecting == ExpectingLine.TimeStart)
                    {
                        paragraph = new Paragraph();
                        paragraph.StartTime = DecodeTimeCodeFramesFourParts(parts);
                        if (line.Length > 12)
                        {
                            paragraph.Text = line.Substring(12).Trim();
                        }
                        // Look if next-line is Text/TimeCode.
                        if (index + 1 < lines.Count && RegexTimeCode.IsMatch(lines[index + 1].TrimStart()))
                        {
                            expecting = ExpectingLine.TimeEnd;
                        }
                        else
                        {
                            expecting = ExpectingLine.Text;
                        }
                    }
                }
                else if (expecting == ExpectingLine.TimeEnd)
                {
                    string[] parts = line.Substring(0, 11).Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    paragraph.EndTime = DecodeTimeCodeFramesFourParts(parts);
                    subtitle.Paragraphs.Add(paragraph);
                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }
                else if (expecting == ExpectingLine.Text)
                {
                    if (line.Length > 0)
                    {
                        string text = line.Replace("|", Environment.NewLine);
                        paragraph.Text = (paragraph.Text.Length > 0) ? Environment.NewLine + text : text;
                        expecting = ExpectingLine.TimeEnd;
                        if (paragraph.Text.Length > 2000)
                        {
                            _errorCount += 100;
                            return;
                        }
                    }
                }
                else
                {
                    _errorCount++;
                }

                index++;
            }
            subtitle.Renumber();
        }

    }
}