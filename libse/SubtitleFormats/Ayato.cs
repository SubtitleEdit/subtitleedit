using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Ayato : SubtitleFormat
    {
        public override string Extension => ".aya";

        public override string Name => "Ayato";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 3000 && fi.Length < 1024000) // not too small or too big
                {
                    if (!fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return base.IsMine(lines, fileName);
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName, string videoFileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                // header
                var header = new byte[2713];
                header[00] = 0x05;
                header[01] = 0x30;
                header[02] = 0x32;
                header[03] = 0x2E;
                header[04] = 0x30;
                header[05] = 0x33;
                header[06] = 0x05;
                header[07] = 0x41;
                header[08] = 0x79;
                header[09] = 0x61;
                header[10] = 0x74;
                header[11] = 0x6F;
                header[12] = 0x02;
                header[77] = 0x01;
                header[81] = 0x01;
                header[608] = 0x3b;
                header[609] = 0x32;
                header[613] = 0x58;
                header[614] = 0x02;
                header[615] = 0xE8;
                header[616] = 0x03;
                header[617] = 0xE8;
                header[618] = 0x03;
                header[619] = 0xE8;
                header[620] = 0x03;
                header[621] = 0xE8;
                header[622] = 0x03;
                header[639] = 0x02;

                header[682] = (byte)(subtitle.Paragraphs.Count & 0xff);
                header[683] = (byte)((subtitle.Paragraphs.Count >> 8) & 0xff);

                header[686] = 0x09;
                header[687] = 0x04;

                header[751] = 0x08;

                header[760] = 0x01;
                header[761] = 0x0a;

                header[830] = 0x58;
                header[831] = 0x48;

                header[2048] = 0x99;
                header[2049] = 0x02;

                header[2050] = (byte)(subtitle.Paragraphs.Count & 0xff);
                header[2051] = (byte)((subtitle.Paragraphs.Count >> 8) & 0xff);

                header[2069] = 0x17;
                header[2071] = 0x17;
                header[2073] = 0x02;
                header[2075] = 0x27;
                header[2077] = 0x0c;
                header[2079] = 0x04;

                header[2082] = 0x01;
                header[2085] = 0x01;

                // Microsoft Sans Serif
                header[2088] = 0x4d;
                header[2089] = 0x69;
                header[2090] = 0x63;
                header[2091] = 0x72;
                header[2092] = 0x6f;
                header[2093] = 0x73;
                header[2094] = 0x6f;
                header[2095] = 0x66;
                header[2096] = 0x74;
                header[2097] = 0x20;
                header[2098] = 0x53;
                header[2099] = 0x61;
                header[2100] = 0x6e;
                header[2101] = 0x73;
                header[2102] = 0x20;
                header[2103] = 0x53;
                header[2104] = 0x65;
                header[2105] = 0x72;
                header[2106] = 0x69;
                header[2107] = 0x66;

                header[2120] = 0x1f;
                header[2123] = 0x01;
                header[2124] = 0x02;

                header[2128] = 0xff;
                header[2136] = 0x02;
                header[2176] = 0x01;
                header[2193] = 0x02;
                header[2194] = 0x02;

                header[2197] = 0x0C;
                header[2198] = 0x14;
                header[2199] = 0x0C;
                header[2200] = 0x01;
                header[2201] = 0xe8;
                header[2202] = 0x03;
                header[2203] = 0xe8;
                header[2204] = 0x03;
                header[2205] = 0xe8;
                header[2206] = 0x03;
                header[2207] = 0xe8;
                header[2208] = 0x03;

                header[2225] = 0x13;
                header[2226] = 0x08;
                header[2227] = 0xdf;
                header[2228] = 0x07;
                header[2229] = 0x13;
                header[2230] = 0x08;
                header[2231] = 0xdf;
                header[2232] = 0x07;

                fs.Write(header, 0, header.Length);

                // paragraphs
                var sub = new Subtitle(subtitle);
                int number = 1;
                foreach (Paragraph p in sub.Paragraphs)
                {
                    WriteParagraph(fs, p, number);
                    number++;
                }
            }
        }

        private static void WriteParagraph(Stream stream, Paragraph paragraph, int number)
        {
            // subtitle number
            stream.WriteByte((byte)(number & 0xff));
            stream.WriteByte((byte)((number >> 8) & 0xff));

            stream.WriteByte(0);
            stream.WriteByte(0);
            WriteFrames(stream, paragraph.StartTime);
            stream.WriteByte(0);
            WriteFrames(stream, paragraph.EndTime);
            stream.WriteByte(0);

            stream.WriteByte(0x17);
            stream.WriteByte(0);
            stream.WriteByte(2);
            stream.WriteByte(0);

            WriteText(stream, paragraph.Text);
        }

        private static void WriteFrames(Stream stream, TimeCode timeCode)
        {
            var frames = (uint)Math.Round((double)MillisecondsToFrames(timeCode.TotalMilliseconds));
            stream.WriteByte((byte)(frames & 0xff));
            stream.WriteByte((byte)((frames >> 8) & 0xff));
            stream.WriteByte((byte)((frames >> 16) & 0xff));
        }

        private static void WriteText(Stream stream, string text)
        {
            var bytes = MakeBytes(text);

            stream.WriteByte((byte)(bytes.Length + 1)); // text length

            for (int i = 0; i < 55; i++) // 55 bytes zero padding
            {
                stream.WriteByte(0);
            }

            stream.WriteByte(7);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static byte[] MakeBytes(string text)
        {
            var bytesList = new List<byte>();
            int count = 0;
            foreach (var line in HtmlUtil.RemoveHtmlTags(text, true).SplitToLines())
            {
                if (count > 0)
                {
                    bytesList.Add(0x1f);
                    bytesList.Add(0x7);
                }
                bytesList.AddRange(Encoding.UTF8.GetBytes(line));
                count++;
            }
            return bytesList.ToArray();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            const int startPosition = 0xa99;
            const int textPosition = 72;

            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            var buffer = FileUtil.ReadAllBytesShared(fileName);
            int index = startPosition;
            if (buffer[index] != 1)
            {
                return;
            }

            while (index + textPosition < buffer.Length)
            {
                int textLength = buffer[index + 16];
                if (textLength > 0 && index + textPosition + textLength < buffer.Length)
                {
                    string text = GetText(index + textPosition, textLength, buffer);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        int startFrames = GetFrames(index + 4, buffer);
                        int endFrames = GetFrames(index + 8, buffer);
                        subtitle.Paragraphs.Add(new Paragraph(text, FramesToMilliseconds(startFrames), FramesToMilliseconds(endFrames)));
                    }
                }
                index += textPosition + textLength;
            }
            subtitle.Renumber();
        }

        private static string GetText(int index, int length, byte[] buffer)
        {
            if (length < 1)
            {
                return string.Empty;
            }

            int offset = 0;
            if (buffer[index] == 7)
            {
                offset = 1;
            }
            else if (buffer[index + 1] == 7)
            {
                offset = 2;
            }
            else if (buffer[index + 2] == 7)
            {
                offset = 3;
            }

            if (buffer[index + offset] < 32)
            {
                offset++;
            }

            if (length - offset < 1)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var textBytes = new List<byte>();
            int i = index + offset;
            int max = i + length - offset;
            while (i < max)
            {
                if (i + 3 < max && buffer[i] < 32 && buffer[i + 1] < 32 && buffer[i + 2] < 32 && buffer[i + 3] == 7)
                {
                    AddToLine(textBytes, sb);
                    sb.AppendLine();
                    if (i + 4 < max && buffer[i + 4] < 32)
                    {
                        i++;
                    }
                    i += 3;
                }
                else if (i + 2 < max && buffer[i] < 32 && buffer[i + 1] < 32 && buffer[i + 2] == 7)
                {
                    AddToLine(textBytes, sb);
                    sb.AppendLine();
                    if (i + 3 < max && buffer[i + 3] < 32)
                    {
                        i++;
                    }
                    i += 2;
                }
                else if (i + 1 < max && buffer[i] < 32 && buffer[i + 1] == 7)
                {
                    AddToLine(textBytes, sb);
                    sb.AppendLine();
                    if (i + 2 < max && buffer[i + 2] < 32)
                    {
                        i++;
                    }
                    i++;
                }
                else if (buffer[i] == 7)
                {
                    AddToLine(textBytes, sb);
                    sb.AppendLine();
                    if (i + 1 < max && buffer[i + 1] < 32)
                    {
                        i++;
                    }
                }
                else
                {
                    textBytes.Add(buffer[i]);
                }
                i++;
            }
            AddToLine(textBytes, sb);
            return sb.ToString();
        }

        private static void AddToLine(List<byte> textBytes, StringBuilder sb)
        {
            if (textBytes.Count > 0)
            {
                var lineBuffer = textBytes.ToArray();
                sb.Append(Encoding.UTF8.GetString(lineBuffer, 0, lineBuffer.Length));
                textBytes.Clear();
            }
        }

        private static int GetFrames(int index, byte[] buffer)
        {
            return (buffer[index + 2] << 16) + (buffer[index + 1] << 8) + buffer[index];
        }

    }
}
