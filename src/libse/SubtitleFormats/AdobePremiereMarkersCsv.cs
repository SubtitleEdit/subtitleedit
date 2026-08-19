using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Adobe Premiere Pro's Markers panel export ("Export Markers as CSV"): a tab-separated
    /// file despite the .csv extension, with a fixed header. Point markers have Out equal to
    /// In and a zero Duration. Time codes are HH:MM:SS:FF, or HH;MM;SS;FF for drop-frame.
    /// </summary>
    public class AdobePremiereMarkersCsv : SubtitleFormat
    {
        private const string HeaderStart = "Marker Name\tDescription\tIn\tOut\tDuration";

        public override string Extension => ".csv";

        public override string Name => "Adobe Premiere Markers";

        public override bool IsMine(List<string> lines, string fileName)
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                return line.StartsWith(HeaderStart, StringComparison.Ordinal);
            }

            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Marker Name\tDescription\tIn\tOut\tDuration\tMarker Type");
            foreach (var p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, " ");
                sb.AppendLine($"{text}\t{text}\t{EncodeTimeCode(p.StartTime)}\t{EncodeTimeCode(p.EndTime)}\t{EncodeTimeCode(new TimeCode(p.DurationTotalMilliseconds))}\tComment");
            }

            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var headerSeen = false;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!headerSeen)
                {
                    headerSeen = true;
                    if (line.StartsWith(HeaderStart, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    _errorCount++;
                    return;
                }

                var arr = line.Split('\t');
                if (arr.Length < 4)
                {
                    _errorCount++;
                    continue;
                }

                if (!TryDecodeTimeCode(arr[2], out var start))
                {
                    _errorCount++;
                    continue;
                }

                var end = TryDecodeTimeCode(arr[3], out var endTime) ? endTime : start;
                if (end.TotalMilliseconds <= start.TotalMilliseconds)
                {
                    // Point marker (Out == In, zero duration) - give it a usable duration
                    end = new TimeCode(start.TotalMilliseconds + 1000);
                }

                var name = arr[0].Trim();
                var description = arr[1].Trim();
                var text = string.IsNullOrEmpty(description) ? name : description;
                subtitle.Paragraphs.Add(new Paragraph(text, start.TotalMilliseconds, end.TotalMilliseconds));
            }

            subtitle.Renumber();
        }

        private static bool TryDecodeTimeCode(string input, out TimeCode timeCode)
        {
            timeCode = new TimeCode();
            var parts = input.Trim().Replace(';', ':').Split(':');
            if (parts.Length != 4 ||
                !int.TryParse(parts[0], out var hours) ||
                !int.TryParse(parts[1], out var minutes) ||
                !int.TryParse(parts[2], out var seconds) ||
                !int.TryParse(parts[3], out var frames))
            {
                return false;
            }

            timeCode = new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
            return true;
        }
    }
}
