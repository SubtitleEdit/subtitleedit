using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// The PAC format was developed by Screen Electronics
// The PAC format save the contents, time code, position, justification, and italicization of each subtitle. The choice of font is not saved.

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// EBU Subtitling data exchange format
    /// </summary>
    public class Pac : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".pac"; }
        }

        public override string Name
        {
            get { return "PAC (Screen Electronics)"; }
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
            for (int i=1; i<23; i++)
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
            for (int i=0; i<11; i++)
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

            string text = Utilities.RemoveHtmlTags(p.Text);
            text = text.Replace(Environment.NewLine, System.Text.Encoding.Default.GetString(new byte[] { 0xfe, 0x0a, 0x03 })); // fix line breaks

            byte length = (byte)(text.Length + 7);
            fs.WriteByte(length);

            // TODO: What is this?
            fs.WriteByte(0);
            fs.WriteByte(0x0a);
            fs.WriteByte(0x80);
            fs.WriteByte(0x80);
            fs.WriteByte(0x80);
            fs.WriteByte(0xfe);
            fs.WriteByte(0x0a);
            fs.WriteByte(0x03);

            byte[] textBuffer = System.Text.Encoding.Default.GetBytes(text);
            fs.Write(textBuffer, 0, textBuffer.Length);

            fs.WriteByte(0xff);
            fs.WriteByte((byte)number);
            fs.WriteByte(0);
            fs.WriteByte(0x60);
        }

        private void WriteTimeCode(FileStream fs, TimeCode timeCode)
        {
            // write four bytes time code
            fs.WriteByte((byte)timeCode.Hours);

            byte frames = (byte)(timeCode.Milliseconds / 40);
            string numberString = string.Format("{0:00}", (byte)timeCode.Minutes) +
                                  string.Format("{0:00}", (byte)timeCode.Seconds) +
                                  string.Format("{0:00}", frames);

            UInt32 number = UInt32.Parse(numberString);
            byte[] buffer = BitConverter.GetBytes(number);

            if (buffer[2] == 0 && buffer[1] == 0)
            {
                fs.WriteByte(0);
                fs.WriteByte(0);
                fs.WriteByte(buffer[0]);
            }
            else if (buffer[2] == 0)
            {
                fs.WriteByte(0);
                fs.WriteByte(buffer[0]);
                fs.WriteByte(buffer[1]);
            }
            else
            {
                fs.WriteByte(buffer[0]);
                fs.WriteByte(buffer[1]);
                fs.WriteByte(buffer[2]);
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length > 100 && fi.Length < 1024000) // not too small or too big
                {
                    byte[] buffer = File.ReadAllBytes(fileName);

                    if (buffer[00] == 1 && // These bytes seems to be PAC files... TODO: Verify!
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
                        buffer[21] < 10 &&
                        buffer[22] == 0 &&
                        buffer[23] == 0x60 &&
                        fileName.ToLower().EndsWith(".pac"))
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

            int index = 0;
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
            while (index < buffer.Length && buffer[index] != 0xFE)
            {
                index++;
            }

            if (index + 20 >= buffer.Length)
                return null;

            int FEIndex = index;
            int endDelimiter1 = 0x00;
            int endDelimiter2 = 0xff;

            StringBuilder sb = new StringBuilder();
            index = FEIndex + 3;
            while (index < buffer.Length && buffer[index] != endDelimiter1 && buffer[index] != endDelimiter2)
            {
                if (buffer[index] == 0xFE)
                {
                    sb.AppendLine();
                    index += 3;
                }
                sb.Append(Encoding.Default.GetString(buffer, index, 1));
                index++;
            }
            if (index + 20 >= buffer.Length)
                return null;

            Paragraph p = new Paragraph();
            p.Text = sb.ToString();
            int timeStartIndex = FEIndex - 8;
            while (timeStartIndex > FEIndex - 20 && buffer[timeStartIndex] != 0x60)
                timeStartIndex--;
            if (buffer[timeStartIndex] == 0x60)
            {
                p.StartTime = GetTimeCode(timeStartIndex+1, buffer);
                p.EndTime = GetTimeCode(timeStartIndex + 5, buffer);
            }
            return p;
        }

        private TimeCode GetTimeCode(int timeCodeIndex, byte[] buffer)
        {
            if (timeCodeIndex > 0)
            {
                int hour = buffer[timeCodeIndex];
                if (hour > 5)
                    hour = 0;

                int timecode1 = buffer[timeCodeIndex + 1];
                int timecode2 = buffer[timeCodeIndex + 2];
                int timecode3 = buffer[timeCodeIndex + 3];

                Int64 number = 0;
                if (timecode1 > 0)
                    number = timecode3 + (timecode2 * 256) + (timecode3 * 256 * 256);
                else if (timecode2 > 0)
                    number = timecode2 + (timecode3* 256);
                else
                    number = timecode3;
                string numberString = string.Format("{0:00000000}", number);

                int minute = int.Parse(numberString.Substring(2, 2));
                int second = int.Parse(numberString.Substring(4, 2));
                int frames = int.Parse(numberString.Substring(6, 2));

                int milliseconds = (int)((1000 / 24.0) * frames);
                if (milliseconds > 999)
                    milliseconds = 999;

                return new TimeCode(hour, minute, second, milliseconds);
            }
            return new TimeCode(0, 0, 0, 0);
        }

    }
}