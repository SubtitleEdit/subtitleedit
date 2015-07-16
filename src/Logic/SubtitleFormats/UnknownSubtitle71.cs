﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle71 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCode = new Regex(@"^ \d \d : \d \d : \d \d : \d \d $", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCode2 = new Regex(@"^\d \d : \d \d : \d \d : \d \d$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
            Number,
            TimeStart,
            TimeEnd,
            Text
        }

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 71"; }
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

        private static string AddSpaces(string text)
        {
            if (string.IsNullOrEmpty(text))
                return " ";

            var sb = new StringBuilder(@" ");
            for (int i = 0; i < text.Length; i++)
            {
                sb.Append(text[i]);
                sb.Append(' ');
            }
            return sb.ToString();
        }

        private static string RemoveSpaces(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = text.Trim();
            for (int i = 0; i < text.Length; i++)
            {
                if (i % 2 == 1 && text[i] != ' ')
                    return text;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (i % 2 == 0)
                    sb.Append(text[i]);
            }
            return sb.ToString();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            // 1 .
            // 0 0 : 0 0 : 0 4 : 1 2
            // 0 0 : 0 0 : 0 6 : 0 5
            // H a l l o !

            // 2.
            // 0 0 : 0 0 : 0 6 : 1 6
            // 0 0 : 0 0 : 0 7 : 2 0
            // G e t   i n s i d e ,   m o m !
            // -   I   w a n t   t o   c o m e .

            const string paragraphWriteFormat = "{4} . {3}{3}{0}{3}{3}{1}{3}{3}{2}{3}{3}";
            var sb = new StringBuilder();
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;
                var text = AddSpaces(HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagFont));
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine, count));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            ExpectingLine expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (expecting == ExpectingLine.Number && (RegexTimeCode.IsMatch(line) || RegexTimeCode2.IsMatch(line.Trim())))
                {
                    _errorCount++;
                    if (paragraph != null)
                        subtitle.Paragraphs.Add(paragraph);
                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }

                if (line.TrimEnd().EndsWith('.') && Utilities.IsInteger(RemoveSpaces(line.Trim().TrimEnd('.').Trim())))
                {
                    if (paragraph != null)
                        subtitle.Paragraphs.Add(paragraph);
                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeStart && (RegexTimeCode.IsMatch(line) || RegexTimeCode2.IsMatch(line.Trim())))
                {
                    string[] parts = RemoveSpaces(line.Trim()).Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.StartTime = tc;
                            expecting = ExpectingLine.TimeEnd;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeEnd && (RegexTimeCode.IsMatch(line) || RegexTimeCode2.IsMatch(line.Trim())))
                {
                    string[] parts = RemoveSpaces(line.Trim()).Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            paragraph.EndTime = tc;
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.Number;
                        }
                    }
                }
                else
                {
                    if (line == " " || line.Trim() == @"...........\...........")
                    {
                    }
                    else if (line == "*END*")
                    {
                        _errorCount++;
                        if (paragraph != null)
                            subtitle.Paragraphs.Add(paragraph);
                        paragraph = new Paragraph();
                        expecting = ExpectingLine.Number;
                    }
                    else if (paragraph != null && expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string s = RemoveSpaces(line);
                            paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                    }
                    else if (line.Length > 1)
                    {
                        _errorCount++;
                    }
                }
            }
            if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
                subtitle.Paragraphs.Add(paragraph);
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            string s = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
            return AddSpaces(s);
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}