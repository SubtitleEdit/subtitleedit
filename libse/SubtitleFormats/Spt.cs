using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Spt : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".spt"; }
        }

        public const string NameOfFormat = "spt";

        public override string Name
        {
            get { return NameOfFormat; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public static void Save(string fileName, Subtitle subtitle)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            // header
            fs.WriteByte(1);
            for (int i = 1; i < 23; i++)
                fs.WriteByte(0);
            fs.WriteByte(0x60);

            // paragraphs
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                WriteParagraph(p);
            }

            // footer
            fs.WriteByte(0xff);
            for (int i = 0; i < 11; i++)
                fs.WriteByte(0);
            fs.WriteByte(0x11);
            byte[] footerBuffer = Encoding.ASCII.GetBytes("dummy end of file");
            fs.Write(footerBuffer, 0, footerBuffer.Length);

            fs.Close();
        }

        private static void WriteParagraph(Paragraph p)
        {
            WriteTimeCode();
            WriteTimeCode();

            string text = p.Text;
            if (Utilities.GetNumberOfLines(text) > 2)
                text = Utilities.AutoBreakLine(p.Text);

            var lines = text.SplitToLines();
            int textLengthFirstLine = 0;
            int textLengthSecondLine = 0;
            if (lines.Length > 0)
            {
                textLengthFirstLine = lines[0].Length;
                if (lines.Length > 1)
                    textLengthSecondLine = lines[1].Length;
            }
        }

        private static void WriteTimeCode()
        {
            // write 8 bytes time code
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    FileInfo fi = new FileInfo(fileName);
                    if (fi.Length > 100 && fi.Length < 1024000) // not too small or too big
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

                        if (buffer[00] > 10 &&
                            buffer[01] == 0 &&
                            fileName.EndsWith(".spt", StringComparison.OrdinalIgnoreCase))
                            return true;
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
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int index = buffer[0]; // go to first subtitle paragraph
            while (index < buffer.Length)
            {
                Paragraph p = GetSptParagraph(ref index, buffer);
                if (p != null)
                    subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

        private Paragraph GetSptParagraph(ref int index, byte[] buffer)
        {
            if (index + 16 + 20 + 4 >= buffer.Length)
            {
                index = index + 16 + 20 + 4;
                return null;
            }

            int textLengthFirstLine = buffer[index + 16 + 20];
            int textLengthSecondLine = buffer[index + 16 + 20 + 4];

            if (textLengthFirstLine == 0 && textLengthSecondLine == 0)
            {
                index += (16 + 20 + 16);
                _errorCount++;
                return null;
            }

            try
            {
                var p = new Paragraph();
                p.StartTime = GetTimeCode(Encoding.Default.GetString(buffer, index, 8));
                p.EndTime = GetTimeCode(Encoding.Default.GetString(buffer, index + 8, 8));

                p.Text = Encoding.Default.GetString(buffer, index + 16 + 20 + 16, textLengthFirstLine);

                if (textLengthSecondLine > 0)
                    p.Text += Environment.NewLine + Encoding.Default.GetString(buffer, index + 16 + 20 + 16 + textLengthFirstLine, textLengthSecondLine);

                index += (16 + 20 + 16 + textLengthFirstLine + textLengthSecondLine);
                return p;
            }
            catch
            {
                index += (16 + 20 + 16 + textLengthFirstLine + textLengthSecondLine);
                _errorCount++;
                return null;
            }
        }

        private static TimeCode GetTimeCode(string timeCode)
        {
            int hour = int.Parse(timeCode.Substring(0, 2));
            int minute = int.Parse(timeCode.Substring(2, 2));
            int second = int.Parse(timeCode.Substring(4, 2));
            int frames = int.Parse(timeCode.Substring(6, 2));

            int milliseconds = (int)Math.Round(1000.0 / Configuration.Settings.General.CurrentFrameRate * frames);
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(hour, minute, second, milliseconds);
        }

    }
}
