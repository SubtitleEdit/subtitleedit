﻿using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Oresme : SubtitleFormat
    {
        //00:00:00:00{BC}{W2710}
        //10:00:00:15{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}
        //10:00:17:06{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}We view
        //10:00:19:06{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}Lufa Farms as{N}an agrotechnology business
        private static Regex regexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d\{", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Oresme"; }
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
            const string format = "{0}{1}{2}";
            sb.AppendLine("00:00:00:00{BC}{W2710}");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                const string tags = "{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}";
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                text = text.Replace(Environment.NewLine, "{N}");
                sb.AppendLine(string.Format(format, EncodeTimeCode(p.StartTime), tags, text));
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = regexTimeCodes1.Match(s);
                if (match.Success)
                {
                    var p = new Paragraph();
                    try
                    {
                        p.StartTime = DecodeTimeCode(s.Substring(0, 11));
                        p.Text = GetText(line.Remove(0, 11));
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception exception)
                    {
                        _errorCount++;
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                    }
                }
                else
                {
                    _errorCount++;
                }

                for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
                {
                    var p2 = subtitle.Paragraphs[i];
                    var next = subtitle.Paragraphs[i + 1];
                    p2.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
                if (subtitle.Paragraphs.Count > 0)
                    subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].StartTime.TotalMilliseconds +
                         Utilities.GetOptimalDisplayMilliseconds(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text);

            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber(1);
        }

        private static string GetText(string s)
        {
            s = s.Replace("{N}", Environment.NewLine);
            var sb = new StringBuilder();
            bool tagOn = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '{')
                    tagOn = true;
                else if (s[i] == '}')
                    tagOn = false;
                else if (!tagOn)
                    sb.Append(s[i]);
            }
            return sb.ToString().Trim();
        }

        private static TimeCode DecodeTimeCode(string part)
        {
            string[] parts = part.Split(new[] { '.', ':' }, StringSplitOptions.RemoveEmptyEntries);

            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}
