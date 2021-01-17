using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class NciCaption : SubtitleFormat
    {
        public override string Extension => ".cap";

        public const string NameOfFormat = "NCI Caption";

        public override string Name => NameOfFormat;

        public static void Save(string fileName)
        {
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.EndsWith(".cap", StringComparison.OrdinalIgnoreCase))
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

                        return ((buffer[0] == 0x43 &&  // CAPT.2.0
                                buffer[1] == 0x41 &&
                                buffer[2] == 0x50 &&
                                buffer[3] == 0x54 &&
                                buffer[4] == 0x00 &&
                                buffer[5] == 0x32 &&
                                buffer[6] == 0x2e &&
                                buffer[7] == 0x30) ||
                               (buffer[0] == 0x43 && // CAPT.1.2
                                buffer[1] == 0x41 &&
                                buffer[2] == 0x50 &&
                                buffer[3] == 0x54 &&
                                buffer[4] == 0x00 &&
                                buffer[5] == 0x31 &&
                                buffer[6] == 0x2e &&
                                buffer[7] == 0x32));
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        private static TimeCode DecodeTimeCode(byte[] buffer, int index)
        {
            int hour = buffer[index];
            int minutes = buffer[index + 1];
            int seconds = buffer[index + 2];
            int frames = buffer[index + 3];

            int milliseconds = (int)Math.Round(TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate * frames);
            if (milliseconds > 999)
            {
                milliseconds = 999;
            }

            return new TimeCode(hour, minutes, seconds, milliseconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            string title = Encoding.ASCII.GetString(buffer, 82, 66);

            int i = 128;
            var encoding = Encoding.GetEncoding(1252);
            while (i < buffer.Length - 66)
            {
                if (buffer[i] == 0xff && buffer[i + 1] == 0xff && buffer[i + 3] != 0xff && buffer[i - 1] != 0xff && buffer[i + 64] == 0xff && buffer[i + 65] == 0xff)
                {
                    var p = new Paragraph();
                    var sb = new StringBuilder();
                    int j = i + 4;
                    while (j < i + 64)
                    {
                        if (buffer[j] == 0)
                        {
                            break;
                        }
                        else if (buffer[j] == 0xd)
                        {
                            sb.AppendLine();
                            j += 3;
                        }
                        else if (buffer[j] == 0x87)
                        {
                            sb.Append('♪');
                            j++;
                        }
                        else
                        {
                            sb.Append(encoding.GetString(buffer, j, 1));
                            j++;
                        }
                    }
                    p.Text = sb.ToString();
                    subtitle.Paragraphs.Add(p);
                    i += 62;
                }
                else
                {
                    i++;
                }
            }
            subtitle.Renumber();

            if (buffer[0] == 0x43 && // CAPT.1.2
                                buffer[1] == 0x41 &&
                                buffer[2] == 0x50 &&
                                buffer[3] == 0x54 &&
                                buffer[4] == 0x00 &&
                                buffer[5] == 0x31 &&
                                buffer[6] == 0x2e &&
                                buffer[7] == 0x32)
            {
                i = 396;
                int number = 0;
                while (i < buffer.Length - 66)
                {
                    if (buffer[i] == 0xff && buffer[i + 1] == 0xff && buffer[i + 2] != 0xff && buffer[i + 28] == 0xff && buffer[i + 29] == 0xff && buffer[i + 30] != 0xff)
                    {
                        if (buffer[i + 14] == number + 1)
                        {
                            Paragraph p = subtitle.GetParagraphOrDefault(number);
                            if (p != null)
                            {
                                p.StartTime = DecodeTimeCode(buffer, i + 18);
                                p.EndTime = DecodeTimeCode(buffer, i + 22);
                                number++;
                            }
                            i += 25;
                        }
                    }
                    i++;
                }
            }
            else
            {
                i = 230;
                int countTimecodes = 0;
                int start = i;
                int lastNumber = -1;
                while (i < buffer.Length - 66)
                {
                    if (buffer[i] == 0xff && buffer[i + 1] == 0xff && buffer[i + 2] == 0xff && buffer[i + 3] == 0xff)
                    {
                        int length = i - start - 2;
                        if (length >= 10)
                        {
                            int count = length / 14;
                            if (length % 14 == 10)
                            {
                                count++;
                            }
                            else
                            {
                                if (length % 14 == 8)
                                {
                                    count++;
                                }
                            }
                            for (int k = 0; k < count; k++)
                            {
                                int index = start + 2 + (14 * k);
                                int number = buffer[index] + buffer[index + 1] * 256;
                                if (number != lastNumber + 1)
                                {
                                    int tempNumber = buffer[index - 2] + buffer[index - 1] * 256;
                                    if (tempNumber == lastNumber + 1)
                                    {
                                        index -= 2;
                                        number = tempNumber;
                                    }
                                }
                                if (number > lastNumber)
                                {
                                    lastNumber = number;
                                    Paragraph p = subtitle.GetParagraphOrDefault(number);
                                    if (p != null)
                                    {
                                        if (k < count - 1)
                                        {
                                            p.StartTime = DecodeTimeCode(buffer, index + 6);
                                            p.EndTime = DecodeTimeCode(buffer, index + 10);
                                        }
                                        else
                                        {
                                            p.StartTime = DecodeTimeCode(buffer, index + 6);
                                        }
                                        countTimecodes++;
                                    }
                                }
                            }
                        }
                        start = i + 2;
                        i += 5;
                    }
                    i++;
                }
            }

            for (i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null && Math.Abs(p.EndTime.TotalMilliseconds) < 0.01)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
            }

            for (i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (p.Duration.TotalMilliseconds <= 0 && next != null)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
            }

            subtitle.RemoveEmptyLines();
            Paragraph last = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1);
            if (last != null)
            {
                last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(last.Text);
            }

            subtitle.Renumber();
        }

    }
}
