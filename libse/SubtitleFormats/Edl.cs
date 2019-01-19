using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Edl : SubtitleFormat
    {
        private static readonly Regex Regex = new Regex(@"^\d+\s+[A-Z]{2}\s+[A-Z]\s+[A-Z]\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);
        private const string TextPrefix = "* FROM CLIP NAME: ";

        public override string Extension => ".edl";

        public override string Name => "EDL";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TITLE: " + title);
            if (Configuration.Settings.General.CurrentFrameRate % 1.0 > 0.01)
            {
                sb.AppendLine("FCM: NON-DROP FRAME");
            }
            else
            {
                sb.AppendLine("FCM: DROP FRAME");
            }

            sb.AppendLine();
            const string writeFormat = "{0:000000}  {1}       {2}     {3}        {4} {5} {6} {7}";
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                int no = index + 1;
                var p = subtitle.Paragraphs[index];
                if (index == 0 && p.StartTime.TotalSeconds > 1)
                {
                    var start = new TimeCode(p.StartTime.TotalMilliseconds - 1000.0);
                    var end = new TimeCode(p.StartTime.TotalMilliseconds - 1);
                    sb.AppendLine(string.Format(writeFormat, no, "BL", "V", "C", EncodeTimeCode(start), EncodeTimeCode(end), EncodeTimeCode(start), EncodeTimeCode(end)));
                    sb.AppendLine();
                }
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                sb.AppendLine(string.Format(writeFormat, no, "AX", "V", "C", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
                sb.AppendLine(TextPrefix + text);
                sb.AppendLine();
                var next = subtitle.GetParagraphOrDefault(no);
                if (next != null && next.StartTime.TotalMilliseconds > p.EndTime.TotalMilliseconds + 100)
                {
                    var start = new TimeCode(p.EndTime.TotalMilliseconds + 1);
                    var end = new TimeCode(start.TotalMilliseconds + 1000);
                    if (end.TotalMilliseconds >= next.StartTime.TotalMilliseconds)
                    {
                        end = new TimeCode(next.StartTime.TotalMilliseconds - 1);
                    }
                    sb.AppendLine(string.Format(writeFormat, no, "BL", "V", "C", EncodeTimeCode(start), EncodeTimeCode(end), EncodeTimeCode(start), EncodeTimeCode(end)));
                    sb.AppendLine();
                }
            }
            return sb.ToString().Trim() + Environment.NewLine;
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return $"{timeCode.Hours:00}:{timeCode.Minutes:00}:{timeCode.Seconds:00}:{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {   //002  AX       V     C        01:00:01:15 01:00:04:18 00:00:01:15 00:00:04:18
            //000002  AX V     C        01:00:04:00 01:00:05:00 00:00:02:05 00:00:03:05
            _errorCount = 0;
            Paragraph lastParagraph = null;
            int count = 0;
            var splitChar = new[] { ' ' };
            foreach (string line in lines)
            {
                bool isTimeCode = false;
                if (line.Length > 0)
                {
                    bool success = false;
                    if (line.Length > 65 && line.Length < 85 && line.IndexOf(':') > 20)
                    {
                        var match = Regex.Match(line);
                        if (match.Success)
                        {
                            isTimeCode = true;
                            if (lastParagraph != null && Math.Abs(lastParagraph.StartTime.TotalMilliseconds + 1) > 0.001)
                            {
                                subtitle.Paragraphs.Add(lastParagraph);
                            }

                            var arr = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                            try
                            {
                                if (arr.Length == 8 && arr[1] != "BL")
                                {
                                    var start = DecodeTimeCodeFrames(arr[6], SplitCharColon);
                                    var end = DecodeTimeCodeFrames(arr[7], SplitCharColon);
                                    lastParagraph = new Paragraph(start, end, string.Empty);
                                    success = true;
                                }
                                else
                                {
                                    lastParagraph = new Paragraph(string.Empty, -1, -1);
                                }
                            }
                            catch
                            {
                                _errorCount++;
                            }
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
            foreach (var paragraph in subtitle.Paragraphs)
            {
                if (paragraph.Text.StartsWith(TextPrefix, StringComparison.Ordinal))
                {
                    paragraph.Text = paragraph.Text.Remove(0, TextPrefix.Length).TrimStart();
                }
            }

            subtitle.Renumber();
        }

    }
}