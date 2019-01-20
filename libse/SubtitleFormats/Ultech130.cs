using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// The ULTECH caption file format (ULT/ULD file) is a compact binary file that stores captions with embedded EIA-608 control codes
    /// http://en.wikipedia.org/wiki/EIA-608
    /// </summary>
    public class Ultech130 : SubtitleFormat
    {
        private const string UltechId = "ULTECH\01.30";
        public const string NameOfFormat = "Ultech 1.30 Caption";

        public override string Extension => ".ult";

        public override string Name => NameOfFormat;

        public static void Save(string fileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(UltechId);
                fs.Write(buffer, 0, buffer.Length);

                buffer = new byte[] { 0, 0, 2, 0x1D, 0 }; // ?
                fs.Write(buffer, 0, buffer.Length);

                int numberOfLines = subtitle.Paragraphs.Count;
                fs.WriteByte((byte)(numberOfLines % 256)); // paragraphs - low byte
                fs.WriteByte((byte)(numberOfLines / 256)); // paragraphs - high byte

                buffer = new byte[] { 0, 0, 0, 0, 0x1, 0, 0xF, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0xE, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0xD, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0xC, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // ?
                fs.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes("Subtitle Edit");
                fs.Write(buffer, 0, buffer.Length);

                while (fs.Length < 512)
                {
                    fs.WriteByte(0);
                }

                var footer = new byte[] { 0xF1, 0x0B, 0x00, 0x00, 0x00, 0x1B, 0x18, 0x14, 0x20, 0x14, 0x2E, 0x14, 0x2F, 0x00 }; // footer

                // convert line breaks
                var sb = new StringBuilder();
                var line = new StringBuilder();

                // paragraphs
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    sb.Clear();
                    line.Clear();

                    int skipCount = 0;
                    int numberOfNewLines = Utilities.GetNumberOfLines(p.Text);
                    bool italic = p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal);
                    string text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                    if (italic)
                    {
                        sb.Append('\u0011');
                        sb.Append('\u002E');
                    }
                    int y = 0x74 - (numberOfNewLines * 0x20);
                    for (int j = 0; j < text.Length; j++)
                    {
                        if (text.Substring(j).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                        {
                            y += 0x20;
                            if (line.Length > 0)
                            {
                                sb.Append(line);
                            }

                            line.Clear();
                            skipCount = Environment.NewLine.Length - 1;
                            sb.Append('\u0014');
                            sb.Append(Convert.ToChar((byte)(y)));

                            //center
                            sb.Append('\u0017');
                            sb.Append('\u0021');

                            if (italic)
                            {
                                sb.Append('\u0011');
                                sb.Append('\u002E');
                            }
                        }
                        else if (skipCount == 0)
                        {
                            line.Append(text[j]);
                        }
                        else
                        {
                            skipCount--;
                        }
                    }
                    if (line.Length > 0)
                    {
                        sb.Append(line);
                    }

                    text = sb.ToString();

                    // codes?
                    buffer = new byte[] {
                        0x14,
                        0x20,
                        0x14,
                        0x2E,
                        0x14,
                        (byte)(0x74 - (numberOfNewLines * 0x20)),

                        0x17, 0x21, // 0x1721=center, 0x1722=right ?
                    };

                    fs.WriteByte(0xF1); //ID of start record

                    // length
                    int length = text.Length + 15;
                    fs.WriteByte((byte)(length));
                    fs.WriteByte(0);

                    // start time
                    WriteTime(fs, p.StartTime);
                    fs.Write(buffer, 0, buffer.Length);

                    // text
                    buffer = Encoding.ASCII.GetBytes(text);
                    fs.Write(buffer, 0, buffer.Length); // Text starter på index 19 (0 baseret)
                    fs.WriteByte(0x14);
                    fs.WriteByte(0x2F);
                    fs.WriteByte(0);

                    // end time
                    fs.WriteByte(0xF1); // id of start record
                    fs.WriteByte(7); // length of end time
                    fs.WriteByte(0);
                    WriteTime(fs, p.EndTime);
                    fs.WriteByte(0x14);
                    fs.WriteByte(0x2c);
                    fs.WriteByte(0);
                }

                buffer = footer;
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        private static void WriteTime(FileStream fs, TimeCode timeCode)
        {
            fs.WriteByte((byte)timeCode.Hours);
            fs.WriteByte((byte)timeCode.Minutes);
            fs.WriteByte((byte)timeCode.Seconds);
            fs.WriteByte((byte)MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds));
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) ||
                !(fileName.EndsWith(".ult", StringComparison.Ordinal) || fileName.EndsWith(".uld", StringComparison.Ordinal)) ||
                !File.Exists(fileName))
            {
                return false;
            }

            var fi = new FileInfo(fileName);
            // too short or too big
            if (fi.Length < 200 || fi.Length >= 1024000)
            {
                return false;
            }
            var buffer = new byte[UltechId.Length];
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int read = fs.Read(buffer, 0, UltechId.Length);
                if (read != UltechId.Length)
                {
                    return false;
                }
                // Expected: ULTECH\01.30
                return Encoding.ASCII.GetString(buffer).Equals(UltechId, StringComparison.OrdinalIgnoreCase);
            }
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

            int i = 512;
            Paragraph last = null;
            var sb = new StringBuilder();
            while (i < buffer.Length - 25)
            {
                var p = new Paragraph();
                int length = buffer[i + 1];

                p.StartTime = DecodeTimestamp(buffer, i + 3);
                if (last != null && Math.Abs(last.EndTime.TotalMilliseconds) < 0.001)
                {
                    last.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                }

                if (length > 22)
                {
                    int start = i + 7;
                    sb.Clear();
                    int skipCount = 0;
                    bool italics = false;
                    for (int k = start; k < length + i; k++)
                    {
                        byte b = buffer[k];
                        if (skipCount > 0)
                        {
                            skipCount--;
                        }
                        else if (b < 0x1F)
                        {
                            byte b2 = buffer[k + 1];
                            skipCount = 1;
                            if (sb.Length > 0 && !sb.ToString().EndsWith(Environment.NewLine, StringComparison.Ordinal) && !sb.EndsWith('>'))
                            {
                                if (italics)
                                {
                                    sb.Append("</i>");
                                }

                                sb.AppendLine();
                                italics = false;
                            }

                            if (b == 0x11 && b2 == 0x2e)
                            {
                                sb.Append("<i>");
                                italics = true;
                            }
                        }
                        else if (b == 0x80)
                        {
                        }
                        else
                        {
                            sb.Append(Encoding.GetEncoding(1252).GetString(buffer, k, 1));
                        }
                    }
                    p.Text = sb.ToString().Trim();
                    if (italics)
                    {
                        p.Text += "</i>";
                    }

                    p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    subtitle.Paragraphs.Add(p);
                }
                else if (last != null)
                {
                    last.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                }
                last = p;

                i += length + 3;
            }
            if (last != null)
            {
                if (Math.Abs(last.EndTime.TotalMilliseconds) < 0.001)
                {
                    last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 2500;
                }

                if (last.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(last.Text);
                }
            }

            subtitle.Renumber();
        }

        public override List<string> AlternateExtensions => new List<string> { ".uld" }; // Ultech drop frame
    }
}
