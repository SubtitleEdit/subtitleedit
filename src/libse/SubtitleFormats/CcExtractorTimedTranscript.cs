using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// CCExtractor's timed transcript output ("--out=ttxt"):
    /// 00:00:01,886|00:00:03,820|POP|e guests of Bunny Saunders.
    /// Start and end time, the CEA-608 caption mode (POP/PAINT/ROLL-UP), then the caption text.
    /// A multi-line caption is written as consecutive lines with identical time codes.
    /// </summary>
    public class CcExtractorTimedTranscript : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d[,.]\d\d\d\|\d\d:\d\d:\d\d[,.]\d\d\d\|[^|]*\|", RegexOptions.Compiled);

        public override string Extension => ".ttxt";

        public override string Name => "CCExtractor Timed Transcript";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{0}|{1}|POP|{2}";
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                foreach (var line in p.Text.SplitToLines())
                {
                    sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), line));
                }
            }

            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00},{time.Milliseconds:000}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            Paragraph last = null;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!RegexTimeCodes.IsMatch(line))
                {
                    _errorCount++;
                    continue;
                }

                var parts = line.Split('|');
                try
                {
                    var start = DecodeTimeCode(parts[0]);
                    var end = DecodeTimeCode(parts[1]);
                    var text = string.Join("|", parts, 3, parts.Length - 3).Trim();
                    if (last != null &&
                        Math.Abs(last.StartTime.TotalMilliseconds - start.TotalMilliseconds) < 0.01 &&
                        Math.Abs(last.EndTime.TotalMilliseconds - end.TotalMilliseconds) < 0.01)
                    {
                        // consecutive lines with the same time codes are one multi-line caption
                        last.Text = (last.Text + Environment.NewLine + text).Trim();
                    }
                    else
                    {
                        last = new Paragraph(start, end, text);
                        subtitle.Paragraphs.Add(last);
                    }
                }
                catch
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string s)
        {
            var parts = s.Split(':', ',', '.');
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }
    }
}
