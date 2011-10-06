using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class Spt : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".spt"; }
        }

        public override string Name
        {
            get { return "spt"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public void Save(string fileName, Subtitle subtitle)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            // header
            fs.WriteByte(1);
            for (int i = 1; i < 23; i++)
                fs.WriteByte(0);
            fs.WriteByte(0x60);

            // paragraphs
            int number = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                WriteParagraph(fs, p, number);
                number++;
            }

            // footer
            fs.WriteByte(0xff);
            for (int i = 0; i < 11; i++)
                fs.WriteByte(0);
            fs.WriteByte(0x11);
            byte[] footerBuffer = System.Text.Encoding.ASCII.GetBytes("dummy end of file");
            fs.Write(footerBuffer, 0, footerBuffer.Length);

            fs.Close();
        }

        private void WriteParagraph(FileStream fs, Paragraph p, int number)
        {
            WriteTimeCode(fs, p.StartTime);
            WriteTimeCode(fs, p.EndTime);

            string text = p.Text;
            if (Utilities.CountTagInText(text, Environment.NewLine)> 1)
                text = Utilities.AutoBreakLine(p.Text);

            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int textLengthFirstLine = 0;
            int textLengthSecondLine = 0;
            if (lines.Length > 0)
            {
                textLengthFirstLine = lines[0].Length;
                if (lines.Length > 1)
                    textLengthSecondLine = lines[1].Length;
            }
        }

        private void WriteTimeCode(FileStream fs, TimeCode timeCode)
        {
            // write 8 bytes time code
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length > 100 && fi.Length < 1024000) // not too small or too big
                {
                    byte[] buffer = File.ReadAllBytes(fileName);

                    if (buffer[00] > 10 &&
                        buffer[01] == 0 &&
                        fileName.ToLower().EndsWith(".spt"))
                        return true;
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
            byte[] buffer = File.ReadAllBytes(fileName);

            int index = buffer[0]; // go to first subtitle paragraph
            while (index < buffer.Length)
            {
                Paragraph p = GetPacParagraph(ref index, buffer);
                if (p != null)
                    subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
        }

        private Paragraph GetPacParagraph(ref int index, byte[] buffer)
        {
            if (index + 16 + 20 + 4 >= buffer.Length)
                return null;

            int textLengthFirstLine = buffer[index + 16 + 20];
            int textLengthSecondLine = buffer[index + 16 + 20 + 4];

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

        private TimeCode GetTimeCode(string timeCode)
        {
            int hour = int.Parse(timeCode.Substring(0, 2));
            int minute = int.Parse(timeCode.Substring(2, 2));
            int second = int.Parse(timeCode.Substring(4, 2));
            int frames = int.Parse(timeCode.Substring(6, 2));

            int milliseconds = (int)((1000 / Configuration.Settings.General.CurrentFrameRate) * frames);
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(hour, minute, second, milliseconds);
        }

    }
}