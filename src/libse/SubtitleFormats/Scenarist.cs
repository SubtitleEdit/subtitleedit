using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Scenarist : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public const string NameOfFormat = "Scenarist";

        public override string Name => NameOfFormat;

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                //0003  00:00:28:16 00:00:31:04 Jeg vil lære jer   frygten for HERREN."  (newline is \t)
                sb.AppendLine($"{index + 1:0000}\t{EncodeTimeCode(p.StartTime)}\t{EncodeTimeCode(p.EndTime)}\t{HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "\t")}");
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (var line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    var temp = line.Substring(0, RegexTimeCodes.Match(line).Length);
                    var start = temp.Substring(5, 11);
                    var end = temp.Substring(12 + 5, 11);

                    var startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    var endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        var text = line.Remove(0, RegexTimeCodes.Match(line).Length - 1).Trim();
                        if (!text.Contains(Environment.NewLine))
                        {
                            text = text.Replace("\t", Environment.NewLine);
                        }

                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    _errorCount++;
                }
            }

            for (var index = 0; index < subtitle.Paragraphs.Count - 1; index++)
            {
                var current = subtitle.Paragraphs[index];
                var next = subtitle.Paragraphs[index + 1];
                if (Math.Abs(current.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) < 0.01)
                {
                    if (current.EndTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines >
                        current.StartTime.TotalMilliseconds)
                    {
                        current.EndTime.TotalMilliseconds -= Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
            }

            subtitle.Renumber();
        }

    }
}
