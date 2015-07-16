﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class FabSubtitler : SubtitleFormat
    {
        private static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d  \d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "FAB Subtitler"; }
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
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:50:34:22  00:50:39:13
                //Ich muss dafür sorgen,
                //dass die Epsteins weiterleben
                sb.AppendLine(string.Format("{0}  {1}{2}{3}{2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text)));
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:50:39:13 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22  00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (regexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, regexTimeCodes.Match(line).Length);
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(13, 11);

                    string[] startParts = start.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text + Environment.NewLine + line;
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];
            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
            return tc;
        }

    }
}
