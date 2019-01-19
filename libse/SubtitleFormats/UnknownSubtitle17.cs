using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle17 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d:\d\d:\d\d:\d\d", RegexOptions.Compiled);
        private static readonly Regex RegexNumber = new Regex(@"^\[\d+\]$", RegexOptions.Compiled);

        private enum ExpectingLine
        {
            Number,
            TimeStart,
            TimeEnd,
            Text
        }

        public override string Extension => ".txt";

        public override string Name => "Unknown 17";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string s = sb.ToString();
            if (!s.Contains("[HEADER]") || !s.Contains("[BODY]"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //[1]
            //01:00:21:20
            //01:00:23:17
            //[I]Pysy kanavalla,
            //[I]koska myöhemmin tänään

            const string paragraphWriteFormat = "[{4}]{3}{0}{3}{1}{3}{2}";
            var sb = new StringBuilder();
            sb.AppendLine(@"[HEADER]
SUBTITLING_COMPANY=Softitler Net, Inc.
TIME_FORMAT=NTSC
CLIENT=UNIVERSAL
LANGUAGE=Finnish
DATE=5/28/2007
JOB_ID=89972
JOB_TYPE=Feature
TITLE=Notting Hill
SUBNAME=BD TRTL
YEAR=1999
DIGITAL_CINEMA=YES
[/HEADER]
[BODY]");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;
                string text = p.Text.Replace("<i>", "[I]").Replace("</i>", "[/I]");
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text, Environment.NewLine, count));
            }
            sb.AppendLine("[/BODY]");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            var expecting = ExpectingLine.Number;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string l in lines)
            {
                string line = l.Replace("[L]", string.Empty).Replace("[N]", string.Empty).TrimEnd();
                if (RegexNumber.IsMatch(line))
                {
                    if (paragraph != null)
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }

                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeStart && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCodeFramesFourParts(parts);
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
                else if (paragraph != null && expecting == ExpectingLine.TimeEnd && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var tc = DecodeTimeCodeFramesFourParts(parts);
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
                    if (paragraph != null && expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string s = line;
                            if (line.StartsWith("[i]", StringComparison.OrdinalIgnoreCase))
                            {
                                s = "<i>" + s.Remove(0, 3);
                                if (s.EndsWith("[/i]", StringComparison.OrdinalIgnoreCase))
                                {
                                    s = s.Remove(s.Length - 4, 4);
                                }

                                s += "</i>";
                            }
                            s = s.Replace("[I]", "<i>");
                            s = s.Replace("[/I]", "</i>");
                            s = s.Replace("[P]", string.Empty);
                            s = s.Replace("[/P]", string.Empty);
                            s = s.Replace("\0", string.Empty);
                            paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                            if (paragraph.Text.Length > 2000)
                            {
                                _errorCount += 100;
                                subtitle.Renumber();
                                return;
                            }
                        }
                    }
                }
            }
            if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
            {
                paragraph.Text = paragraph.Text.Replace("[/BODY]", string.Empty).Trim();
                subtitle.Paragraphs.Add(paragraph);
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }
    }
}
