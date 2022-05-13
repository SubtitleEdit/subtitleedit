using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimeCodesOnly3 : SubtitleFormat
    {
        // 01:00:00:15
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public const string NameOfFormat = "Time Codes Only 3";

        public override string Name => NameOfFormat;

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}");
            }

            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (var line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    var startParts = line.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(startParts), string.Empty);
                        p.EndTime.TotalMilliseconds += Configuration.Settings.General.NewEmptyDefaultMs;

                        subtitle.Paragraphs.Add(p);

                        var prev = subtitle.GetParagraphOrDefault(subtitle.GetIndex(p) - 1);
                        if (prev != null)
                        {
                            if (prev.EndTime.TotalMilliseconds > p.StartTime.TotalMilliseconds)
                            {
                                prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                            }
                        }
                    }
                }
                else if (p != null)
                {
                    _errorCount++;
                }
            }


            subtitle.Renumber();
        }
    }
}
