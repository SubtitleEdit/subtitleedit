using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Footage : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\s*\d+,\d\d$", RegexOptions.Compiled);

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
            get { return "Footage"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var asc = new TimeLineFootageAscii();
            if (fileName != null && asc.IsMine(null, fileName))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //1.
            //   66,13
            //   70,00
            //#Tā nu es sapazinos
            //#И так я познакомился

            //2.
            //   71,14
            //   78,10
            //#ar dakteri Henriju Gūsu.
            //#с доктором Генри Гусом.

            const string paragraphWriteFormat = "{4}.{3}{0}{3}{1}{3}{2}{3}";
            var sb = new StringBuilder();
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                if (p.Text.StartsWith("<i>") && p.Text.EndsWith("</i>"))
                    text = "#" + text;
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
                if (line.EndsWith('.') && Utilities.IsInteger(line.TrimEnd('.')))
                {
                    if (paragraph != null && !string.IsNullOrEmpty(paragraph.Text))
                        subtitle.Paragraphs.Add(paragraph);
                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeStart;
                }
                else if (paragraph != null && expecting == ExpectingLine.TimeStart && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
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
                else if (paragraph != null && expecting == ExpectingLine.TimeEnd && RegexTimeCode.IsMatch(line))
                {
                    string[] parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
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
                    if (paragraph != null && expecting == ExpectingLine.Text)
                    {
                        if (line.Length > 0)
                        {
                            string s = line.Trim();
                            if (s.StartsWith('#'))
                                s = "<i>" + s.Remove(0, 1) + "</i>";
                            paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                            paragraph.Text = paragraph.Text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
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
            int frames = MillisecondsToFrames(time.TotalMilliseconds);
            int footage = frames / 16;
            int rest = (int)((frames % 16) / 16.0 * 24.0);
            return string.Format("{0:00},{1:00}", footage, rest).PadLeft(8);
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            int frames16 = int.Parse(parts[0]);
            int frames = int.Parse(parts[1]);
            return new TimeCode(0, 0, 0, FramesToMilliseconds(16 * frames16 + frames));
        }

    }
}
