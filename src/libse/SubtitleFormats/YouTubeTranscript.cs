using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class YouTubeTranscript : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d{1,3}:\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesHours = new Regex(@"^\d{1,2}:\d{1,3}:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "YouTube Transcript";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0}" + Environment.NewLine + "{1}", EncodeTimeCode(p.StartTime), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
            }
            return sb.ToString();
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (new UnknownSubtitle88().IsMine(lines, fileName))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            if (time.Hours > 0)
            {
                return $"{time.Hours}:{time.Minutes:00}:{time.Seconds:00}";
            }

            return $"{time.Hours * 60 + time.Minutes}:{time.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var afterTimeCode = false;
            var skippedSpokenTimeCodes = new Dictionary<Paragraph, string>();
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                var s = RemoveMarkdownBold(line.Trim());
                if (RegexTimeCodes.IsMatch(s))
                {
                    p = new Paragraph(DecodeTimeCode(s), new TimeCode(), string.Empty);
                    subtitle.Paragraphs.Add(p);
                    afterTimeCode = true;
                    continue;
                }

                if (RegexTimeCodesHours.IsMatch(s))
                {
                    p = new Paragraph(DecodeTimeCodeWithHours(s), new TimeCode(), string.Empty);
                    subtitle.Paragraphs.Add(p);
                    afterTimeCode = true;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(s))
                {
                    // skip these lines
                    continue;
                }

                var isSpokenTimeCode = afterTimeCode && p != null && IsSpokenTimeCode(s, p.StartTime);
                afterTimeCode = false;
                if (isSpokenTimeCode)
                {
                    skippedSpokenTimeCodes[p] = s;
                }
                else if (p != null)
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

            // a "spoken time code" line that is the only text was real text after all
            foreach (var kvp in skippedSpokenTimeCodes)
            {
                if (string.IsNullOrEmpty(kvp.Key.Text))
                {
                    kvp.Key.Text = kvp.Value;
                }
            }

            foreach (var p2 in subtitle.Paragraphs)
            {
                p2.Text = Utilities.AutoBreakLine(p2.Text);
            }
            subtitle.RecalculateDisplayTimes(Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
            subtitle.Renumber();
        }

        /// <summary>
        /// Transcripts copied from YouTube can have the time code wrapped in markdown bold, e.g. "**0:07**".
        /// </summary>
        private static string RemoveMarkdownBold(string s)
        {
            if (s.Length > 4 && s.StartsWith("**", StringComparison.Ordinal) && s.EndsWith("**", StringComparison.Ordinal))
            {
                return s.Substring(2, s.Length - 4).Trim();
            }

            return s;
        }

        /// <summary>
        /// Transcripts copied from YouTube can have the time code spelled out in the UI language on the line
        /// after the time code, e.g. "7 seconds" or "2 hours, 2 minutes, 14 seconds" (in any script).
        /// Such a line contains exactly the non-zero parts of the time code as numbers.
        /// </summary>
        private static bool IsSpokenTimeCode(string s, TimeCode timeCode)
        {
            if (s.Length == 0 || s.Length > 80)
            {
                return false;
            }

            var expected = new List<int>();
            foreach (var part in new[] { timeCode.Hours, timeCode.Minutes, timeCode.Seconds })
            {
                if (part != 0)
                {
                    expected.Add(part);
                }
            }

            if (expected.Count == 0)
            {
                expected.Add(0);
            }

            var numbers = new List<int>();
            var current = -1;
            foreach (var ch in s)
            {
                if (char.IsDigit(ch))
                {
                    var digit = (int)char.GetNumericValue(ch);
                    current = current < 0 ? digit : current * 10 + digit;
                    if (current > 9999)
                    {
                        return false;
                    }
                }
                else if (current >= 0)
                {
                    numbers.Add(current);
                    current = -1;
                }
            }

            if (current >= 0)
            {
                numbers.Add(current);
            }

            if (numbers.Count == 0 || numbers.Count != expected.Count)
            {
                return false;
            }

            for (var i = 0; i < numbers.Count; i++)
            {
                if (numbers[i] != expected[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            var parts = s.Split(':');

            var minutes = int.Parse(parts[0]);
            var seconds = int.Parse(parts[1]);

            return new TimeCode(0, minutes, seconds, 0);
        }

        private static TimeCode DecodeTimeCodeWithHours(string s)
        {
            var parts = s.Split(':');

            var hours = int.Parse(parts[0]);
            var minutes = int.Parse(parts[1]);
            var seconds = int.Parse(parts[2]);

            return new TimeCode(hours, minutes, seconds, 0);
        }
    }
}
