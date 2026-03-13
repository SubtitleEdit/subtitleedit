using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CheetahCaptionOld : SubtitleFormat, IBinaryPersistableSubtitle
    {

        public override string Extension => ".cap";

        public const string NameOfFormat = "Cheetah Caption Old";

        public override string Name => NameOfFormat;

        public static void Save(string fileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                new CheetahCaptionOld().Save(fileName, fs, subtitle, false);
            }
        }

        public bool Save(string fileName, Stream stream, Subtitle subtitle, bool batchMode)
        {
            // 128-byte header: magic bytes 0xEA 0x10, rest zeros
            var header = new byte[0x80];
            header[0] = 0xEA;
            header[1] = 0x10;
            stream.Write(header, 0, header.Length);

            var encoding = Encoding.ASCII;
            foreach (var p in subtitle.Paragraphs)
            {
                var record = new byte[0xb2]; // 178 bytes per record

                // Start timecode at offset 0
                record[0] = (byte)p.StartTime.Hours;
                record[1] = (byte)p.StartTime.Minutes;
                record[2] = (byte)p.StartTime.Seconds;
                record[3] = (byte)MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds);

                // End timecode at offset 4
                record[4] = (byte)p.EndTime.Hours;
                record[5] = (byte)p.EndTime.Minutes;
                record[6] = (byte)p.EndTime.Seconds;
                record[7] = (byte)MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds);

                // Write text lines - line 1 at 0x1a (38 bytes), line 2 at 0x40 (38 bytes)
                var text = HtmlUtil.RemoveHtmlTags(p.Text);
                var lines = text.SplitToLines();
                WriteTextLine(record, 0x1a, lines.Count > 0 ? lines[0] : string.Empty, encoding);
                WriteTextLine(record, 0x40, lines.Count > 1 ? lines[1] : string.Empty, encoding);

                stream.Write(record, 0, record.Length);
            }

            return true;
        }

        private static void WriteTextLine(byte[] record, int offset, string text, Encoding encoding)
        {
            var bytes = encoding.GetBytes(text.Length > 38 ? text.Substring(0, 38) : text);
            Array.Copy(bytes, 0, record, offset, bytes.Length);
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

        public void LoadSubtitle(Subtitle subtitle, byte[] buffer)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
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
                if (paragraph.DurationTotalSeconds > 100)
                {
                    _errorCount++;
                }
                if (Math.Abs(paragraph.EndTime.TotalMilliseconds) < 0.1 || paragraph.DurationTotalMilliseconds < 0.1)
                {
                    if (next != null)
                    {
                        paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                    if (next == null || paragraph.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    {
                        paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                    }
                }
            }
            subtitle.Renumber();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
            LoadSubtitle(subtitle, buffer);
        }

    }
}
