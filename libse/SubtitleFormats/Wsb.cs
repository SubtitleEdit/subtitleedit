using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Wsb : SubtitleFormat
    {
        public override string Extension => ".WSB";

        public override string Name => "WSB";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine($"{index + 1:0000} : {EncodeTimeCode(p.StartTime)},{EncodeTimeCode(p.EndTime)},10");
                sb.AppendLine("80 80 80");
                foreach (string line in p.Text.SplitToLines())
                {
                    sb.AppendLine("C1Y00 " + line.Trim());
                }

                sb.AppendLine();
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return $"{time.Hours:00}{time.Minutes:00}{time.Seconds:00}{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //01072508010729007
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var indexOf7001 = line.IndexOf("7\x01\x01\0", StringComparison.Ordinal);
                var indexOfTen = line.IndexOf("     10     ", StringComparison.Ordinal);
                if (indexOf7001 >= 0 && indexOfTen > 0)
                {
                    try
                    {
                        string text = line.Substring(0, indexOfTen).Trim();
                        string time = line.Substring(indexOf7001 - 16, 16);

                        var starTime = time.Substring(0, 8);
                        var endTime = time.Substring(8);

                        string[] startTimeParts = { starTime.Substring(0, 2), starTime.Substring(2, 2), starTime.Substring(4, 2), starTime.Substring(6, 2) };
                        string[] endTimeParts = { endTime.Substring(0, 2), endTime.Substring(2, 2), endTime.Substring(4, 2), endTime.Substring(6, 2) };

                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startTimeParts), DecodeTimeCodeFramesFourParts(endTimeParts), text);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        _errorCount++;
                    }
                }
                else if (p != null)
                {
                    _errorCount++;
                }
            }
            if (p != null && !string.IsNullOrEmpty(p.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

    }
}
