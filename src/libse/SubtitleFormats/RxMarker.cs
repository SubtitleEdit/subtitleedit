using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class RxMarker : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^Region \d+\t\d\d:\d\d:\d\d:\d\d\.\d\d\t\d\d:\d\d:\d\d:\d\d\.\d\d\t.+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "RX marker";

        private static string MakeTimeCode(TimeCode tc)
        {
            var ts = tc.TimeSpan;
            var s = $"{ts.Days * 24 + ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{(int)Math.Round(ts.Milliseconds / 10.0, MidpointRounding.AwayFromZero):00}.00";
            return s;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Marker file version: 1");
            sb.AppendLine("Time format: Timecode");
            sb.AppendLine("Frame rate: 25,000");
            var count = 1;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var text = p.Text;
                text = HtmlUtil.RemoveHtmlTags(text, true);
                sb.AppendLine($"Region {count}\t{MakeTimeCode(p.StartTime)}\t{MakeTimeCode(p.EndTime)}\t{text.Replace(Environment.NewLine, "|")}");
                count++;
            }

            return sb.ToString();
        }

        protected static TimeCode DecodeTimeCode(string[] tokens)
        {
            if (tokens.Length != 5)
            {
                return new TimeCode();
            }

            return new TimeCode(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]) * 10 + int.Parse(tokens[4]) / 10);
        }


        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var s = line.TrimEnd();
                if (RegexTimeCode.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Replace("|", Environment.NewLine).Trim();
                            subtitle.Paragraphs.Add(p);
                        }

                        sb.Clear();
                        var arr = s.Split('\t');
                        if (arr.Length >= 3)
                        {
                            var text = s.Remove(0, arr[0].Length + arr[1].Length + arr[2].Length + 2).Trim();
                            if (string.IsNullOrWhiteSpace(text.RemoveChar('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ':', ',')))
                            {
                                _errorCount++;
                            }

                            sb.AppendLine(text);
                            char[] splitChars = { ',', '.', ':' };
                            p = new Paragraph(DecodeTimeCode(arr[1].Split(splitChars)), DecodeTimeCode(arr[2].Split(splitChars)), string.Empty);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    sb.AppendLine(s);
                }
            }

            if (p != null)
            {
                p.Text = sb.ToString().Replace("|", Environment.NewLine).Trim();
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }
    }
}
