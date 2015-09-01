using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Cavena890 : SubtitleFormat
    {
        private const int LanguageIdDanish = 0x07;
        private const int LanguageIdEnglish = 0x09;
        private const int LanguageIdRussian = 0x56;
        private const int LanguageIdArabic = 0x80;
        private const int LanguageIdHebrew = 0x8f;
        private const int LanguageIdChineseTraditional = 0x90;
        private const int LanguageIdChineseSimplified = 0x91;

        private static readonly List<int> HebrewCodes = new List<int> {
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
            0x48, // ט
            0x53, // ף
            0x55, // ץ
        };

        private static readonly List<string> HebrewLetters = new List<string> {
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
            "ט",
            "ף",
            "ץ",
        };

        private static readonly List<int> RussianCodes = new List<int> {
            0x42, // Б
            0x45, // Е
            0x5A, // З
            0x56, // В
            0x49, // И
            0x4E, // Н
            0x58, // Ы
            0x51, // Я
            0x56, // V
            0x53, // С
            0x72, // р
            0x69, // и
            0x71, // я
            0x6E, // н
            0x74, // т
            0x5C, // Э
            0x77, // ю
            0x46, // Ф
            0x5E, // Ч
            0x44, // Д
            0x62, // б
            0x73, // с
            0x75, // у
            0x64, // д
            0x60, // ж
            0x6A, // й
            0x6C, // л
            0x47, // Г
            0x78, // ы
            0x7A, // з
            0x7E, // ч
            0x6D, // м
            0x67, // г
            0x79, // ь
            0x70, // п
            0x76, // в
            0x55, // У
            0x7D, // щ
            0x66, // ф
            0x7C, // э
            0x7B, // ш
            0x50, // П
            0x52, // П
            0x68, // П
        };

        private static readonly List<string> RussianLetters = new List<string> {
            "Б",
            "Е",
            "З",
            "В",
            "И",
            "Н",
            "Ы",
            "Я",
            "V",
            "С",
            "р",
            "и",
            "я",
            "н",
            "т",
            "Э",
            "ю",
            "Ф",
            "Ч",
            "Д",
            "б",
            "с",
            "у",
            "д",
            "ж",
            "й",
            "л",
            "Г",
            "ы",
            "з",
            "ч",
            "м",
            "г",
            "ь",
            "п",
            "в",
            "У",
            "щ",
            "ф",
            "э",
            "ш",
            "П",
            "Р",
            "х",
        };

        public override string Extension
        {
            get { return ".890"; }
        }

        public const string NameOfFormat = "Cavena 890";

        public override string Name
        {
            get { return NameOfFormat; }
        }

        public override bool IsTimeBased
        {
            get { return false; }
        }

        private int _languageIdLine1 = LanguageIdEnglish;
        private int _languageIdLine2 = LanguageIdEnglish;

        public void Save(string fileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                int russianCount = 0;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    if (p.Text.Contains(new[] { '的', '是', '啊', '吧', '好', '吧', '亲', '爱', '的', '早', '上' }))
                    {
                        _languageIdLine1 = LanguageIdChineseSimplified;
                        _languageIdLine2 = LanguageIdChineseSimplified;
                        break;
                    }
                    if (p.Text.Contains(new[] { 'я', 'д', 'й', 'л', 'щ', 'ж', 'ц', 'ф', 'ы' }))
                    {
                        russianCount++;
                        if (russianCount > 10)
                        {
                            _languageIdLine1 = LanguageIdRussian;
                            _languageIdLine2 = LanguageIdRussian; // or 0x09?
                            break;
                        }
                    }
                }

                var language = Utilities.AutoDetectGoogleLanguage(subtitle);
                if (language == "he") // Hebrew
                {
                    _languageIdLine1 = LanguageIdHebrew;
                    _languageIdLine2 = LanguageIdHebrew; // or 0x09
                }
                else if (language == "ru")
                {
                    _languageIdLine1 = LanguageIdRussian;
                    _languageIdLine2 = LanguageIdRussian; // or 0x09?
                }
                else if (language == "zh")
                {
                    _languageIdLine1 = LanguageIdChineseSimplified;
                    _languageIdLine2 = LanguageIdChineseSimplified;
                }
                else if (language == "da")
                {
                    _languageIdLine1 = LanguageIdDanish;
                    _languageIdLine2 = LanguageIdDanish;
                }

                // prompt???
                //if (Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine1 >= 0)
                //    _languageIdLine1 = Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine1;
                //if (Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine2 >= 0)
                //    _languageIdLine2 = Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine2;

                // write file header (some fields are known, some are not...)

                fs.WriteByte(0); // ?
                fs.WriteByte(0); // ?

                // tape number (20 bytes)
                for (int i = 0; i < 20; i++)
                    fs.WriteByte(0);

                // ?
                for (int i = 0; i < 18; i++)
                    fs.WriteByte(0);

                // translated programme title (28 bytes)
                string title = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
                if (title.Length > 28)
                    title = title.Substring(0, 28);
                var buffer = Encoding.ASCII.GetBytes(title);
                fs.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 28 - buffer.Length; i++)
                    fs.WriteByte(0);

                // translator (28 bytes)
                for (int i = 0; i < 28; i++)
                    fs.WriteByte(0);

                // ?
                for (int i = 0; i < 9; i++)
                    fs.WriteByte(0);

                // translated episode title (11 bytes)
                for (int i = 0; i < 11; i++)
                    fs.WriteByte(0);

                // ?
                for (int i = 0; i < 18; i++)
                    fs.WriteByte(0);

                // ? + language codes
                buffer = new byte[] { 0xA0, 0x05, 0x04, 0x03, 0x06, 0x06, 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, (byte)_languageIdLine1, (byte)_languageIdLine2 };
                fs.Write(buffer, 0, buffer.Length);

                // comments (24 bytes)
                buffer = Encoding.ASCII.GetBytes("Made with Subtitle Edit");
                fs.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 24 - buffer.Length; i++)
                    fs.WriteByte(0);

                // ??
                buffer = new byte[] { 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00 };
                fs.Write(buffer, 0, buffer.Length);

                // number of subtitles
                fs.WriteByte((byte)(subtitle.Paragraphs.Count % 256));
                fs.WriteByte((byte)(subtitle.Paragraphs.Count / 256));

                // write font - prefix with binary zeroes
                buffer = GetFontBytesFromLanguageId(_languageIdLine1); // also TBX308VFONTL.V for english...
                for (int i = 0; i < 14 - buffer.Length; i++)
                    fs.WriteByte(0);
                fs.Write(buffer, 0, buffer.Length);

                // ?
                for (int i = 0; i < 13; i++)
                    fs.WriteByte(0);

                // some language codes again?
                if (_languageIdLine1 == LanguageIdHebrew || _languageIdLine2 == LanguageIdHebrew)
                {
                    buffer = new byte[] { 0x64, 0x02, 0x64, 0x02 };
                }
                else if (_languageIdLine1 == LanguageIdRussian || _languageIdLine2 == LanguageIdRussian)
                {
                    buffer = new byte[] { 0xce, 0x00, 0xce, 0x00 };
                }
                else
                {
                    buffer = new byte[] { 0x37, 0x00, 0x37, 0x00 }; // seen in English files
                }
                fs.Write(buffer, 0, buffer.Length);

                // ?
                for (int i = 0; i < 6; i++)
                    fs.WriteByte(0);

                // original programme title (28 chars)
                for (int i = 0; i < 28; i++)
                    fs.WriteByte(0);

                // write font (use same font id from line 1)
                buffer = GetFontBytesFromLanguageId(_languageIdLine1);
                fs.Write(buffer, 0, buffer.Length);

                buffer = new byte[]
                {
                    0x3d, 0x8d, 0x31, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x3A, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x54, 0x44
                };
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

                    if (p.Text.StartsWith("{\\an1}"))
                        fs.WriteByte(0x50); // left
                    else if (p.Text.StartsWith("{\\an3}"))
                        fs.WriteByte(0x52); // left
                    else
                        fs.WriteByte(0x54); // center

                    buffer = new byte[] { 0, 0, 0, 0, 0, 0, 0 }; // 0x16 }; -- the last two bytes might be something with vertical alignment...
                    fs.Write(buffer, 0, buffer.Length);

                    WriteText(fs, p.Text, p == subtitle.Paragraphs[subtitle.Paragraphs.Count - 1], _languageIdLine1);

                    number += 16;
                }
            }
        }

        private static byte[] GetFontBytesFromLanguageId(int languageId)
        {
            var buffer = Encoding.ASCII.GetBytes("HLV23N.V");
            if (languageId == LanguageIdChineseTraditional || languageId == LanguageIdChineseSimplified)
                buffer = Encoding.ASCII.GetBytes("CCKM44.V");
            else if (languageId == LanguageIdArabic)
                buffer = Encoding.ASCII.GetBytes("ARA19N.V");
            else if (languageId == LanguageIdRussian)
                buffer = Encoding.ASCII.GetBytes("KYRIL4.V");
            else if (languageId == LanguageIdHebrew)
                buffer = Encoding.ASCII.GetBytes("HEBNOA.V");
            else if (languageId == LanguageIdDanish)
                buffer = Encoding.ASCII.GetBytes("VFONTL.V");
            return buffer;
        }

        private static void WriteText(FileStream fs, string text, bool isLast, int languageIdLine)
        {
            string line1 = string.Empty;
            string line2 = string.Empty;
            var lines = text.SplitToLines();
            if (lines.Length > 2)
                lines = Utilities.AutoBreakLine(text).SplitToLines();
            if (lines.Length > 1)
            {
                line1 = lines[0];
                line2 = lines[1];
            }
            else if (lines.Length == 1)
            {
                line2 = lines[0];
            }

            var buffer = GetTextAsBytes(line1, languageIdLine);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00, 00, 00 };
            fs.Write(buffer, 0, buffer.Length);

            buffer = GetTextAsBytes(line2, languageIdLine);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00 };
            if (!isLast)
                fs.Write(buffer, 0, buffer.Length);
        }

        private static byte[] GetTextAsBytes(string text, int languageId)
        {
            var buffer = new byte[51];
            int skipCount = 0;
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0x7F;

            if (languageId == LanguageIdChineseTraditional || languageId == LanguageIdChineseSimplified)
            {
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = 0;
            }

            var encoding = Encoding.Default;
            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var current = text[i];
                if (skipCount > 0)
                {
                    skipCount--;
                }
                else if (languageId == LanguageIdHebrew)
                {
                    int letterIndex = HebrewLetters.IndexOf(current.ToString(CultureInfo.InvariantCulture));
                    if (letterIndex >= 0)
                    {
                        buffer[index] = (byte)HebrewCodes[letterIndex];
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
                        buffer[index] = encoding.GetBytes(new[] { current })[0];
                    }
                    index++;
                }
                else if (languageId == LanguageIdChineseTraditional || languageId == LanguageIdChineseSimplified)
                {
                    encoding = Encoding.GetEncoding(1201);
                    if (index < 49)
                    {
                        if (i + 3 < text.Length && text.Substring(i, 3) == "<i>")
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
                            buffer[index] = encoding.GetBytes(new[] { current })[0];
                            index++;
                        }
                    }
                }
                else
                {
                    if (index < 50)
                    {
                        if (current == 'æ')
                            buffer[index] = 0x1B;
                        else if (current == 'ø')
                            buffer[index] = 0x1C;
                        else if (current == 'å')
                            buffer[index] = 0x1D;
                        else if (current == 'Æ')
                            buffer[index] = 0x5B;
                        else if (current == 'Ø')
                            buffer[index] = 0x5C;
                        else if (current == 'Å')
                            buffer[index] = 0x5D;
                        else if (current == 'Ä')
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x41;
                        }
                        else if (current == 'ä')
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == 'Ö')
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x4F;
                        }
                        else if (current == 'ö')
                        {
                            buffer[index] = 0x86;
                            index++;
                            buffer[index] = 0x6F;
                        }

                        // different language setting?
                        //else if (current == 'å')
                        //{
                        //    buffer[index] = 0x8C;
                        //    index++;
                        //    buffer[index] = 0x61;
                        //}
                        //else if (current == 'Å')
                        //{
                        //    buffer[index] = 0x8C;
                        //    index++;
                        //    buffer[index] = 0x41;
                        //}

                        // ăĂ îÎ şŞ ţŢ âÂ (romanian)
                        else if (current == 'ă')
                        {
                            buffer[index] = 0x89;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == 'Ă')
                        {
                            buffer[index] = 0x89;
                            index++;
                            buffer[index] = 0x41;
                        }
                        else if (current == 'î')
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x69;
                        }
                        else if (current == 'Î')
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x49;
                        }
                        else if (current == 'ş')
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x73;
                        }
                        else if (current == 'Ş')
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x53;
                        }
                        else if (current == 'ţ')
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x74;
                        }
                        else if (current == 'Ţ')
                        {
                            buffer[index] = 0x87;
                            index++;
                            buffer[index] = 0x74;
                        }
                        else if (current == 'â')
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x61;
                        }
                        else if (current == 'Â')
                        {
                            buffer[index] = 0x83;
                            index++;
                            buffer[index] = 0x41;
                        }
                        else if (current == 'è')
                        {
                            buffer[index] = 0x81;
                            index++;
                            buffer[index] = 0x65;
                        }
                        else if (current == 'é')
                        {
                            buffer[index] = 0x82;
                            index++;
                            buffer[index] = 0x65;
                        }
                        else if (current == 'É')
                        {
                            buffer[index] = 0x82;
                            index++;
                            buffer[index] = 0x45;
                        }
                        else if (current == 'È')
                        {
                            buffer[index] = 0x81;
                            index++;
                            buffer[index] = 0x45;
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
                            buffer[index] = encoding.GetBytes(new[] { current })[0];
                        }
                        index++;
                    }
                }
            }

            return buffer;
        }

        private static void WriteTime(FileStream fs, TimeCode timeCode)
        {
            double totalMilliseconds = timeCode.TotalMilliseconds;
            int frames = (int)Math.Round(totalMilliseconds / (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate));
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
                    if (!fileName.EndsWith(".890", StringComparison.Ordinal))
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
            const int textLength = 51;

            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            _languageIdLine1 = buffer[146];
            if (_languageIdLine1 == 0)
                _languageIdLine1 = LanguageIdEnglish;
            Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine1 = _languageIdLine1;

            _languageIdLine2 = buffer[147];
            if (_languageIdLine2 == 0)
                _languageIdLine2 = LanguageIdEnglish;
            Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine2 = _languageIdLine2;

            var fontNameLine1 = Encoding.ASCII.GetString(buffer, 187, 6);
            var fontNameLine2 = Encoding.ASCII.GetString(buffer, 246, 6);

            // Hebrew
            if (_languageIdLine1 == LanguageIdHebrew || fontNameLine1 == "HEBNOA" || fontNameLine2 == "HEBNOA")
            {
                _languageIdLine1 = LanguageIdHebrew;
                _languageIdLine2 = LanguageIdHebrew;
            }

            // Russian
            else if (_languageIdLine1 == LanguageIdRussian || fontNameLine1.StartsWith("KYRIL", StringComparison.Ordinal) || fontNameLine2.StartsWith("KYRIL", StringComparison.Ordinal))
            {
                _languageIdLine1 = LanguageIdRussian;
                _languageIdLine2 = LanguageIdRussian;
            }


            // Chinese
            else if (_languageIdLine1 == LanguageIdChineseSimplified)
            {
                _languageIdLine1 = LanguageIdChineseSimplified;
                _languageIdLine2 = LanguageIdChineseSimplified;
            }
            else if (_languageIdLine1 == LanguageIdChineseTraditional || fontNameLine1 == "CCKM44" || fontNameLine2 == "CCKM44")
            {
                _languageIdLine1 = LanguageIdChineseTraditional;
                _languageIdLine2 = LanguageIdChineseTraditional;
            }


            int i = 455;
            int lastNumber = -1;
            while (i < buffer.Length - 20)
            {
                int start = i - textLength;

                int number = buffer[start - 16] * 256 + buffer[start - 15];

                var p = new Paragraph();
                double startFrame = buffer[start - 14] * 256 * 256 + buffer[start - 13] * 256 + buffer[start - 12];
                double endFrame = buffer[start - 11] * 256 * 256 + buffer[start - 10] * 256 + buffer[start - 9];

                string line1 = FixText(buffer, start, textLength, _languageIdLine1);
                string line2 = FixText(buffer, start + textLength + 6, textLength, _languageIdLine2);

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
                    p.StartTime.TotalMilliseconds = (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate) * startFrame;
                    p.EndTime.TotalMilliseconds = (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate) * endFrame;
                    p.Text = (line1 + Environment.NewLine + line2).Trim();
                }

                lastNumber = number;

                i += 128;
            }

            subtitle.Renumber();
        }

        private static string FixText(byte[] buffer, int start, int textLength, int languageId)
        {
            string text;

            if (languageId == LanguageIdRussian)
            {
                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                var sb = new StringBuilder();
                for (int i = 0; i < textLength; i++)
                {
                    int b = buffer[start + i];
                    int idx = RussianCodes.IndexOf(b);
                    if (idx >= 0)
                        sb.Append(RussianLetters[idx]);
                    else
                        sb.Append(encoding.GetString(buffer, start + i, 1));
                }

                text = sb.ToString();

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?

                if (text.Contains("<i></i>"))
                    text = text.Replace("<i></i>", "<i>");
                if (text.Contains("<i>") && !text.Contains("</i>"))
                    text += "</i>";
            }
            else if (languageId == LanguageIdHebrew) // (_language == "HEBNOA")
            {
                var encoding = Encoding.Default; // which encoding?? Encoding.GetEncoding("ISO-8859-5")
                var sb = new StringBuilder();
                for (int i = 0; i < textLength; i++)
                {
                    int b = buffer[start + i];
                    int idx = HebrewCodes.IndexOf(b);
                    if (idx >= 0)
                        sb.Append(HebrewLetters[idx]);
                    else
                        sb.Append(encoding.GetString(buffer, start + i, 1));
                }

                text = sb.ToString();

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?

                text = Utilities.FixEnglishTextInRightToLeftLanguage(text, ",.?-'/\"0123456789abcdefghijklmnopqrstuvwxyz");
            }
            else if (languageId == LanguageIdChineseTraditional || languageId == LanguageIdChineseSimplified) //  (_language == "CCKM44" || _language == "TVB000")
            {
                int index = start;

                while (textLength >= 1 && index + textLength < buffer.Length && (buffer[index + textLength - 1] == 0))
                    textLength--;
                if (textLength > 0)
                {
                    text = Encoding.GetEncoding(1201).GetString(buffer, index, textLength).Replace("\0", string.Empty);
                }
                else
                {
                    text = string.Empty;
                }

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
                text = encoding.GetString(buffer, start, textLength).Replace("\0", string.Empty);

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?
                text = text.Replace(encoding.GetString(new byte[] { 0x1B }), "æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x1C }), "ø");
                text = text.Replace(encoding.GetString(new byte[] { 0x1D }), "å");
                text = text.Replace(encoding.GetString(new byte[] { 0x1E }), "Æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x1F }), "Ø");

                text = text.Replace(encoding.GetString(new byte[] { 0x5B }), "Æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x5C }), "Ø");
                text = text.Replace(encoding.GetString(new byte[] { 0x5D }), "Å");

                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x41 }), "Ä");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x61 }), "ä");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x4F }), "Ö");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x6F }), "ö");

                text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x61 }), "å");
                text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x41 }), "Å");

                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x65 }), "è");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x65 }), "é");

                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x45 }), "É");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x65 }), "È");

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