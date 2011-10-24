using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Forms;

// The PAC format was developed by Screen Electronics
// The PAC format save the contents, time code, position, justification, and italicization of each subtitle. The choice of font is not saved.

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

    public class Pac : SubtitleFormat
    {
        static List<int> _latinCodes = new List<int> {
            0xe161, // å
            0xe141, // Å
            0x7c, // æ
            0x7d, // ø
            0x5c, // Æ
            0x5d, // Ø
        };

        static List<string> _latinLetters = new List<string> {
            "å",
            "Å",
            "æ",
            "ø",
            "Æ",
            "Ø",
        };


        static List<int> _arabicCodes = new List<int> {
            0xe081, //=أ
            0xe09b, //=ؤ
            0xe09c, //=ئ
            0xe181, //=إ
            0xe281, //=آ
            0xe781, //=اً
            0x80,
            0x81, //=ا
            0x82, //=ب
            0x83, //=ت
            0x84, //=ث
            0x85, //=ج
            0x86, //=ح
            0x87, //=خ
            0x88, //=د
            0x89, //=ذ
            0x8A, //=ر
            0x8b, //=ز
            0x8c, //=س
            0x8d, //=ش
            0x8e, //=ص
            0x8f, //=ض
            0x90, //=ظ
            0x91, //=ط
            0x92, //=ع
            0x93, //=غ
            0x94, //=ف
            0x95, //=ق
            0x96, //=ك
            0x97, //=ل
            0x98, //=م
            0x99, //=ن
            0x9A, //=ه
            0x9b, //=و
            0x9c, //=ى
            0x9d, //=ة
            0x9f, //=ي
            0xa0, //=ء
        };

        static List<string> _arabicLetters = new List<string> {
            "أ",
            "ؤ",
            "ئ",
            "إ",
            "آ",
            "اً",
            "ـ",
            "ا",
            "ب",
            "ت",
            "ث",
            "ج",
            "ح",
            "خ",
            "د",
            "ذ",
            "ر",
            "ز",
            "س",
            "ش",
            "ص",
            "ض",
            "ظ",
            "ط",
            "ع",
            "غ",
            "ف",
            "ق",
            "ك",
            "ل",
            "م",
            "ن",
            "ه",
            "و",
            "ى",
            "ة",
            "ي",
            "ء",
        };

        private string _fileName = string.Empty;
        private int _codePage = -1;

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
            _fileName = fileName; 

            // header
            fs.WriteByte(1);
            for (int i=1; i<23; i++)
                fs.WriteByte(0);
            fs.WriteByte(0x60);

            // paragraphs
            int number = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                WriteParagraph(fs, p, number, number +1 == subtitle.Paragraphs.Count);
                number++;
            }

            // footer
            fs.WriteByte(0xff);
            for (int i=0; i<11; i++)
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
            if (_codePage == 3)
                textBuffer = GetArabicBytes(FixEnglishTextInArabic(text));
            else if (_codePage == 0)
                textBuffer = GetLatinBytes(encoding, text);
            else
                textBuffer = encoding.GetBytes(text);


            byte length = (byte)(textBuffer.Length+4);
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
                        //buffer[21] < 10 && // start from number
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
                if (index +20 >= buffer.Length)
                    return null;

                if (buffer[index] == 0xFE && (buffer[index - 15] == 0x60 ||buffer[index - 15] == 0x61))
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
                else if (_codePage == 0)
                    sb.Append(GetLatinString(GetEncoding(_codePage), buffer, ref index));
                else if (_codePage == 3)
                    sb.Append(GetArabicString(buffer, ref index));
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
            else if (buffer[timeStartIndex+3] == 0x60)
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

        private void GetCodePage( byte[] buffer, int index, int endDelimiter)
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
                            textSample[textIndex++] = buffer[index+1];
                            textSample[textIndex++] = buffer[index+2];
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

            var pacEncoding = new PacEncoding(previewBuffer, _fileName);
            if (pacEncoding.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _codePage = pacEncoding.CodePageIndex;
                Configuration.Settings.General.LastPacCodePage = _codePage;
            }
            else
            {
                _codePage = -2;
            }
        }

        private byte[] GetLatinBytes(Encoding encoding, string text)
        {
            int i = 0;
            byte[] buffer = new byte[text.Length * 2];
            int extra = 0;
            while (i < text.Length)
            {
                string letter = text.Substring(i, 1);
                int idx = _latinLetters.IndexOf(letter);
                if (idx >= 0)
                {
                    int byteValue = _latinCodes[idx];
                    if (byteValue < 256)
                    {
                        buffer[i + extra] = (byte)byteValue;
                    }
                    else
                    {
                        int high = byteValue / 256;
                        int low = byteValue % 256;

                        buffer[i + extra] = (byte)high;
                        extra++;
                        buffer[i + extra] = (byte)low;
                    }
                }
                else
                {
                    var values = encoding.GetBytes(letter);
                    for (int k = 0; k < values.Length; k++)
                    {
                        byte v = values[k];
                        if (k > 0)
                            extra++;
                        buffer[i + extra] = v;
                    }
                }
                i++;
            }

            byte[] result = new byte[i + extra];
            for (int j = 0; j < i + extra; j++)
                result[j] = buffer[j];
            return result;
        }


        private byte[] GetArabicBytes(string text)
        {
            int i = 0;
            byte[] buffer = new byte[text.Length * 2];
            int extra = 0;
            while (i < text.Length)
            {
                bool doubleCharacter = false;
                string letter = string.Empty;
                int idx = -1;
                if (i + 1 < text.Length)
                {
                     letter = text.Substring(i, 2);
                     idx = _arabicLetters.IndexOf(letter);
                     if (idx >= 0)
                         doubleCharacter = true;
                }
                if (idx < 0)
                {
                    letter = text.Substring(i, 1);
                    idx = _arabicLetters.IndexOf(letter);
                }
                if (idx >= 0)
                {
                    int byteValue = _arabicCodes[idx];
                    if (byteValue < 256)
                    {
                        buffer[i + extra] = (byte)byteValue;
                    }
                    else
                    {
                        int high = byteValue / 256;
                        int low = byteValue % 256;
                        buffer[i + extra] = (byte)high;
                        if (doubleCharacter)
                        {
                            i++;
                            doubleCharacter = false;
                        }
                        else
                        {
                            extra++;
                        }
                        buffer[i + extra] = (byte)low;
                    }
                }
                else
                {
                    var values = Encoding.Default.GetBytes(letter);
                    for (int k = 0; k < values.Length; k++)
                    {
                        byte v = values[k];
                        if (k > 0)
                            extra++;
                        buffer[i + extra] = v;
                    }
                }
                i++;
                if (doubleCharacter)
                    i++;
            }

            byte[] result = new byte[i+extra];
            for (int j = 0; j < i + extra; j++)
                result[j] = buffer[j];
            return result;
        }

        public static string GetArabicString(byte[] buffer, ref int index)
        {
            byte b = buffer[index];
            if (b >= 0x20 && b < 0x70)
                return Encoding.ASCII.GetString(buffer, index, 1);

            int idx = _arabicCodes.IndexOf(b);
            if (idx >= 0)
                return _arabicLetters[idx];

            if (buffer.Length > index + 1)
            {
                idx = _arabicCodes.IndexOf(b * 256 + buffer[index + 1]);
                if (idx >= 0)
                {
                    index++;
                    return _arabicLetters[idx];
                }
            }

            return string.Format("({0})", b);
        }

        public static string GetLatinString(Encoding encoding, byte[] buffer, ref int index)
        {
            byte b = buffer[index];

            int idx = _latinCodes.IndexOf(b);
            if (idx >= 0)
                return _latinLetters[idx];

            if (buffer.Length > index + 1)
            {
                idx = _latinCodes.IndexOf(b * 256 + buffer[index + 1]);
                if (idx >= 0)
                {
                    index++;
                    return _latinLetters[idx];
                }
            }

            return encoding.GetString(buffer, index, 1);
        }

        private TimeCode GetTimeCode(int timeCodeIndex, byte[] buffer)
        {
            if (timeCodeIndex > 0)
            {
                string highPart = string.Format("{0:000000}", buffer[timeCodeIndex] + buffer[timeCodeIndex + 1] * 256);
                string lowPart = string.Format("{0:000000}", buffer[timeCodeIndex+2] + buffer[timeCodeIndex + 3] * 256);

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