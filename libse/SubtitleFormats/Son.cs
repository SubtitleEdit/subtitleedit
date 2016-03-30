﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Son : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d[\t]+\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t.+\.(tif|tiff|png|bmp|TIF|TIFF|PNG|BMP)", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".son"; }
        }

        public override string Name
        {
            get { return "SON"; }
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

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            const string writeFormat = "{0:0000}\t{1}\t{2}\t{3}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, index + 1, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "\t")));
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0001  00:00:19:13 00:00:22:10 a_0001.tif
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            int index = 0;
            Match match = null;
            foreach (string line in lines)
            {
                if (line.Length > 32)
                {
                    match = RegexTimeCodes.Match(line);
                    if (match.Success)
                    {
                        string temp = line.Substring(0, match.Length);
                        string start = temp.Substring(5, 11);
                        string end = temp.Substring(12 + 5, 11);

                        string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            int lastIndexOfTab = line.LastIndexOf('\t');
                            string text = line.Remove(0, lastIndexOfTab + 1).Trim();
                            if (!text.Contains(Environment.NewLine))
                                text = text.Replace("\t", Environment.NewLine);
                            p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Display_Area") || line.StartsWith('#') || line.StartsWith("Color") || index < 10)
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    _errorCount++;
                }
                index++;
            }
            subtitle.Renumber();
        }
    }
}
