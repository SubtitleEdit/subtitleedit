using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// UniPac
    /// </summary>
    public class PacUnicode : SubtitleFormat
    {

        public override string Extension => ".fpc";

        public override string Name => "PAC Unicode (UniPac)";

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

                        if (buffer[00] == 1 &&
                            buffer[01] == 0 &&
                            buffer[02] == 0 &&
                            buffer[03] == 0 &&
                            buffer[04] == 0 &&
                            buffer[05] == 0 &&
                            buffer[06] == 0 &&
                            buffer[07] == 0 &&
                            buffer[08] == 0 &&
                            buffer[09] == 0 &&
                            buffer[10] == 0 &&
                            buffer[11] == 0 &&
                            buffer[12] == 0 &&
                            buffer[13] == 0 &&
                            buffer[14] == 0 &&
                            buffer[15] == 0 &&
                            buffer[16] == 0 &&
                            buffer[17] == 0 &&
                            buffer[18] == 0 &&
                            buffer[19] == 0 &&
                            buffer[20] == 0 &&
                            fileName.EndsWith(".fpc", StringComparison.OrdinalIgnoreCase))
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
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int index = 0;
            while (index < buffer.Length)
            {
                Paragraph p = GetPacParagraph(ref index, buffer);
                if (p != null)
                {
                    subtitle.Paragraphs.Add(p);
                }
            }
            if (subtitle.Paragraphs.Count > 2 && subtitle.Paragraphs[0].StartTime.TotalMilliseconds < 0.001 && subtitle.Paragraphs[0].EndTime.TotalMilliseconds < 0.001)
            {
                subtitle.Paragraphs.RemoveAt(0);
            }
            subtitle.Renumber();
        }

        private static Paragraph GetPacParagraph(ref int index, byte[] buffer)
        {
            while (index < 15)
            {
                index++;
            }
            bool con = true;
            while (con)
            {
                index++;
                if (index + 20 >= buffer.Length)
                {
                    return null;
                }

                if (buffer[index] == 0xFE && buffer[index - 1] == 0x80)
                {
                    con = false;
                }
            }

            int feIndex = index;
            byte alignment = buffer[feIndex + 1];
            byte verticalAlignment = buffer[feIndex - 1];
            var p = new Paragraph();

            int timeStartIndex = feIndex - 15;
            p.StartTime = Pac.GetTimeCode(timeStartIndex + 1, buffer);
            p.EndTime = Pac.GetTimeCode(timeStartIndex + 5, buffer);

            int textLength = buffer[timeStartIndex + 9] + buffer[timeStartIndex + 10] * 256;
            if (textLength > 500)
            {
                return null; // probably not correct index
            }

            int maxIndex = timeStartIndex + 10 + textLength;

            var sb = new StringBuilder();
            index = feIndex + 3;

            int textIndex = index;
            int textBegin = index;
            while (textIndex < buffer.Length && textIndex <= maxIndex)
            {
                if (buffer[textIndex] == 0xFE)
                {
                    if (textIndex > textBegin)
                    {
                        for (int j = textBegin; j <= textIndex - textBegin - 1; j++)
                        {
                            if (buffer[j] == 0xff)
                            {
                                buffer[j] = 0x2e; // replace end of line marker
                            }
                        }

                        sb.AppendLine(Encoding.UTF8.GetString(buffer, textBegin, textIndex - textBegin));
                        textBegin = textIndex + 7;
                        textIndex += 6;
                    }
                }
                else if (buffer[textIndex] == 0xFF)
                {
                    sb.Append(' ');
                }
                textIndex++;
            }
            if (textIndex > textBegin)
            {
                sb.Append(Encoding.UTF8.GetString(buffer, textBegin, textIndex - textBegin - 1));
            }
            p.Text = sb.ToString().Trim();
            if (p.Text.Length > 1 && (p.Text[0] == 31 || p.Text[1] == 65279))
            {
                p.Text = p.Text.Remove(0, 2);
            }
            for (int k = 0; k < p.Text.Length; k++)
            {
                if (p.Text[k] == 65533)
                {
                    p.Text = p.Text.Remove(k, 1).Insert(k, ".");
                }
            }

            index += textLength;
            if (index + 20 >= buffer.Length)
            {
                return null;
            }

            p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
            p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
            p.Text = p.Text.Replace(Environment.NewLine + ">", Environment.NewLine);
            p.Text = p.Text.Replace("\0", string.Empty);

            if (verticalAlignment < 5)
            {
                if (alignment == 1) // left
                {
                    p.Text = "{\\an7}" + p.Text;
                }
                else if (alignment == 0) // right
                {
                    p.Text = "{\\an9}" + p.Text;
                }
                else
                {
                    p.Text = "{\\an8}" + p.Text;
                }
            }
            else if (verticalAlignment < 9)
            {
                if (alignment == 1) // left
                {
                    p.Text = "{\\an4}" + p.Text;
                }
                else if (alignment == 0) // right
                {
                    p.Text = "{\\an6}" + p.Text;
                }
                else
                {
                    p.Text = "{\\an5}" + p.Text;
                }
            }
            else
            {
                if (alignment == 1) // left
                {
                    p.Text = "{\\an1}" + p.Text;
                }
                else if (alignment == 0) // right
                {
                    p.Text = "{\\an3}" + p.Text;
                }
            }

            // Remove all control-characters if any in p.Text.
            p.Text = p.Text.RemoveControlCharactersButWhiteSpace();


            p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
            p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
            p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);

            // Fix italics (basic)
            if (p.Text.StartsWith('<') &&
                !p.Text.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) &&
                !p.Text.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) &&
                !p.Text.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) &&
                !p.Text.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
            {
                p.Text = "<i>" + p.Text.TrimStart('<').Replace(Environment.NewLine + "<", Environment.NewLine) + "</i>";
            }
            else if (p.Text.Contains(Environment.NewLine + "<"))
            {
                p.Text = p.Text.Replace(Environment.NewLine + "<", Environment.NewLine + "<i>") + "</i>";
            }
            return p;
        }

        public void Save(string fileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                // header
                fs.WriteByte(1);
                for (int i = 1; i < 24; i++)
                {
                    fs.WriteByte(0);
                }

                // paragraphs
                var sub = new Subtitle(subtitle);
                sub.Paragraphs.Insert(0, new Paragraph { Text = "-" });

                int number = 0;
                foreach (Paragraph p in sub.Paragraphs)
                {
                    WriteParagraph(fs, p, number, number + 1 == sub.Paragraphs.Count);
                    number++;
                }

                // footer
                fs.WriteByte(0xff);
                for (int i = 0; i < 11; i++)
                {
                    fs.WriteByte(0);
                }

                fs.WriteByte(0x11);
                fs.WriteByte(0);
                byte[] footerBuffer = Encoding.ASCII.GetBytes("dummy end of file.");
                fs.Write(footerBuffer, 0, footerBuffer.Length);
            }
        }

        private void WriteParagraph(FileStream fs, Paragraph p, int number, bool isLast)
        {
            Pac.WriteTimeCode(fs, p.StartTime);
            Pac.WriteTimeCode(fs, p.EndTime);

            byte alignment = 2; // center
            byte verticalAlignment = 0x0a; // bottom
            if (!p.Text.Contains(Environment.NewLine))
            {
                verticalAlignment = 0x0b;
            }

            string text = p.Text;
            if (text.StartsWith("{\\an1}", StringComparison.Ordinal) || text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an7}", StringComparison.Ordinal))
            {
                alignment = 1; // left
            }
            else if (text.StartsWith("{\\an3}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
            {
                alignment = 0; // right
            }
            if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
            {
                verticalAlignment = 0; // top
            }
            else if (text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal))
            {
                verticalAlignment = 5; // center
            }
            if (text.Length >= 6 && text[0] == '{' && text[5] == '}')
            {
                text = text.Remove(0, 6);
            }

            text = Pac.MakePacItalicsAndRemoveOtherTags(text);

            byte[] textBuffer = GetUf8Bytes(text, alignment);

            // write text length
            var length = (UInt16)(textBuffer.Length + 4 + 3);
            fs.Write(BitConverter.GetBytes(length), 0, 2);

            fs.WriteByte(verticalAlignment); // fs.WriteByte(0x0a); // sometimes 0x0b? - this seems to be vertical alignment - 0 to 11
            fs.WriteByte(0x80);
            fs.WriteByte(0x80);
            fs.WriteByte(0x80);
            fs.WriteByte(0xfe);
            fs.WriteByte(alignment); //2=centered, 1=left aligned, 0=right aligned, 09=Fount2 (large font),
            //55=safe area override (too long line), 0A=Fount2 + centered, 06=centered + safe area override
            fs.WriteByte(0x03);

            fs.Write(textBuffer, 0, textBuffer.Length);

            if (!isLast)
            {
                fs.WriteByte(0);
                fs.WriteByte((byte)((number + 1) % 256));
                fs.WriteByte((byte)((number + 1) / 256));
                fs.WriteByte(0x60);
            }
        }

        private static byte[] GetUf8Bytes(string text, byte alignment)
        {
            var result = new List<byte>();
            bool firstLine = true;
            var lines = text.SplitToLines();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (!firstLine)
                {
                    result.Add(0xfe);
                    result.Add(alignment);
                    result.Add(3);

                    result.Add(0x1F); // utf8 BOM
                    result.Add(0xEF);
                    result.Add(0xBB);
                    result.Add(0xBF);
                }

                string s = line;
                for (int index = 0; index < s.Length; index++)
                {
                    var ch = s[index];
                    if (ch == '.') // 0x2e
                    {
                        result.Add(0xff); // period
                    }
                    else
                    {
                        foreach (var b in Encoding.UTF8.GetBytes(ch.ToString()))
                        {
                            result.Add(b);
                        }
                    }
                }


                firstLine = false;
            }
            result.Add(0x2e);
            return result.ToArray();
        }

    }
}
