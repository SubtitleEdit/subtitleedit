using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CheetahCaption : SubtitleFormat
    {
        private static readonly Dictionary<byte, char> DicCodeLatin = new Dictionary<byte, char>
        {
            [0x81] = '♪',
            [0x82] = 'á',
            [0x83] = 'é',
            [0x84] = 'í',
            [0x85] = 'ó',
            [0x86] = 'ú',
            [0x87] = 'â',
            [0x88] = 'ê',
            [0x89] = 'î',
            [0x8A] = 'ô',
            [0x8B] = 'û',
            [0x8C] = 'à',
            [0x8D] = 'è',
            [0x8E] = 'Ñ',
            [0x8F] = 'ñ',
            [0x90] = 'ç',
            [0x91] = '¢',
            [0x92] = '£',
            [0x93] = '¿',
            [0x94] = '½',
            [0x95] = '®',
        };

        public override string Extension => ".cap";

        public const string NameOfFormat = "Cheetah Caption";

        public override string Name => NameOfFormat;

        public static void Save(string fileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = { 0xEA, 0x22, 1, 0 }; // header
                fs.Write(buffer, 0, buffer.Length);

                int numberOfLines = subtitle.Paragraphs.Count;
                fs.WriteByte((byte)(numberOfLines % 256)); // paragraphs - low byte
                fs.WriteByte((byte)(numberOfLines / 256)); // paragraphs - high byte

                buffer = new byte[] { 9, 0xA8, 0xAF, 0x4F }; // ?
                fs.Write(buffer, 0, buffer.Length);

                for (int i = 0; i < 118; i++)
                {
                    fs.WriteByte(0);
                }

                var dictionaryLatinCode = DicCodeLatin.ToLookup(pair => pair.Value, pair => pair.Key);

                // paragraphs
                for (int index = 0; index < subtitle.Paragraphs.Count; index++)
                {
                    var p = subtitle.Paragraphs[index];
                    var next = subtitle.GetParagraphOrDefault(index + 1);
                    string text = p.Text;

                    var bufferShort = new byte[]
                    {
                        0,
                        0,
                        3, // justification, 1=left, 2=right, 3=center
                        0xE, //horizontal position, 1=top, F=bottom
                        0x10 //horizontal position, 3=left, 0x10=center, 0x19=right
                    };

                    //styles + ?
                    buffer = new byte[]
                    {
                        0x12,
                        1,
                        0,
                        0,
                        0,
                        0,
                        3, // justification, 1=left, 2=right, 3=center
                        0xF, //horizontal position, 1=top, F=bottom
                        0x10 //horizontal position, 3=left, 0x10=center, 0x19=right
                    };

                    //Normal        : 12 01 00 00 00 00 03 0F 10
                    //Right-top     : 12 01 00 00 00 00 03 01 1C
                    //Top           : 12 01 00 00 00 00 03 01 10
                    //Left-top      : 12 01 00 00 00 00 03 01 05
                    //Left          : 12 01 00 00 00 00 03 0F 0A
                    //Right         : 12 01 00 00 00 00 03 0F 1E
                    //Left          : 12 03 00 00 00 00 03 0F 07

                    if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                    {
                        buffer[7] = 1; // align top (vertical)
                        bufferShort[3] = 1; // align top (vertical)
                    }
                    else if (text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal))
                    {
                        buffer[7] = 8; // center (vertical)
                        bufferShort[3] = 8; // align top (vertical)
                    }

                    if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an1}", StringComparison.Ordinal))
                    {
                        buffer[8] = 2; // align left (horizontal)
                        bufferShort[4] = 2; // align left (horizontal)
                    }
                    else if (text.StartsWith("{\\an9}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal) || text.StartsWith("{\\an3}", StringComparison.Ordinal))
                    {
                        buffer[8] = 0x1e; // align right (vertical)
                        bufferShort[4] = 0x1e; // align right (vertical)
                    }

                    int startTag = text.IndexOf('}');
                    if (text.StartsWith("{\\", StringComparison.Ordinal) && startTag > 0 && startTag < 10)
                    {
                        text = text.Remove(0, startTag + 1);
                    }

                    var textBytes = new List<byte>();
                    var italic = p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal);
                    text = HtmlUtil.RemoveHtmlTags(text);
                    int j = 0;
                    if (italic)
                    {
                        textBytes.Add(0xd0);
                    }

                    var encoding = Encoding.GetEncoding(1252);
                    while (j < text.Length)
                    {
                        if (text.Substring(j).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                        {
                            j += Environment.NewLine.Length;
                            textBytes.Add(0);
                            textBytes.Add(0);
                            textBytes.Add(0);
                            textBytes.Add(0);
                            if (italic)
                            {
                                textBytes.Add(0xd0);
                            }
                        }
                        else
                        {
                            if (dictionaryLatinCode.Contains(text[j]))
                            {
                                textBytes.AddRange(dictionaryLatinCode[text[j]]);
                            }
                            else
                            {
                                textBytes.Add(encoding.GetBytes(new[] { text[j] })[0]);
                            }

                            j++;
                        }
                    }

                    int length = textBytes.Count + 20;
                    long end = fs.Position + length;
                    if (Configuration.Settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime || next == null || next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds >= 1500)
                    {
                        fs.WriteByte((byte)length);

                        fs.WriteByte(p.Text.Trim().Contains(Environment.NewLine) ? (byte)0x62 : (byte)0x61);

                        WriteTime(fs, p.StartTime);
                        WriteTime(fs, p.EndTime);
                        fs.Write(buffer, 0, buffer.Length); // styles
                    }
                    else
                    {
                        length = textBytes.Count + 20 - (buffer.Length - bufferShort.Length);
                        end = fs.Position + length;
                        fs.WriteByte((byte)length);

                        fs.WriteByte(p.Text.Trim().Contains(Environment.NewLine) ? (byte)0x42 : (byte)0x41);

                        WriteTime(fs, p.StartTime);
                        fs.WriteByte(2);
                        fs.WriteByte(1);
                        fs.WriteByte(0);
                        fs.WriteByte(0);
                        fs.Write(bufferShort, 0, bufferShort.Length); // styles
                    }

                    foreach (byte b in textBytes) // text
                    {
                        fs.WriteByte(b);
                    }

                    while (end > fs.Position)
                    {
                        fs.WriteByte(0);
                    }
                }
            }
        }

        private static void WriteTime(Stream fs, TimeCode timeCode)
        {
            fs.WriteByte((byte)timeCode.Hours);
            fs.WriteByte((byte)timeCode.Minutes);
            fs.WriteByte((byte)timeCode.Seconds);
            fs.WriteByte((byte)MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds));
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
                        for (int i = 0; i < buffer.Length - 20; i++)
                        {
                            if (buffer[i + 0] == 0xEA &&
                                buffer[i + 1] == 0x22 &&
                                buffer[i + 2] <= 3)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title) => "Not supported!";

        private static TimeCode DecodeTimestamp(byte[] buffer, int index)
        {
            return new TimeCode(buffer[index], buffer[index + 1], buffer[index + 2], FramesToMillisecondsMax999(buffer[index + 3]));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int i = 128;
            Paragraph last = null;
            var sb = new StringBuilder();
            while (i < buffer.Length - 16)
            {
                var p = new Paragraph();
                int length = buffer[i];

                int usedBytes = 20;

                p.StartTime = DecodeTimestamp(buffer, i + 2);
                p.EndTime = DecodeTimestamp(buffer, i + 6);
                if (p.EndTime.Hours == 2 && p.EndTime.Minutes == 1 && p.EndTime.Seconds == 0 && p.EndTime.Milliseconds == 0 &&
                    (p.Duration.TotalMilliseconds < 0 || p.Duration.TotalMilliseconds > 5000))
                {
                    usedBytes = 20 - 4;
                }

                int textLength = length - usedBytes;
                int start = usedBytes - 1;
                for (int j = 0; j < 4 && i + start - 1 < buffer.Length; j++)
                {
                    if (buffer[i + start - 1] > 0x10)
                    {
                        start--;
                        textLength++;
                    }
                }

                if (textLength > 0 && buffer.Length >= i + textLength)
                {
                    if (last != null && last.EndTime.TotalMilliseconds > p.StartTime.TotalMilliseconds)
                    {
                        last.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }

                    sb.Clear();
                    int j = 0;
                    bool italics = false;
                    var encoding = Encoding.GetEncoding(1252);
                    while (j < textLength)
                    {
                        int index = i + start + j;
                        if (buffer[index] == 0)
                        {
                            if (italics)
                            {
                                sb.Append("</i>");
                            }

                            italics = false;
                            if (!sb.ToString().EndsWith(Environment.NewLine, StringComparison.Ordinal))
                            {
                                sb.AppendLine();
                            }
                        }
                        else if (DicCodeLatin.ContainsKey(buffer[index]))
                        {
                            sb.Append(DicCodeLatin[buffer[index]]);
                        }
                        else if (buffer[index] >= 0xC0 || buffer[index] <= 0x14) // codes/styles?
                        {
                            if (buffer[index] == 0xd0) // italics
                            {
                                italics = true;
                                sb.Append("<i>");
                            }
                        }
                        else
                        {
                            sb.Append(encoding.GetString(buffer, index, 1));
                        }
                        j++;
                    }
                    if (italics)
                    {
                        sb.Append("</i>");
                    }

                    p.Text = sb.ToString().Trim();
                    p.Text = p.Text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);

                    subtitle.Paragraphs.Add(p);
                    last = p;
                }
                if (length == 0)
                {
                    length++;
                }

                i += length;
            }
            if (last != null && (last.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds || last.Duration.TotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds))
            {
                last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(last.Text);
            }

            for (var index = 0; index < subtitle.Paragraphs.Count - 1; index++)
            {
                var current = subtitle.Paragraphs[index];
                var next = subtitle.Paragraphs[index + 1];
                if (current.EndTime.Hours == 2 && current.EndTime.Minutes == 1 && current.EndTime.Seconds == 0 && current.EndTime.Milliseconds == 0 &&
                    (current.Duration.TotalMilliseconds < 0 || current.Duration.TotalMilliseconds > 5000))
                {
                    current.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
            }

            subtitle.Renumber();
        }
    }
}
