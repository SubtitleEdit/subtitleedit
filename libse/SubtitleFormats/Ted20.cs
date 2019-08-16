using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Ted20 : SubtitleFormat
    {

        private const int TextBufferSize = 64;

        public override string Extension => ".ted";

        public const string NameOfFormat = "TED Caption 2.0";

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
                    if (fileName.EndsWith(".ted", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
                        if (buffer[0] == 0x43 && buffer[1] == 0x41 && buffer[2] == 0x50 && buffer[3] == 0x54 && buffer[4] == 0x00 && buffer[5] == 0x32 && buffer[6] == 0x2e) //43 41 50 54 00 32 2E - CAPT.2
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

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
            int i = 244; // start of first time code (14 bytes block length)
            bool timeCodesDone = false;
            int number = 1;
            while (i < buffer.Length - 15 && !timeCodesDone)
            {
                var paragraph = ReadTimeCode(buffer, i, number);
                if (paragraph != null)
                {
                    subtitle.Paragraphs.Add(paragraph);
                }
                else
                {
                    timeCodesDone = true;
                }
                i += 14;
                number++;
            }

            var texts = new List<string>();
            while (i < buffer.Length - TextBufferSize)
            {
                if (buffer[i + 0] == 0xff &&
                    buffer[i + 1] == 0xff &&
                    buffer[i + 2] == 0x01 &&
                    buffer[i + 3] == 0x01 &&
                    buffer[i + 4] == 0x01 &&
                    buffer[i + TextBufferSize] == 0xff)
                {
                    var text = ReadText(buffer, i);
                    texts.Add(text);

                    i += TextBufferSize;
                }
                else
                {
                    i++;
                }
            }

            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                if (index < texts.Count)
                {
                    paragraph.Text = texts[index];
                }

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
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static string ReadText(byte[] buffer, int i)
        {
            var sb = new StringBuilder();
            const int textStartIndex = 21;
            int index = textStartIndex;
            while (index < TextBufferSize)
            {
                if (buffer[i + index] == 3)
                {
                    break;
                }
                if (buffer[i + index] == 0x0d && buffer[i + index + 1] == 2 && buffer[i + index + 2] == 1)
                {
                    // 0D 02 01 = ?
                    index += 3;
                }
                else if (buffer[i + index] == 0x12 && buffer[i + index + 1] == 0x29)
                {
                    // 12 29 = ?
                    index += 2;
                }
                else if (buffer[i + index] == 0)
                {
                    index++;
                }
                else
                {
                    sb.Append(Encoding.UTF8.GetString(buffer, i + index, 1));
                    index++;
                }
            }
            return sb.ToString();
        }

        private static Paragraph ReadTimeCode(byte[] buffer, int i, int number)
        {
            if (buffer[i] + buffer[i + 1] * 256 == number)
            {
                var start = new TimeCode(buffer[i + 6], buffer[i + 7], buffer[i + 8], FramesToMillisecondsMax999(buffer[i + 9]));
                TimeCode end;
                if (buffer[i + 10] == 0xff && buffer[i + 11] == 0xff && buffer[i + 12] == 0xff && buffer[i + 13] == 0xff)
                {
                    end = new TimeCode();
                }
                else
                {
                    end = new TimeCode(buffer[i + 10], buffer[i + 11], buffer[i + 12], FramesToMillisecondsMax999(buffer[i + 13]));
                }
                return new Paragraph(start, end, string.Empty);
            }
            return null;
        }

    }
}
