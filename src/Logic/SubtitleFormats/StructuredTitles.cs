using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class StructuredTitles : SubtitleFormat
    {
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

            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            StringBuilder sb = new StringBuilder();
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
                foreach (string line in p.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    sb.AppendLine("C1Y00 " + line.Trim());
                sb.AppendLine();
                index++;
            }
            sb.AppendLine(string.Format("{0:0000}", index+1) + @" : --:--:--:--,--:--:--:--,-1
80 80 80");
            return sb.ToString();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0001 : 01:07:25:08,01:07:29:00,10
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            var regexTimeCodes = new Regex(@"^\d\d\d\d : \d\d:\d\d:\d\d:\d\d,\d\d:\d\d:\d\d:\d\d,\d\d", RegexOptions.Compiled);
            var regexSomeCodes = new Regex(@"^\d\d \d\d \d\d", RegexOptions.Compiled);
            var regexText = new Regex(@"^[A-Z]\d[A-Z]\d\d ", RegexOptions.Compiled);
            foreach (string line in lines)
            {
                if (regexTimeCodes.IsMatch(line))
                {
                    if (p != null)
                        subtitle.Paragraphs.Add(p);

                    string start = line.Substring(7, 11);
                    string end = line.Substring(19, 11);

                    string[] startParts = start.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), string.Empty);
                    }
                }
                else if (p != null && regexText.IsMatch(line))
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line.Substring(5).Trim();
                    else
                        p.Text +=  Environment.NewLine + line.Substring(5).Trim();
                }
                else if (regexSomeCodes.IsMatch(line))
                {
                }
                else if (line.Trim().Length == 0)
                {
                    // skip these lines
                }
                else if (line.Trim().Length > 0 && p != null)
                {
                    if (p.Text != null && Utilities.CountTagInText(p.Text, Environment.NewLine) > 2)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        if (!line.Trim().EndsWith(": --:--:--:--,--:--:--:--,-1"))
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

            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
            return tc;
        }

    }
}

