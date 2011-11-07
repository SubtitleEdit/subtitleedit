using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Scantitling890 : SubtitleFormat
    {
        private string _fileName = string.Empty;
        private int _codePage = -1;

        public override string Extension
        {
            get { return ".890"; }
        }

        public override string Name
        {
            get { return "Scantitling 890"; }
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
            _fileName = fileName;

            // header
            fs.WriteByte(1);
            for (int i = 1; i < 23; i++)
                fs.WriteByte(0);
            fs.WriteByte(0x60);

            // paragraphs
            int number = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                WriteParagraph(fs, p, number, number + 1 == subtitle.Paragraphs.Count);
                number++;
            }

            // footer
            fs.WriteByte(0xff);
            for (int i = 0; i < 11; i++)
                fs.WriteByte(0);
            fs.WriteByte(0x11);
            fs.WriteByte(0);
            byte[] footerBuffer = System.Text.Encoding.ASCII.GetBytes("dummy end of file");
            fs.Write(footerBuffer, 0, footerBuffer.Length);

            fs.Close();
        }

        private void WriteParagraph(FileStream fs, Paragraph p, int number, bool isLast)
        {
            WriteTimeCode(fs, p.StartTime);
            WriteTimeCode(fs, p.EndTime);

            if (_codePage == -1)
                GetCodePage(null, 0, 0);

            string text = Utilities.RemoveHtmlTags(p.Text);
            text = text.Replace(Environment.NewLine, System.Text.Encoding.Default.GetString(new byte[] { 0xfe, 0x02, 0x03 })); // fix line breaks
            //            text = text.Replace(Environment.NewLine, System.Text.Encoding.Default.GetString(new byte[] { 0xfe, 0x0a, 0x03 })); // fix line breaks

            Encoding encoding = GetEncoding(_codePage);
            byte[] textBuffer;
            textBuffer = encoding.GetBytes(text);

            byte length = (byte)(textBuffer.Length + 4);
            fs.WriteByte(length);

            // TODO: What is this?
            fs.WriteByte(0);
            fs.WriteByte(0x0a); // sometimes 0x0b?
            fs.WriteByte(0xfe);
            fs.WriteByte(0x02); //2=centered, 1=left aligned, 0=right aligned,
            fs.WriteByte(0x03);


            fs.Write(textBuffer, 0, textBuffer.Length);

            if (!isLast)
            {
                fs.WriteByte(0);
                fs.WriteByte((byte)(number + 1));
                fs.WriteByte(0);
                fs.WriteByte(0x60);
            }
        }

        private void WriteTimeCode(FileStream fs, TimeCode timeCode)
        {
            // write four bytes time code
            string highPart = string.Format("{0:00}", timeCode.Hours) + string.Format("{0:00}", timeCode.Minutes);
            if (timeCode.Hours == 7 && timeCode.Minutes == 35)
                highPart = "065535";

            byte frames = (byte)(timeCode.Milliseconds / (1000.0 / Configuration.Settings.General.CurrentFrameRate));
            string lowPart = string.Format("{0:00}", timeCode.Seconds) + string.Format("{0:00}", frames);

            int high = int.Parse(highPart);
            if (high < 256)
            {
                fs.WriteByte((byte)high);
                fs.WriteByte(0);
            }
            else
            {
                fs.WriteByte((byte)(high % 256));
                fs.WriteByte((byte)(high / 256));
            }

            int low = int.Parse(lowPart);
            if (low < 256)
            {
                fs.WriteByte((byte)low);
                fs.WriteByte(0);
            }
            else
            {
                fs.WriteByte((byte)(low % 256));
                fs.WriteByte((byte)(low / 256));
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

                    if (buffer[00] == 0 && // These bytes seems to be PAC files... TODO: Verify!
                        buffer[01] == 0 &&
                        buffer[02] == 0x31 &&
                        buffer[03] == 0x32 &&
                        buffer[04] == 0x33 &&
                        buffer[05] == 0x34 &&
                        buffer[06] == 0x35 &&
                        buffer[07] == 0x36 &&
                        buffer[08] == 0x37 &&
                        buffer[09] == 0x38 &&
                        buffer[10] == 0x20 &&
                        buffer[11] == 0x20 &&
                        buffer[12] == 0x20 &&
                        buffer[13] == 0x20 &&
                        buffer[14] == 0x20 &&
                        buffer[15] == 0x20 &&
                        buffer[16] == 0x20 &&
                        buffer[17] == 0x20 &&
                        buffer[18] == 0x20 &&
                        buffer[19] == 0x20 &&
                        buffer[20] == 0x20 &&
                        buffer[21] == 0x20 &&
                        buffer[22] == 0x20 &&
                        buffer[23] == 0x20 &&
                        fileName.ToLower().EndsWith(".890"))
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
            _fileName = fileName;
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
            while (index < 15)
            {
                index++;
            }
            bool con = true;
            while (con)
            {
                index++;
                if (index + 20 >= buffer.Length)
                    return null;

                if (buffer[index] == 0xFE && (buffer[index - 15] == 0x60 || buffer[index - 15] == 0x61))
                    con = false;
                if (buffer[index] == 0xFE && (buffer[index - 12] == 0x60 || buffer[index - 12] == 0x61))
                    con = false;
            }

            int FEIndex = index;
            int endDelimiter = 0x00;

            if (_codePage == -1)
                GetCodePage(buffer, index, endDelimiter);

            StringBuilder sb = new StringBuilder();
            index = FEIndex + 3;
            while (index < buffer.Length && buffer[index] != endDelimiter)
            {
                if (buffer[index] == 0xFF)
                {
                    sb.Append(" ");
                }
                else if (buffer[index] == 0xFE)
                {
                    sb.AppendLine();
                    index += 2;
                }
                else
                {
                    sb.Append(GetEncoding(_codePage).GetString(buffer, index, 1));
                }

                index++;
            }
            if (index + 20 >= buffer.Length)
                return null;

            Paragraph p = new Paragraph();
            p.Text = sb.ToString();
            if (_codePage == 3)
                p.Text = FixEnglishTextInArabic(p.Text);
            int timeStartIndex = FEIndex - 15;
            if (buffer[timeStartIndex] == 0x60)
            {
                p.StartTime = GetTimeCode(timeStartIndex + 1, buffer);
                p.EndTime = GetTimeCode(timeStartIndex + 5, buffer);
            }
            else if (buffer[timeStartIndex + 3] == 0x60)
            {
                timeStartIndex += 3;
                p.StartTime = GetTimeCode(timeStartIndex + 1, buffer);
                p.EndTime = GetTimeCode(timeStartIndex + 5, buffer);
            }
            return p;
        }

        public static string FixEnglishTextInArabic(string text)
        {
            var sb = new StringBuilder();
            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string s = line.Trim();
                for (int i = 0; i < s.Length; i++)
                {
                    if (s.Substring(i, 1) == ")")
                        s = s.Remove(i, 1).Insert(i, "(");
                    else if (s.Substring(i, 1) == "(")
                        s = s.Remove(i, 1).Insert(i, ")");
                }

                bool numbersOn = false;
                string numbers = string.Empty;
                string reverseChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                for (int i = 0; i < s.Length; i++)
                {
                    if (numbersOn && reverseChars.Contains(s.Substring(i, 1)))
                    {
                        numbers = s.Substring(i, 1) + numbers;
                    }
                    else if (numbersOn)
                    {
                        numbersOn = false;
                        s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers.ToString());
                        numbers = string.Empty;
                    }
                    else if (reverseChars.Contains(s.Substring(i, 1)))
                    {
                        numbers = s.Substring(i, 1) + numbers;
                        numbersOn = true;
                    }
                }
                if (numbersOn)
                {
                    int i = s.Length;
                    s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers.ToString());
                    numbers = string.Empty;
                }

                sb.AppendLine(s);
            }
            return sb.ToString().Trim();
        }

        public static Encoding GetEncoding(int codePage)
        {
            switch (codePage)
            {
                case 0: // Latin
                    return Encoding.GetEncoding("iso-8859-1");
                case 1: // Greek
                    return Encoding.GetEncoding("iso-8859-7");
                case 2: // Latin Czech
                    return Encoding.GetEncoding("iso-8859-2");
                case 3: // Arabic
                    return Encoding.GetEncoding("iso-8859-6");
                case 4: // Hebrew
                    return Encoding.GetEncoding("iso-8859-8");
                case 5: // Thai
                    return Encoding.GetEncoding("windows-874");
                case 6: // Cyrillic
                    return Encoding.GetEncoding("iso-8859-5");
                default: return Encoding.Default;
            }
        }

        private void GetCodePage(byte[] buffer, int index, int endDelimiter)
        {
            byte[] previewBuffer = null;

            if (buffer != null)
            {
                byte[] textSample = new byte[200];
                int textIndex = 0;
                while (index < buffer.Length && buffer[index] != endDelimiter)
                {
                    if (buffer[index] == 0xFF)
                    {
                        textSample[textIndex++] = 32; // space
                    }
                    else if (buffer[index] == 0xFE)
                    {
                        if (textIndex < textSample.Length - 3)
                        {
                            textSample[textIndex++] = buffer[index];
                            textSample[textIndex++] = buffer[index + 1];
                            textSample[textIndex++] = buffer[index + 2];
                        }
                        index += 3;
                    }
                    if (textIndex < textSample.Length - 1)
                        textSample[textIndex++] = buffer[index];
                    index++;
                }
                previewBuffer = new byte[textIndex];
                for (int i = 0; i < textIndex; i++)
                    previewBuffer[i] = textSample[i];
            }
        }     

        private TimeCode GetTimeCode(int timeCodeIndex, byte[] buffer)
        {
            if (timeCodeIndex > 0)
            {
                string highPart = string.Format("{0:000000}", buffer[timeCodeIndex] + buffer[timeCodeIndex + 1] * 256);
                string lowPart = string.Format("{0:000000}", buffer[timeCodeIndex + 2] + buffer[timeCodeIndex + 3] * 256);

                int hours = int.Parse(highPart.Substring(0, 4));
                int minutes = int.Parse(highPart.Substring(4, 2));
                int second = int.Parse(lowPart.Substring(2, 2));
                int frames = int.Parse(lowPart.Substring(4, 2));

                int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * frames);
                if (milliseconds > 999)
                    milliseconds = 999;

                return new TimeCode(hours, minutes, second, milliseconds);
            }
            return new TimeCode(0, 0, 0, 0);
        }

    }
}