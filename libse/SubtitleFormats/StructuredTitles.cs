using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class StructuredTitles : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d : \d\d:\d\d:\d\d:\d\d,\d\d:\d\d:\d\d:\d\d,\d\d", RegexOptions.Compiled);
        private static readonly Regex RegexSomeCodes = new Regex(@"^\d\d \d\d \d\d", RegexOptions.Compiled);
        private static readonly Regex RegexText = new Regex(@"^[A-Z]\d[A-Z]\d\d ", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Structured titles"; }
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
            sb.AppendLine(@"Structured titles
0000 : --:--:--:--,--:--:--:--,10
80 80 80
");

            //0001 : 01:07:25:08,01:07:29:00,10
            //80 80 80
            //C1Y00 Niemand zal je helpen ontsnappen.
            //C1Y00 - Een agent heeft me geholpen.
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0:0000} : {1},{2},10", index + 1, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
                sb.AppendLine("80 80 80");
                foreach (string line in p.Text.SplitToLines())
                    sb.AppendLine("C1Y00 " + line.Trim());
                sb.AppendLine();
                index++;
            }
            sb.AppendLine(string.Format("{0:0000}", index + 1) + @" : --:--:--:--,--:--:--:--,-1
80 80 80");
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0001 : 01:07:25:08,01:07:29:00,10
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (line.IndexOf(':') == 5 && RegexTimeCodes.IsMatch(line))
                {
                    if (p != null)
                        subtitle.Paragraphs.Add(p);

                    string start = line.Substring(7, 11);
                    string end = line.Substring(19, 11);

                    string[] startParts = start.Split(SplitCharColon);
                    string[] endParts = end.Split(SplitCharColon);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCodeFrames(startParts), DecodeTimeCodeFrames(endParts), string.Empty);
                    }
                }
                else if (p != null && RegexText.IsMatch(line))
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line.Substring(5).Trim();
                    else
                        p.Text += Environment.NewLine + line.Substring(5).Trim();
                }
                else if (line.Length < 10 && RegexSomeCodes.IsMatch(line))
                {
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (p.Text != null && Utilities.GetNumberOfLines(p.Text) > 3)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        if (!line.TrimEnd().EndsWith(": --:--:--:--,--:--:--:--,-1", StringComparison.Ordinal))
                        {
                            if (string.IsNullOrEmpty(p.Text))
                                p.Text = line.Trim();
                            else
                                p.Text += Environment.NewLine + line.Trim();
                        }
                    }
                }
            }
            if (p != null && !string.IsNullOrEmpty(p.Text))
                subtitle.Paragraphs.Add(p);

            subtitle.Renumber();
        }

    }
}
