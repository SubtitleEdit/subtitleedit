﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle49 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d \d\d \d\d \d\d $", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCode2 = new Regex(@"^\d\d \d\d \d\d \d\d$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
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
            get { return "Unknown 49"; }
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
            //10 04 36 02
            //10 04 37 04
            //
            //Greetings.
            //10 04 37 06
            //10 04 40 08
            //It's confirmed, after reading
            //Not Out on the poster..
            //10 04 40 15
            //10 04 44 06
            //..you have not come to pass you
            //time, in this unique story.

            const string paragraphWriteFormat = "{0}{3}{1}{3}{2}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagFont);
                if (!text.Contains(Environment.NewLine))
                    text = Environment.NewLine + text;
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            ExpectingLine expecting = ExpectingLine.TimeStart;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (paragraph == null && expecting == ExpectingLine.TimeStart && (RegexTimeCode.IsMatch(line) || RegexTimeCode2.IsMatch(line)))
                {
                    paragraph = new Paragraph();
                }
                else if (paragraph != null && expecting == ExpectingLine.Text && (RegexTimeCode.IsMatch(line) || RegexTimeCode2.IsMatch(line)))
                {
                    if (string.IsNullOrEmpty(paragraph.Text))
                        _errorCount++;
                    if (paragraph.StartTime.TotalMilliseconds < 0.1)
                        _errorCount++;
                    subtitle.Paragraphs.Add(paragraph);
                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }

                if (paragraph != null && expecting == ExpectingLine.TimeStart && (RegexTimeCode.IsMatch(line) || RegexTimeCode2.IsMatch(line)))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                            expecting = ExpectingLine.TimeStart;
                        }
                    }
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeEnd && (RegexTimeCode.IsMatch(line) || RegexTimeCode2.IsMatch(line)))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                            expecting = ExpectingLine.TimeStart;
                        }
                    }
                }
                else
                {
                    if (paragraph != null && expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string s = line;
                            paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                    }
                }
            }
            if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
                subtitle.Paragraphs.Add(paragraph);
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00} {1:00} {2:00} {3:00} ", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
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