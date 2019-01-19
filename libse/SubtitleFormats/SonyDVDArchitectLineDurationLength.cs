using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SonyDVDArchitectLineDurationLength : SubtitleFormat
    {
        private static readonly Regex Regex = new Regex(@"^\d+\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t\d+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Sony DVDArchitect line/dur/length";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Title: " + title);
            sb.AppendLine("Translator: No Author");
            sb.AppendLine("Date: " + DateTime.Now.ToString("dd-MM-yyyy").Replace("-", ".")); //  25.08.2011
            double milliseconds = 0;
            if (subtitle.Paragraphs.Count > 0)
            {
                milliseconds = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds;
            }

            var tc = new TimeCode(milliseconds);
            sb.AppendLine(string.Format("Duration: {0:00}:{1:00}:{2:00}:{3:00}", tc.Hours, tc.Minutes, tc.Seconds, MillisecondsToFramesMaxFrameRate(tc.Milliseconds))); // 01:20:49:12
            sb.AppendLine("Program start: 00:00:00:00");
            sb.AppendLine("Title count: " + subtitle.Paragraphs.Count);
            sb.AppendLine();
            sb.AppendLine("#\tIn\tOut\tDuration\tLength");
            sb.AppendLine();
            int count = 0;
            const string writeFormat = "{14}\t{0:00}:{1:00}:{2:00}:{3:00}\t{4:00}:{5:00}:{6:00}:{7:00}\t{8:00}:{9:00}:{10:00}:{11:00}\t{12}\r\n{13}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);

                // to avoid rounding errors in duration
                var startFrame = MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds);
                var endFrame = MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds);
                var durationCalc = new Paragraph(
                        new TimeCode(p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, FramesToMillisecondsMax999(startFrame)),
                        new TimeCode(p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, FramesToMillisecondsMax999(endFrame)),
                        string.Empty);

                sb.AppendLine(string.Format(writeFormat + Environment.NewLine,
                                            p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, startFrame,
                                            p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, endFrame,
                                            durationCalc.Duration.Hours, durationCalc.Duration.Minutes, durationCalc.Duration.Seconds, MillisecondsToFramesMaxFrameRate(durationCalc.Duration.Milliseconds),
                                            text.Length, text, count));
            }
            return sb.ToString().Trim() + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {   //22    00:04:19:12 00:04:21:09 00:00:01:21 14
            _errorCount = 0;
            Paragraph lastParagraph = null;
            int count = 0;
            foreach (string line in lines)
            {
                bool isTimeCode = false;
                if (line.Length > 0)
                {
                    bool success = false;
                    if (line.Length > 33 && line.IndexOf(':') > 1)
                    {
                        var match = Regex.Match(line);
                        if (match.Success)
                        {
                            isTimeCode = true;
                            if (lastParagraph != null)
                            {
                                subtitle.Paragraphs.Add(lastParagraph);
                            }

                            var arr = line.Split('\t');
                            TimeCode start = DecodeTimeCodeFrames(arr[1], SplitCharColon);
                            TimeCode end = DecodeTimeCodeFrames(arr[2], SplitCharColon);
                            lastParagraph = new Paragraph(start, end, string.Empty);
                            success = true;
                        }
                    }
                    if (!isTimeCode && !string.IsNullOrWhiteSpace(line) && lastParagraph != null && Utilities.GetNumberOfLines(lastParagraph.Text) < 5)
                    {
                        lastParagraph.Text = (lastParagraph.Text + Environment.NewLine + line).Trim();
                        success = true;
                    }
                    if (!success && count > 9)
                    {
                        _errorCount++;
                    }
                }
                count++;
            }
            if (lastParagraph != null)
            {
                subtitle.Paragraphs.Add(lastParagraph);
            }

            subtitle.Renumber();
        }

    }
}
