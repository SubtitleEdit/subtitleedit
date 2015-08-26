using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class PE2 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d:\d\d:\d\d:\d\d ", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodeEnd = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
            TimeStart,
            Text,
            TimeEndOrText,
        }

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "PE2"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            string s = sb.ToString();
            if (s.Contains("[HEADER]") && s.Contains("[BODY]"))
                return false; // UnknownSubtitle17

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //#PE2 Format file
            //10:00:05:16 You will get a loan of//Rs 1.5 million in 15 minutes.
            //10:00:08:19
            //10:00:09:01 What have you brought//as the guarantee?
            //10:00:12:01
            //10:00:12:11 What?//I didn't get you.
            //10:00:14:11
            //10:00:14:15 We will sanction your loan.
            //10:00:16:00

            const string writeFormat = "{0} {2}{3}{1}";
            var sb = new StringBuilder();
            sb.AppendLine("#PE2 Format file");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace(Environment.NewLine, "//");
                sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            ExpectingLine expecting = ExpectingLine.TimeStart;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            char[] splitChar = { ':' };
            foreach (string line in lines)
            {
                if (RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Substring(0, 11).Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCode(parts);
                            if (expecting == ExpectingLine.TimeStart)
                            {
                                paragraph = new Paragraph();
                                paragraph.StartTime = tc;
                                expecting = ExpectingLine.Text;
                                if (line.Length > 12)
                                    paragraph.Text = line.Substring(12).Trim().Replace("//", Environment.NewLine);
                            }
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.TimeStart;
                        }
                    }
                }
                else if (RegexTimeCodeEnd.IsMatch(line))
                {
                    string[] parts = line.Substring(0, 11).Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        var tc = DecodeTimeCode(parts);
                        paragraph.EndTime = tc;
                        subtitle.Paragraphs.Add(paragraph);
                        if (paragraph.StartTime.TotalMilliseconds < 0.001)
                            _errorCount++;
                        paragraph = new Paragraph();
                        expecting = ExpectingLine.TimeStart;
                    }
                }
                else
                {
                    if (expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string text = line.Replace("//", Environment.NewLine);
                            paragraph.Text += Environment.NewLine + text;
                            expecting = ExpectingLine.TimeEndOrText;

                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                return;
                            }
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(line) && line != "#PE2 Format file")
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            var hour = int.Parse(parts[0]);
            var minutes = int.Parse(parts[1]);
            var seconds = int.Parse(parts[2]);
            var frames = int.Parse(parts[3]);

            return new TimeCode(hour, minutes, seconds, FramesToMillisecondsMax999(frames));
        }

    }
}