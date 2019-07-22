﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class OtterAi : TextFormat
    {
        private static readonly Regex RegexTimeCodes1 = new Regex(@" \d{1,2}:\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes2 = new Regex(@" \d{1,2}:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Otter AI transcription";

        private const string Signature = "Transcribed by https://otter.ai";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append(string.IsNullOrWhiteSpace(p.Actor) ? "Speaker" : p.Actor);
                sb.AppendLine("  " + EncodeTime(p.StartTime));
                sb.AppendLine(p.Text);
                sb.AppendLine();
            }
            sb.AppendLine(Signature);
            return sb.ToString();
        }

        private static string EncodeTime(TimeCode timeCode)
        {
            var ts = timeCode.TimeSpan;
            string s;
            if (ts.Minutes == 0 && ts.Hours == 0 && ts.Days == 0)
            {
                s = string.Format("0:{0:00}", ts.Seconds);
            }
            else if (ts.Hours == 0 && ts.Days == 0)
            {
                s = string.Format("{0:0}:{1:00}", ts.Minutes, ts.Seconds);
            }
            else
            {
                s = string.Format("{0:0}:{1:00}:{2:00}", ts.Hours + ts.Days * 24, ts.Minutes, ts.Seconds);
            }

            if (timeCode.TotalMilliseconds >= 0)
            {
                return s;
            }

            return "-" + s.RemoveChar('-');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            bool foundTranscribedTag = false;
            foreach (string line in lines)
            {
                if (line.Contains(Signature))
                {
                    foundTranscribedTag = true;
                    break;
                }
            }
            if (!foundTranscribedTag)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match1 = RegexTimeCodes1.Match(s);
                var match2 = RegexTimeCodes2.Match(s);
                if (s == Signature)
                {
                    // Skip
                }
                else if (match1.Success || match2.Success)
                {
                    var m = match1.Success ? match1 : match2;
                    if (p != null && sb.Length > 0)
                    {
                        p.Text = sb.ToString().Trim();
                        subtitle.Paragraphs.Add(p);
                    }
                    p = new Paragraph();
                    p.StartTime = DecodeTimeCode(m.Value.Trim(), SplitCharColon);
                    if (m.Index > 0)
                    {
                        p.Actor = s.Substring(0, m.Index).Trim();
                    }
                    sb.Clear();
                }
                else if (p != null)
                {
                    sb.AppendLine(s);
                }
            }
            if (p != null && sb.Length > 0)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds,
                                            null,
                                            Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string s, char[] splitCharColon)
        {
            var arr = s.Split(splitCharColon);
            if (arr.Length == 2)
            {
                return new TimeCode(0, int.Parse(arr[0]), int.Parse(arr[1]), 0);
            }
            else if (arr.Length == 3)
            {
                return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), 0);
            }
            throw new InvalidOperationException();
        }
    }
}
