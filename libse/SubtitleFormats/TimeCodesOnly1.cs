using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimeCodesOnly1 : SubtitleFormat
    {
        // 1<HT>1:01:08:05<HT>1:01:10:21<HT>02:16
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+\t\d+:\d\d:\d\d:\d\d\t\d+:\d\d:\d\d:\d\d\t\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public const string NameOfFormat = "Time Codes Only 1";

        public override string Name => NameOfFormat;

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{index}\t{EncodeTimeCode(p.StartTime)}\t{EncodeTimeCode(p.EndTime)}\t{p.Duration}");
                index++;
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
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    var temp = line.Split('\t');
                    var startParts = temp[1].Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    var endParts = temp[2].Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), string.Empty);
                        subtitle.Paragraphs.Add(p);
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
