using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class Cavena890 : SubtitleFormat
    {

        static List<int> _hebrewCodes = new List<int> {
            0x40, // א
            0x41, // ב
            0x42, // ג
            0x43, // ד
            0x44, // ה
            0x45, // ו
            0x46, // ז
            0x47, // ח
            0x49, // י
            0x4c, // ל
            0x4d, // ם
            0x4e, // מ
            0x4f, // ן
            0x50, // נ
            0x51, // ס
            0x52, // ע
            0x54, // פ
            0x56, // צ
            0x57, // ק
            0x58, // ר
            0x59, // ש
            0x5A, // ת
            0x4b, // כ
            0x4a, // ך
            0x48, // ע
            0x53, // ף
        };

        static List<string> _hebrewLetters = new List<string> {
            "א",
            "ב",
            "ג",
            "ד",
            "ה",
            "ו",
            "ז",
            "ח",
            "י",
            "ל",
            "ם",
            "מ",
            "ן",
            "נ",
            "ס",
            "ע",
            "פ",
            "צ",
            "ק",
            "ר",
            "ש",
            "ת",
            "כ",
            "ך",
            "ע",
            "ף",
        };

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

        private string _language;

        public void Save(string fileName, Subtitle subtitle)
        {
            _language = null;
            if (subtitle.Header != null && subtitle.Header.StartsWith("890-language:"))
                _language = subtitle.Header.Remove(0, "890-language:".Length);

            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            if (_language != null && _language.StartsWith("HEB"))
            {
                byte[] buffer = new byte[388];
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = 0;

                buffer[32] = 0x3A;
                buffer[33] = 0x30;
                buffer[34] = 0x30;

                buffer[134] = 0x09;
                buffer[135] = 0x07;
                buffer[136] = 0x15;
                buffer[137] = 0x13;
                buffer[138] = 0x56;
                buffer[139] = 0x56;
                buffer[140] = 0x08;
                buffer[141] = 0x90;

                buffer[146] = 0x09;
                buffer[147] = 0x8f;

                buffer[172] = 0x08;
                buffer[173] = 0x90;

                buffer[179] = 0xF6;
                buffer[180] = 0x01;

                buffer[187] = 0x48; // HEBNOA.V
                buffer[188] = 0x45;
                buffer[189] = 0x42;
                buffer[190] = 0x4E;
                buffer[191] = 0x4F;
                buffer[192] = 0x41;
                buffer[193] = 0x2E;
                buffer[194] = 0x56;

                buffer[208] = 0xf6;
                buffer[209] = 0x01;
                buffer[210] = 0xf3;
                buffer[211] = 0x01;
                buffer[213] = 0x03;

                buffer[246] = 0x56;
                buffer[247] = 0x46;
                buffer[248] = 0x4F;
                buffer[249] = 0x4E;
                buffer[250] = 0x54;
                buffer[251] = 0x4C;
                buffer[252] = 0x2E;
                buffer[253] = 0x56;
                buffer[254] = 0x4B;
                buffer[255] = 0x02;
                buffer[256] = 0x30;
                buffer[257] = 0x30;
                buffer[258] = 0x3A;
                buffer[259] = 0x30;
                buffer[260] = 0x30;
                buffer[261] = 0x3A;
                buffer[262] = 0x30;
                buffer[263] = 0x30;
                buffer[264] = 0x3A;
                buffer[265] = 0x30;
                buffer[266] = 0x30;

                fs.Write(buffer, 0, buffer.Length);
            }
            else
            {
                //header
                for (int i = 0; i < 22; i++)
                    fs.WriteByte(0);

                byte[] buffer = ASCIIEncoding.ASCII.GetBytes("Subtitle Edit");
                fs.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 18 - buffer.Length; i++)
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
            }

            // paragraphs
            int number = 16;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // number
                fs.WriteByte((byte)(number / 256));
                fs.WriteByte((byte)(number % 256));

                WriteTime(fs, p.StartTime);
                WriteTime(fs, p.EndTime);

                var buffer = new byte[] { 0x14, 00, 00, 00, 00, 00, 00, 0x16 };
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
            int skipCount = 0;
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0x7F;

            var encoding = Encoding.Default;
            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                string current = text.Substring(i, 1);
                if (skipCount > 0)
                {
                    skipCount--;
                }
                else if (_language != null && _language.StartsWith("HEB"))
                {
                    int letterIndex = _hebrewLetters.IndexOf(current);
                    if (letterIndex >= 0)
                    {
                        buffer[index] = (byte)_hebrewCodes[letterIndex];
                    }
                    else if (i + 3 < text.Length && text.Substring(i, 3) == "<i>")
                    {
                        buffer[index] = 0x88;
                        skipCount = 2;
                    }
                    else if (i + 4 <= text.Length && text.Substring(i, 4) == "</i>")
                    {
                        buffer[index] = 0x98;
                        skipCount = 2;
                    }
                    else
                    {
                        buffer[index] = encoding.GetBytes(current)[0];
                    }
                    index++;
                }
                else
                {
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
                            index++;
                            buffer[index] = 0x41;
                        }
                        else if (current == "ä")
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == "Ö")
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x4F;
                        }
                        else if (current == "ö")
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x6F;
                        }
                        else if (current == "å")
                        {
                            buffer[index] = 0x8C;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == "Å")
                        {
                            buffer[index] = 0x8C;
                            index++;
                            buffer[index] = 0x41;
                        }

                        // ăĂ îÎ şŞ ţŢ âÂ (romanian)
                        else if (current == "ă")
                        {
                            buffer[index] = 0x89;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == "Ă")
                        {
                            buffer[index] = 0x89;
                            index++;
                            buffer[index] = 0x41;
                        }
                        else if (current == "î")
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x69;
                        }
                        else if (current == "Î")
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x49;
                        }
                        else if (current == "ş")
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x73;
                        }
                        else if (current == "Ş")
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x53;
                        }
                        else if (current == "ţ")
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x74;
                        }
                        else if (current == "Ţ")
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x74;
                        }
                        else if (current == "â")
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == "Â")
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x41;
                        }

                        else if (i + 3 < text.Length && text.Substring(i, 3) == "<i>")
                        {
                            buffer[index] = 0x88;
                            skipCount = 2;
                        }
                        else if (i + 4 <= text.Length && text.Substring(i, 4) == "</i>")
                        {
                            buffer[index] = 0x98;
                            skipCount = 3;
                        }
                        else
                        {
                            buffer[index] = encoding.GetBytes(current)[0];
                        }
                        index++;
                    }
                }
            }

            return buffer;
        }

        private void WriteTime(FileStream fs, TimeCode timeCode)
        {
            double totalMilliseconds = timeCode.TotalMilliseconds; // +TimeSpan.FromHours(10).TotalMilliseconds; // +10 hours
            int frames = (int)Math.Round(totalMilliseconds / (1000.0 /Configuration.Settings.General.CurrentFrameRate));
            fs.WriteByte((byte)(frames / 256 / 256));
            fs.WriteByte((byte)(frames / 256));
            fs.WriteByte((byte)(frames % 256));
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (!fileName.EndsWith(".890"))
                        return false;

                    var sub = new Subtitle();
                    LoadSubtitle(sub, lines, fileName);
                    return sub.Paragraphs.Count > 0;
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
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);

            _language = GetLanguage(buffer);
            if (_language != null)
                subtitle.Header = "890-language:" + _language;

            int i = 455;
            int lastNumber = -1;
            while (i < buffer.Length - 20)
            {
                int start = i - TextLength;

                int number = buffer[start - 16] * 256 + buffer[start - 15];

                Paragraph p = new Paragraph();
                double startFrame = buffer[start - 14] * 256 * 256 + buffer[start - 13] * 256 + buffer[start - 12];
                double endFrame = buffer[start - 11] * 256 * 256 + buffer[start - 10] * 256 + buffer[start - 9];
                
                string line1 = FixText(buffer, start, TextLength);
                string line2 = FixText(buffer, start + TextLength + 6, TextLength);
                if (lastNumber == number)
                {
                    p = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    string temp = (line1 + Environment.NewLine + line2).Trim();
                    if (temp.Length > 0)
                        p.Text = temp;
                }
                else
                {
                    subtitle.Paragraphs.Add(p);
                    p.StartTime.TotalMilliseconds = ((1000.0 / Configuration.Settings.General.CurrentFrameRate) * startFrame);
                    p.EndTime.TotalMilliseconds = ((1000.0 / Configuration.Settings.General.CurrentFrameRate) * endFrame);
                    p.Text = (line1 + Environment.NewLine + line2).Trim();
                }

                lastNumber = number;

                i+=128;
            }

            subtitle.Renumber(1);
        }

        private string GetLanguage(byte[] buffer)
        {
            if (buffer.Length < 200)
                return null;

            return Encoding.ASCII.GetString(buffer, 187, 6);
        }

        private string FixText(byte[] buffer, int start, int textLength)
        {
            string text;
            if (_language == "HEBNOA")
            {
                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                var sb = new StringBuilder();
                for (int i = 0; i < textLength; i++)
                {
                    int b = buffer[start + i];
                    int idx = _hebrewCodes.IndexOf(b);
                    if (idx >= 0)
                        sb.Append(_hebrewLetters[idx]);
                    else
                        sb.Append(encoding.GetString(buffer, start+i, 1));
                }

                text = sb.ToString();

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?

                text = Utilities.FixEnglishTextInRightToLeftLanguage(text, "0123456789abcdefghijklmnopqrstuvwxyz");
            }
            else if (_language == "CCKM44")
            {
                var sb = new StringBuilder();
                int index = start;

                text = Encoding.GetEncoding(1201).GetString(buffer, index, textLength - 1).Replace("\0", string.Empty);                

                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?

                text = text.Replace(encoding.GetString(new byte[] { 0x88 }), "<i>");
                text = text.Replace(encoding.GetString(new byte[] { 0x98 }), "</i>");

                if (text.Contains("<i></i>"))
                    text = text.Replace("<i></i>", "<i>");
                if (text.Contains("<i>") && !text.Contains("</i>"))
                    text += "</i>";
            }
            else
            {
                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                text = encoding.GetString(buffer, start, textLength);

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


                //ăĂ îÎ şŞ ţŢ âÂ (romanian)
                text = text.Replace(encoding.GetString(new byte[] { 0x89, 0x61 }), "ă");
                text = text.Replace(encoding.GetString(new byte[] { 0x89, 0x41 }), "Ă");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x69 }), "î");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x49 }), "Î");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x73 }), "ş");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x53 }), "Ş");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x74 }), "ţ");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x54 }), "Ţ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x61 }), "â");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x41 }), "Â");


                if (text.Contains("<i></i>"))
                    text = text.Replace("<i></i>", "<i>");
                if (text.Contains("<i>") && !text.Contains("</i>"))
                    text += "</i>";
            }

            return text;
        }

    }
}