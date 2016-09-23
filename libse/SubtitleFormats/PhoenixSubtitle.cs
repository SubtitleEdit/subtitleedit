using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    // Specs: http://devel.aegisub.org/wiki/SubtitleFormats/PJS
    public class PhoenixSubtitle : SubtitleFormat
    {
        //2447,   2513, "You should come to the Drama Club, too."
        //2513,   2594, "Yeah. The Drama Club is worried|that you haven't been coming."
        //2603,   2675, "I see. Sorry, I'll drop by next time."

        // TODO: Build optimized regex to match time-codes with non-whites spaces.
        private static readonly Regex RegexTimeCodes = new Regex(@"^(\d+),\s+(\d+),", RegexOptions.Compiled);
        private static readonly char[] TrimChars = { ' ', '"' };

        public override string Extension
        {
            get
            {
                return ".pjs";
            }
        }

        public override bool IsTimeBased
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "Phoenix Subtitle";
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(".pjs", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var sub = new Subtitle();
            LoadSubtitle(sub, lines, fileName);
            return sub.Paragraphs.Count > _errorCount;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            var paragraph = new Paragraph();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                Match match = null;
                if (line.Length >= 4)
                {
                    match = RegexTimeCodes.Match(line);
                }
                if (match?.Success == true)
                {
                    try
                    {
                        // Decode times.
                        paragraph.StartTime = DecodeTimeCode(match.Groups[1].Value);
                        paragraph.EndTime = DecodeTimeCode(match.Groups[2].Value);

                        // Decode text.
                        line = line.Substring(match.Value.Length).Trim();
                        // Read quote (") if there are more than one.
                        if (line.Length > 2 && line[0] == '"' && line[1] == '"')
                        {
                            line = "\"" + line.Trim(TrimChars) + "\"";
                        }
                        else
                        {
                            line = line.Trim(TrimChars);
                        }

                        paragraph.Number = i + 1;
                        paragraph.Text = string.Join(Environment.NewLine, line.Split('|'));
                        subtitle.Paragraphs.Add(paragraph);
                        paragraph = new Paragraph();
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
        }

        private static TimeCode DecodeTimeCode(string encodedTime)
        {
            // Format is timed in tenths of seconds. 2447
            int time;
            if (int.TryParse(encodedTime, out time))
            {
                return new TimeCode(time * .1 * TimeCode.BaseUnit);
            }
            return new TimeCode(0);
        }

        private static string EncodeTimeCode(TimeCode tc)
        {
            return Convert.ToString((int)Math.Round(tc.TotalMilliseconds / 100.0));
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{0},   {1},  \"{2}\"\r\n";
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true).Trim(TrimChars);
                // Pipe character for forced line breaks.
                text = text.Replace(Environment.NewLine, "|");
                sb.AppendFormat(writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text);
            }
            return sb.ToString();
        }

    }
}
