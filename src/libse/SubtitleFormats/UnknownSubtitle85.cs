using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle85 : SubtitleFormat
    {

        private enum ExpectingLine
        {
            Number,
            TimeAndText
        }

        //00:02:03 e simbolizam a relação espiritual between \Nentre o Imam e os seus murids.
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d ", RegexOptions.Compiled);
        private static readonly Regex NotRegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d \d\d:\d\d:\d\d", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 85";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ISUBTITLES MASTER");
            sb.AppendLine();
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                sb.AppendLine((i + 1).ToString(CultureInfo.InvariantCulture));
                Paragraph p = subtitle.Paragraphs[i];
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)} {HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "\\N"), true)}");
                sb.AppendLine();
            }
            sb.AppendLine("[END]");
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode ts)
        {
            //00:03:15 (last is SECONDS)
            ts = new TimeCode(ts.TotalMilliseconds);
            if (ts.Milliseconds >= 500)
            {
                ts.TotalMilliseconds += TimeCode.BaseUnit;
            }

            var s = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

            if (ts.TotalMilliseconds >= 0)
            {
                return s;
            }

            return "-" + s.RemoveChar('-');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //1
            //00:06:31 including members of the Ismaili Leaders’ \NInternational Forum, or LIF,
            //
            //2
            //00:06:36 que é composto pelos Presidentes dos 20 Conselhos Nacionais
            Paragraph p = null;
            _errorCount = 0;
            var expecting = ExpectingLine.Number;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (expecting == ExpectingLine.Number)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        if (long.TryParse(line, out long l))
                        {
                            expecting = ExpectingLine.TimeAndText;
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (expecting == ExpectingLine.TimeAndText)
                {
                    Match match = RegexTimeCodes.Match(line);
                    if (match.Success && !NotRegexTimeCodes.IsMatch(line))
                    {
                        try
                        {
                            string start = line.Substring(0, match.Value.Length);
                            string text = line.Remove(0, match.Value.Length).Trim().Replace("\\N", Environment.NewLine);
                            p = new Paragraph(DecodeTimeCode(start.Split(':')), TimeCode.FromSeconds(0), text);
                            subtitle.Paragraphs.Add(p);
                            expecting = ExpectingLine.Number;
                        }
                        catch
                        {
                            _errorCount += 10;
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(line))
                    {
                        expecting = ExpectingLine.Number;
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                p = subtitle.Paragraphs[i];
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
                var next = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    if (next.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                        if (p.Duration.TotalMilliseconds < 0)
                        {
                            _errorCount++;
                        }
                    }
                    else if (next.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds < Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds;
                    }
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), 0);
        }

    }
}
