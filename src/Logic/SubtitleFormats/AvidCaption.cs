using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class AvidCaption : SubtitleFormat
    {
        private static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Avid Caption"; }
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
            StringBuilder sb = new StringBuilder();
            int index = 0;
            sb.AppendLine("@ This file written with the Avid Caption plugin, version 1");
            sb.AppendLine();
            sb.AppendLine("<begin subtitles>");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0} {1}{2}{3}{2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text, true)));
                //00:50:34:22 00:50:39:13
                //Ich muss dafür sorgen,
                //dass die Epsteins weiterleben
                index++;
            }
            sb.AppendLine("<end subtitles>");
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:50:39:13 (last is frame)
            int frames = MillisecondsToFramesMaxFrameRate(time.Milliseconds);
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, frames);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22  00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            bool beginFound = false;
            bool endFound = false;
            foreach (string line in lines)
            {
                string tline = line.Trim();
                if (tline.Equals("<begin subtitles>", StringComparison.OrdinalIgnoreCase))
                {
                    beginFound = true;
                }
                else if (tline.Equals("<end subtitles>", StringComparison.OrdinalIgnoreCase))
                {
                    endFound = true;
                    break;
                }

                if (line.IndexOf(':') == 2 && regexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, regexTimeCodes.Match(line).Length);
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (tline.Length == 0 || tline[0] == '@')
                {
                    // skip these lines
                }
                else if (tline.Length > 0 && p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text.TrimEnd() + Environment.NewLine + line;
                }
            }
            if (!beginFound)
                _errorCount++;
            if (!endFound)
                _errorCount++;

            subtitle.Renumber(1);
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];
            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}