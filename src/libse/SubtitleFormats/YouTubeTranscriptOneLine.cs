using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class YouTubeTranscriptOneLine : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d{1,3}:\d\d.+$", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesHours = new Regex(@"^\d{1,2}:\d{1,3}:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "YouTube Transcript one line";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            const string writeFormat = "{0} {1}";
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
            }
            return sb.ToString();
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
            subtitle.Paragraphs.Clear();
            char[] trimChars = { '–', '.', ';', ':' };
            foreach (var line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    var splitter = line.IndexOf(':') + 3;
                    var text = line.Remove(0, splitter);
                    var p = new Paragraph(DecodeTimeCode(line.Substring(0, splitter)), new TimeCode(), text);
                    subtitle.Paragraphs.Add(p);
                    text = text.Trim().Trim(trimChars).Trim();
                    if (text.Length > 0 && char.IsDigit(text[0]))
                    {
                        _errorCount++;
                    }
                }
                else if (RegexTimeCodesHours.IsMatch(line))
                {
                    var matchHours = RegexTimeCodesHours.Match(line);
                    var text = line.Remove(0, matchHours.Length);
                    var p = new Paragraph(DecodeTimeCodeHours(line.Substring(0, matchHours.Length)), new TimeCode(), text);
                    subtitle.Paragraphs.Add(p);
                    text = text.Trim().Trim(trimChars).Trim();
                    if (text.Length > 0 && char.IsDigit(text[0]))
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount += 2;
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
            var parts = s.Split(':');

            var minutes = parts[0];
            var seconds = parts[1];

            return new TimeCode(0, int.Parse(minutes), int.Parse(seconds), 0);
        }

        private static TimeCode DecodeTimeCodeHours(string s)
        {
            var parts = s.Split(':');

            var hours = parts[0];
            var minutes = parts[1];
            var seconds = parts[2];

            return new TimeCode(int.Parse(hours), int.Parse(minutes), int.Parse(seconds), 0);
        }
    }
}
