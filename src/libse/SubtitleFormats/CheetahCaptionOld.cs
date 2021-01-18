using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CheetahCaptionOld : SubtitleFormat
    {

        public override string Extension => ".cap";

        public const string NameOfFormat = "Cheetah Caption Old";

        public override string Name => NameOfFormat;

        public static void Save(string fileName, Subtitle subtitle)
        {
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 200 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.EndsWith(".cap", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
                        if (buffer[0] == 0xEA && buffer[1] == 0x10)
                        {
                            var subtitle = new Subtitle();
                            LoadSubtitle(subtitle, lines, fileName);
                            return subtitle.Paragraphs.Count > _errorCount || _errorCount > 25;
                        }
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        private static TimeCode DecodeTimestamp(byte[] buffer, int index)
        {
            return new TimeCode(buffer[index], buffer[index + 1], buffer[index + 2], FramesToMillisecondsMax999(buffer[index + 3]));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
            int i = 0x80;
            var sb = new StringBuilder();
            while (i < buffer.Length - 0xb1)
            {
                if (buffer[i] > 30)
                {
                    _errorCount++;
                }
                sb.AppendLine(Encoding.ASCII.GetString(buffer, i + 0x1a, 38).Replace("\0", string.Empty));
                sb.Append(Encoding.ASCII.GetString(buffer, i + 0x40, 38).Replace("\0", string.Empty));
                var p = new Paragraph(DecodeTimestamp(buffer, i), DecodeTimestamp(buffer, i + 4), sb.ToString().Trim());
                subtitle.Paragraphs.Add(p);
                sb.Clear();
                i += 0xb2;
            }
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (paragraph.Duration.TotalSeconds > 100)
                {
                    _errorCount++;
                }
                if (Math.Abs(paragraph.EndTime.TotalMilliseconds) < 0.1 || paragraph.Duration.TotalMilliseconds < 0.1)
                {
                    if (next != null)
                    {
                        paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                    if (next == null || paragraph.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    {
                        paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
