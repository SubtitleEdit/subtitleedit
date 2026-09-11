using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle105 : SubtitleFormat
    {
        public override string Extension => ".html";
        public override string Name => "Unknown 105";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            var seconds = 0d;
            var first = subtitle.GetParagraphOrDefault(0);
            if (first != null && first.StartTime.TotalMilliseconds > 100)
            {
                // the timeline is a chain of waits from zero - an empty wait carries the first cue's offset
                sb.AppendLine("<br>");
                sb.AppendLine($"{EncodeTime(first.StartTime.TotalSeconds)}");
            }

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];

                // The reader shows the text ACCUMULATED BEFORE a [WAIT] line for that wait's
                // duration, so the text goes first and its duration follows. Writing the wait
                // first paired every text with the next cue's duration and shifted all cues.
                sb.AppendLine(p.Text);
                sb.AppendLine($"{EncodeTime(p.DurationTotalSeconds)}");

                seconds += p.DurationTotalSeconds;
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null && (next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) > 100)
                {
                    sb.AppendLine("<br>");
                    sb.AppendLine($"{EncodeTime(next.StartTime.TotalSeconds - p.EndTime.TotalSeconds)}");
                }
            }

            sb.AppendLine();
            return sb.ToString();
        }

        private static string EncodeTime(double seconds)
        {
            return $"[WAIT&nbsp;{seconds.ToString(CultureInfo.InvariantCulture)}sec]<br>";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var allText = string.Join(Environment.NewLine, lines);
            if (!allText.Contains("[WAIT&nbsp;", StringComparison.Ordinal))
            {
                return;
            }

            var text = string.Empty;
            var milliseconds = 0d;
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("[WAIT&nbsp;",StringComparison.Ordinal))
                {
                    var ms = GetMs(line);
                    text = text.Replace("<br>", Environment.NewLine).Trim();
                    var p = new Paragraph(text, milliseconds, milliseconds + ms);
                    subtitle.Paragraphs.Add(p);
                    milliseconds += ms;
                    text = string.Empty;
                }
                else
                {
                    text += Environment.NewLine + line;
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace("<br>", Environment.NewLine).Trim();
                var p = new Paragraph(text, milliseconds, milliseconds + 3000);
                subtitle.Paragraphs.Add(p);
            }

            subtitle.RemoveEmptyLines();
        }

        private static double GetMs(string line)
        {
            var endIndex = line.IndexOf(']');
            if (endIndex == -1)
            {
                return 0;
            }

            var secondString = line.Substring(11, endIndex - 11);
            secondString = secondString.Replace("sec", string.Empty);
            if (double.TryParse(secondString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
            {
                return d * 1000.0;
            }

            return 0;
        }
    }
}
