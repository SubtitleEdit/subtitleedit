using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle88 : SubtitleFormat
    {
        private static readonly Regex RegexSpeaker = new Regex(@"\p{L}+:", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes1 = new Regex(@"^\d{1,3}:\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodes2 = new Regex(@"^\d{1,3}:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 88";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                string speaker = p.Actor;
                if (string.IsNullOrEmpty(speaker))
                {
                    speaker = "UNKNOWN";
                }

                sb.AppendLine(speaker.TrimEnd().ToUpperInvariant() + ":");
                sb.AppendLine(EncodeTimeCode(p.StartTime));
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours * 60 + time.Minutes}:{time.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            var speaker = string.Empty;
            bool isNew = true;
            foreach (string line in lines)
            {
                var s = line.TrimEnd();

                if (s.EndsWith(':') && RegexSpeaker.IsMatch(s))
                {
                    speaker = s.TrimEnd(':');
                }
                else if (!string.IsNullOrEmpty(s) && char.IsDigit(s[0]) && RegexTimeCodes1.IsMatch(s) || RegexTimeCodes2.IsMatch(s))
                {
                    p = new Paragraph(DecodeTimeCode(s), new TimeCode(), string.Empty) { Actor = speaker };
                    subtitle.Paragraphs.Add(p);
                    if (string.IsNullOrEmpty(speaker) || Utilities.RemoveNonNumbers(speaker).Length == speaker.Length)
                    {
                        _errorCount++;
                    }

                    speaker = string.Empty;
                    isNew = true;
                }
                else if (string.IsNullOrWhiteSpace(s))
                {
                    isNew = false;
                }
                else if (p != null && isNew)
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = s;
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + s;
                    }

                    if (p.Text.Length > 800)
                    {
                        _errorCount++;
                        return;
                    }
                }
            }
            foreach (var p2 in subtitle.Paragraphs)
            {
                p2.Text = Utilities.AutoBreakLine(p2.Text);
            }
            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            string[] parts = s.Split(':');
            if (parts.Length == 2)
            {
                return new TimeCode(0, int.Parse(parts[0]), int.Parse(parts[1]), 0);
            }
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), 0);
        }

    }
}
