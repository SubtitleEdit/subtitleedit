using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class OtterAi : SubtitleFormat
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

            if (!HasSignature(lines))
            {
                _errorCount = lines.Count;
                return;
            }
            
            Paragraph p = null;
            var sb = new StringBuilder();
            
            // skip signature/identifier
            foreach (var line in lines.Where(line => line != Signature))
            {
                var s = line.Trim();

                var match = TryMatch(s);
                if (match.Success)
                {
                    if (p != null && sb.Length > 0)
                    {
                        p.Text = sb.ToString().Trim();
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();
                    p.StartTime = DecodeTimeCode(match.Value.Trim(), SplitCharColon);
                    if (match.Index > 0)
                    {
                        p.Actor = s.Substring(0, match.Index).Trim();
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

        private bool HasSignature(IList<string> lines)
        {
            // start checking in reverse because the signature is expected to be at last line
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var line = lines[i];
                if (line.Contains(Signature))
                {
                    return true;
                }
            }

            return false;
        }

        private Match TryMatch(string s)
        {
            var matchFormat = RegexTimeCodes1.Match(s);
            if (!matchFormat.Success)
            {
                return RegexTimeCodes2.Match(s);
            }

            return matchFormat;
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

            throw new InvalidOperationException("Too many parts for time-code. Expected 2 or 3 tokens!");
        }
    }
}
