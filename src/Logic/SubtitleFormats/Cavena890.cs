using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class Cavena890 : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".890"; }
        }

        public override string Name
        {
            get { return "Cavena 890"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return false; }
        }

        public void Save(string fileName, Subtitle subtitle)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            //header
            for (int i=0; i<22; i++)
                fs.WriteByte(0);

            byte[] buffer = ASCIIEncoding.ASCII.GetBytes("Subtitle Edit");
            fs.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 18-buffer.Length; i++)
                fs.WriteByte(0);

            string title = Path.GetFileNameWithoutExtension(fileName);
            if (title.Length > 25)
                title = title.Substring(0, 25);
            buffer = ASCIIEncoding.ASCII.GetBytes(title);
            fs.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 28 - title.Length; i++)
                fs.WriteByte(0);

            buffer = ASCIIEncoding.ASCII.GetBytes("NV");
            fs.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 66 - buffer.Length; i++)
                fs.WriteByte(0);

            buffer = new byte[] { 0xA0, 0x05, 0x04, 0x03, 0x06, 0x06, 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, 0x07, 0x07, 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, 0x4E, 0xBF, 0x02, 0x45, 0x54, 0x4C, 0x35, 0x30, 0x35, 0x56, 0x46, 0x4F, 0x4E, 0x54, 0x4C, 0x2E, 0x56, 0x44, 0x46, 0x4F, 0x4E, 0x54, 0x4C, 0x2E, 0x44,
                0x01, 0x07, 0x01, 0x08, 0x00, 0xBF, 0x02, 0xBF, 0x02, 0x00, 0x00, 0x0D, 0xBF, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x46, 0x4F, 0x4E, 0x54, 0x4C, 0x2E, 0x56, 0x14, 0x56, 0x31, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x54, 0x44 };
            fs.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 92; i++)
                fs.WriteByte(0);


            // paragraphs
            int number = 16;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // number
                fs.WriteByte((byte)(number / 256));
                fs.WriteByte((byte)(number % 256));

                WriteTime(fs, p.StartTime);
                WriteTime(fs, p.EndTime);

                buffer = new byte[] { 0x14, 00, 00, 00, 00, 00, 00, 0x16 };
                fs.Write(buffer, 0, buffer.Length);

                WriteText(fs, p.Text, p == subtitle.Paragraphs[subtitle.Paragraphs.Count-1]);

                number += 16;
            }
            fs.Close();
        }

        private void WriteText(FileStream fs, string text, bool isLast)
        {
            string line1 = string.Empty;
            string line2 = string.Empty;
            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 2)
                lines = Utilities.AutoBreakLine(text).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
                line1 = lines[0];
            if (lines.Length > 1)
                line2 = lines[1];

            var buffer = GetTextAsBytes(line1);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00, 00, 00 };
            fs.Write(buffer, 0, buffer.Length);

            buffer = GetTextAsBytes(line2);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00 };
            if (!isLast)
                fs.Write(buffer, 0, buffer.Length);
        }

        private byte[] GetTextAsBytes(string text)
        {
            byte[] buffer = new byte[51];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0x7F;

            var encoding = Encoding.Default;
            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                string current = text.Substring(i, 1);
                if (index < 50)
                {
                    if (current == "æ")
                        buffer[index] = 0x1B;
                    else if (current == "ø")
                        buffer[index] = 0x1C;
                    else if (current == "å")
                        buffer[index] = 0x1D;
                    else if (current == "Æ")
                        buffer[index] = 0x5B;
                    else if (current == "Ø")
                        buffer[index] = 0x5C;
                    else if (current == "Å")
                        buffer[index] = 0x5D;
                    else if (current == "Ä")
                    {
                        buffer[index] = 0x86;
                        buffer[index + 1] = 0x41;
                    }
                    else if (current == "ä")
                    {
                        buffer[index] = 0x86;
                        buffer[index + 1] = 0x61;
                    }
                    else if (current == "Ö")
                    {
                        buffer[index] = 0x86;
                        buffer[index + 1] = 0x4F;
                    }
                    else if (current == "ö")
                    {
                        buffer[index] = 0x86;
                        buffer[index + 1] = 0x6F;
                    }
                    else if (current == "å")
                    {
                        buffer[index] = 0x8C;
                        buffer[index + 1] = 0x61;
                    }
                    else if (current == "Å")
                    {
                        buffer[index] = 0x8C;
                        buffer[index + 1] = 0x41;
                    }
                    else if (text.Substring(i, 3) == "<i>")
                        buffer[index] = 0x88;
                    else if (text.Substring(i, 4) == "</i>")
                        buffer[index] = 0x98;
                    else
                        buffer[index] = encoding.GetBytes(current)[0];
                }
            }

            return buffer;
        }

        private void WriteTime(FileStream fs, TimeCode timeCode)
        {
            double totalMilliseconds = timeCode.TotalMilliseconds + TimeSpan.FromHours(10).TotalMilliseconds; // +10 hours
            int frames = (int)Math.Round(totalMilliseconds / (1000.0 /Configuration.Settings.General.CurrentFrameRate));
            fs.WriteByte((byte)(frames / 256 / 256));
            fs.WriteByte((byte)(frames / 256));
            fs.WriteByte((byte)(frames % 256));
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length > 1024 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.EndsWith(".890"))
                    {
                        byte[] buffer = File.ReadAllBytes(fileName);
                        for (int i = 0; i < buffer.Length - 20; i++)
                        {
                            if (buffer[i + 0] == 0x7F &&
                                buffer[i + 1] == 0x7F &&
                                buffer[i + 2] == 0x7F &&
                                buffer[i + 3] == 0 &&
                                buffer[i + 4] == 0 &&
                                buffer[i + 5] == 0)
                                return true;
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
            const int TextLength = 51;

            subtitle.Paragraphs.Clear();
            byte[] buffer = File.ReadAllBytes(fileName);

            int i = 455;
            while (i < buffer.Length - 20)
            {
                if (i == 455 ||
                    (buffer[i - 3] == 0x7F &&
                     buffer[i - 2] == 0x7F &&
                     buffer[i - 1] == 0x7F &&
                     buffer[i + 0] == 0 &&
                     buffer[i + 1] == 0 &&
                     buffer[i + 2] == 0))
                {
                    int start = i - TextLength;

                    //int number = buffer[start - 16] * 256 + buffer[start - 15];

                    double startFrame = buffer[start - 14] * 256 * 256 + buffer[start - 13] * 256 + buffer[start - 12];
                    double endFrame = buffer[start - 11] * 256 * 256 + buffer[start - 10] * 256 + buffer[start - 9];


                    string line1 = FixText(buffer, start, TextLength);
                    string line2 = FixText(buffer, start + TextLength + 6, TextLength);

                    Paragraph p = new Paragraph();
                    p.Text = (line1 + Environment.NewLine + line2).Trim();
                    p.StartTime.TotalMilliseconds = ((1000.0 / Configuration.Settings.General.CurrentFrameRate) * startFrame);
                    p.EndTime.TotalMilliseconds = ((1000.0 / Configuration.Settings.General.CurrentFrameRate) * endFrame);

                    subtitle.Paragraphs.Add(p);
                    i += TextLength * 2;
                }
                else
                {
                    i++;
                }
            }

            subtitle.Renumber(1);
        }

        private static string FixText(byte[] buffer, int start, int textLength)
        {
            var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")

            string text = encoding.GetString(buffer, start, textLength);

            text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
            text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?

            text = text.Replace(encoding.GetString(new byte[] { 0x1B }), "æ");
            text = text.Replace(encoding.GetString(new byte[] { 0x1C }), "ø");
            text = text.Replace(encoding.GetString(new byte[] { 0x1D }), "å");
            text = text.Replace(encoding.GetString(new byte[] { 0x1E }), "Æ");

            text = text.Replace(encoding.GetString(new byte[] { 0x5B }), "Æ");
            text = text.Replace(encoding.GetString(new byte[] { 0x5C }), "Ø");
            text = text.Replace(encoding.GetString(new byte[] { 0x5D }), "Å");


            text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x41 }), "Ä");
            text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x61 }), "ä");
            text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x4F }), "Ö");
            text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x6F }), "ö");

            text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x61 }), "å");
            text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x41 }), "Å");

            text = text.Replace(encoding.GetString(new byte[] { 0x88 }), "<i>");
            text = text.Replace(encoding.GetString(new byte[] { 0x98 }), "</i>");
            if (text.Contains("<i></i>"))
                text = text.Replace("<i></i>", "<i>");
            if (text.Contains("<i>") && !text.Contains("</i>"))
                text += "</i>";

            return text;
        }

    }
}