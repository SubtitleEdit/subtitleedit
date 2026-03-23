using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Speechmatics : SubtitleFormat
    {
        public override string Extension => ".txt";

        public override string Name => "Speechmatics";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append("<time=" + p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) + ">");
                sb.Append(p.Text);
                sb.Append("<time=" + p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) + ">");
                sb.Append(" ");
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            var allText = sb.ToString();

            var startTimeIdx = allText.IndexOf("<time=", StringComparison.Ordinal);
            while (startTimeIdx >= 0)
            {
                var endTimeIndex = allText.IndexOf('>', startTimeIdx);
                if (endTimeIndex < 0)
                {
                    break;
                }

                var startTime = allText.Substring(startTimeIdx + 6, endTimeIndex - startTimeIdx - 6).Trim();

                startTimeIdx = allText.IndexOf("<time=", endTimeIndex, StringComparison.Ordinal);
                if (startTimeIdx < 0)
                {
                    break;
                }

                var text = allText.Substring(endTimeIndex + 1, startTimeIdx - endTimeIndex -1).Trim();

                endTimeIndex = allText.IndexOf('>', startTimeIdx);
                if (endTimeIndex < 0)
                {
                    break;
                }

                var endTime = allText.Substring(startTimeIdx + 6, endTimeIndex - startTimeIdx - 6).Trim();

                if (double.TryParse(startTime, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) &&
                    double.TryParse(endTime, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
                {
                    subtitle.Paragraphs.Add(new Paragraph(text, startSeconds * 1000.0, endSeconds * 1000.0));
                }
                else
                {
                    _errorCount++;
                }

                startTimeIdx = allText.IndexOf("<time=", endTimeIndex + 1, StringComparison.Ordinal);
            }

            subtitle.Renumber();
        }
    }
}
