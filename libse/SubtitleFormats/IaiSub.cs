using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class IaiSub : SubtitleFormat
    {
        public override string Extension => ".sub";

        public override string Name => "IAI subtitle";

        public override bool IsTimeBased => false;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    var fi = new FileInfo(fileName);
                    if (fi.Length > 100 && fi.Length < 1024000) // not too small or too big
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
                        if (buffer[00] == 0x49 &&
                            buffer[01] == 0x41 &&
                            buffer[02] == 0x49 &&
                            buffer[03] == 0x5f &&
                            buffer[04] == 0x53 &&
                            buffer[05] == 0x55 &&
                            buffer[06] == 0x42 &&
                            buffer[07] == 0x54 &&
                            fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int index = 32;
            while (index < buffer.Length)
            {
                Paragraph p = GetParagraph(ref index, buffer);
                if (p != null)
                {
                    subtitle.Paragraphs.Add(p);
                }
            }
            subtitle.RecalculateDisplayTimes(25, null, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
            subtitle.Renumber();
        }

        private Paragraph GetParagraph(ref int index, byte[] buffer)
        {
            var paragraphFound1 = false;
            var paragraphFound2 = false;
            while (index < buffer.Length && !(paragraphFound1 || paragraphFound2))
            {
                if (index > buffer.Length - 15)
                {
                    index += 20;
                    return null;
                }
                paragraphFound1 = buffer[index + 0] == 0xff && buffer[index + 1] == 0xff && buffer[index + 2] == 0xff && buffer[index + 3] == 0xff &&
                                 buffer[index + 4] == 0xff && buffer[index + 5] == 0xff && buffer[index + 6] == 0xff && buffer[index + 7] == 0xff;
                paragraphFound2 = buffer[index + 0] == 0x1a && buffer[index + 1] == 0x00 && buffer[index + 4] == 0x00 && buffer[index + 5] == 0x00;

                index++;
            }
            if (paragraphFound1)
            {
                index += 7;
            }
            else
            {
                index += 9;
            }

            if (index + 5 >= buffer.Length)
            {
                return null;
            }

            var startTime = DecodeTimeCode(buffer, index);

            index += 4;
            if (index + 5 >= buffer.Length)
            {
                return null;
            }

            var endTime = DecodeTimeCode(buffer, index);

            var text = new StringBuilder();
            index += 12;
            int startText = index;
            int max = index + 100; // safety - don't read more than 100 chars
            while (index < buffer.Length - 5 && buffer[index] > 0 &&
                  !(index > startText && buffer[index - 1] == 0 && buffer[index] == 0x1a && buffer[index + 1] == 0 && ((buffer[index + 4] == 00 && buffer[index + 5] == 0) || (buffer[index + 4] == 0xff && buffer[index + 5] == 0xff))) &&
                  index < max)
            {
                if (index + 5 >= buffer.Length)
                {
                    return null;
                }

                int length = buffer[index];
                if (index + length > buffer.Length)
                {
                    if (text.ToString().Trim().Length > 0)
                    {
                        return new Paragraph(startTime, endTime, text.ToString().Trim());
                    }

                    return null;
                }
                for (int i = 7; i < length; i++)
                {
                    if (index < buffer.Length && buffer[index + i] >= 32)
                    {
                        text.Append(Encoding.Default.GetString(buffer, index + i, 1));
                    }
                }
                text.AppendLine();
                index += length;
            }
            index--;
            return new Paragraph(startTime, endTime, text.ToString().Trim());
        }

        private static TimeCode DecodeTimeCode(byte[] buffer, int index)
        {
            return new TimeCode(0, 0, 0, FramesToMilliseconds((buffer[index + 1] << 8) + buffer[index]));
        }

    }
}
