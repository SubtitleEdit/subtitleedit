using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class VidIcelandic : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^#S\d{14}$", RegexOptions.Compiled);

        public override string Extension => ".vid";

        public override string Name => "Vid Icelandic";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> inputLines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            var lines = new List<string>(inputLines);
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) && File.Exists(fileName))
            {
                lines = FileUtil.ReadAllLinesShared(fileName, Encoding.GetEncoding(861)); // icelandic dos encoding
            }

            foreach (var line in lines)
            {
                if (line.StartsWith('#') && RegexTimeCodes.IsMatch(line))
                {
                    var startTime = DecodeTimeCode(line);
                    var endTime = DecodeDuration(startTime, line);
                    p = new Paragraph(startTime, endTime, string.Empty);
                    subtitle.Paragraphs.Add(p);
                }
                else if (line.Length > 11 && p != null && line.StartsWith('B'))
                {
                    p.Text = (p.Text.TrimEnd() + Environment.NewLine + line.Remove(0, 11).Trim()).Trim();
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string line)
        {
            var hour = line.Substring(2, 1);
            var minutes = line.Substring(3, 2);
            var seconds = line.Substring(5, 2);
            var frames = line.Substring(7, 2);
            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

        private static TimeCode DecodeDuration(TimeCode startTime, string line)
        {
            var seconds = line.Substring(10, 2);
            var frames = line.Substring(12, 2);
            return new TimeCode(startTime.Hours, startTime.Minutes, startTime.Seconds + int.Parse(seconds), startTime.Milliseconds + FramesToMillisecondsMax999(int.Parse(frames)));
        }
    }
}
