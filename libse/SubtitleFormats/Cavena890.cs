using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Cavena890 : SubtitleFormat, IBinaryPersistableSubtitle
    {
        public const int LanguageIdDanish = 0x07;
        public const int LanguageIdEnglish = 0x09;
        public const int LanguageIdRussian = 0x56;
        public const int LanguageIdArabic = 0x80;
        public const int LanguageIdHebrew = 0x8f;
        public const int LanguageIdChineseTraditional = 0x90;
        public const int LanguageIdChineseSimplified = 0x91;
        public const int LanguageIdRomanian = 0x22;

        private static readonly List<int> HebrewCodes = new List<int>
        {
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

            0xB1, // "a"
            0xB2, // "b"
            0xB3, // "c"
            0xB4, // "d"
            0xB5, // "e"
            0xB6, // "f"
            0xB7, // "g"
            0xB8, // "h"
            0xB9, // "i"
            0xBA, // "j"
            0xBB, // "k"
            0xBC, // "l"
            0xBD, // "m"
            0xBE, // "n"
            0xBF, // "o"
            0xC0, // "p"
            0xC1, // "q"
            0xC2, // "r"
            0xC3, // "s"
            0xC4, // "t"
            0xC5, // "u"
            0xC6, // "v"
            0xC7, // "w"
            0xC8, // "x"
            0xC9, // "y"
            0xCA, // "z"

            0x91, // "A"
            0xDB, // "B" -- weird
            0x93, // "C"
            0xDC, // "D" -- weird
            0x95, // "E"
            0x96, // "F"
            0x97, // "G"
            0xAB, // "H" -- weird
            0x99, // "I"
            0x9A, // "J"
            0x9B, // "K"
            0x9C, // "L"
            0xDD, // "M"
            0xDE, // "N"
            0x9F, // "O"
            0xA0, // "P"
            0xA1, // "Q"
            0xA2, // "R"
            0xA3, // "S"
            0xA4, // "T"
            0xA5, // "U"
            0xA6, // "V"
            0xA7, // "W"
            0xA8, // "X" - weird
            0xA9, // "Y"
            0xAA, // "Z" - weird
        };

        private static readonly List<string> HebrewLetters = new List<string>
        {
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

            "a", // 0xB1
            "b", // 0xB2
            "c", // 0xB3
            "d", // 0xB4
            "e", // 0xB5
            "f", // 0xB6
            "g", // 0xB7
            "h", // 0xB8
            "i", // 0xB9
            "j", // 0xBA
            "k", // 0xBB
            "l", // 0xBC
            "m", // 0xBD
            "n", // 0xBE
            "o", // 0xBF
            "p", // 0xC0
            "q", // 0xC1
            "r", // 0xC2
            "s", // 0xC3
            "t", // 0xC4
            "u", // 0xC5
            "v", // 0xC6
            "w", // 0xC7
            "x", // 0xC8
            "y", // 0xC9
            "z", // 0xCA

            "A", // 0x91,
            "B", // 0xDB,
            "C", // 0x93,
            "D", // 0xDC,
            "E", // 0x95,
            "F", // 0x96,
            "G", // 0x97,
            "H", // 0xAB,
            "I", // 0x99,
            "J", // 0x9A,
            "K", // 0x9B,
            "L", // 0x9C,
            "M", // 0xDD,
            "N", // 0xDE,
            "O", // 0x9F,
            "P", // 0xA0,
            "Q", // 0xA1,
            "R", // 0xA2,
            "S", // 0xA3,
            "T", // 0xA4,
            "U", // 0xA5,
            "V", // 0xA6,
            "W", // 0xA7,
            "X", // 0xA8,
            "Y", // 0xA9,
            "Z", // 0xAA,
        };

        private static readonly List<int> RussianCodes = new List<int>
        {
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

        private static readonly List<string> RussianLetters = new List<string>
        {
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

        public override string Extension => ".890";

        public const string NameOfFormat = "Cavena 890";

        public override string Name => NameOfFormat;

        public override bool IsTimeBased => false;

        private int _languageIdLine1 = LanguageIdEnglish;
        private int _languageIdLine2 = LanguageIdEnglish;

        public bool Save(string fileName, Subtitle subtitle, bool batchMode = false)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                return Save(fileName, fs, subtitle, batchMode);
            }
        }

        public bool Save(string fileName, Stream stream, Subtitle subtitle, bool batchMode)
        {
            int russianCount = 0;
            char[] logoGrams = { '的', '是', '啊', '吧', '好', '吧', '亲', '爱', '的', '早', '上' };
            char[] russianChars = { 'я', 'д', 'й', 'л', 'щ', 'ж', 'ц', 'ф', 'ы' };
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (p.Text.Contains(logoGrams))
                {
                    _languageIdLine1 = LanguageIdChineseSimplified;
                    _languageIdLine2 = LanguageIdChineseSimplified;
                    break;
                }
                if (p.Text.Contains(russianChars))
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

            if (Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId > 0)
            {
                _languageIdLine1 = Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId;
                _languageIdLine2 = Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId;
            }
            else
            {
                var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                switch (language)
                {
                    case "he":
                        _languageIdLine1 = LanguageIdHebrew;
                        _languageIdLine2 = LanguageIdHebrew; // or 0x09
                        break;
                    case "ru":
                        _languageIdLine1 = LanguageIdRussian;
                        _languageIdLine2 = LanguageIdRussian; // or 0x09?
                        break;
                    case "zh":
                        _languageIdLine1 = LanguageIdChineseSimplified;
                        _languageIdLine2 = LanguageIdChineseSimplified;
                        break;
                    case "da":
                        _languageIdLine1 = LanguageIdDanish;
                        _languageIdLine2 = LanguageIdDanish;
                        break;
                }
            }

            // prompt???
            //if (Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine1 >= 0)
            //    _languageIdLine1 = Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine1;
            //if (Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine2 >= 0)
            //    _languageIdLine2 = Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine2;

            // write file header (some fields are known, some are not...)

            stream.WriteByte(0); // ?
            stream.WriteByte(0); // ?

            // tape number (20 bytes)
            for (int i = 0; i < 20; i++)
            {
                stream.WriteByte(0);
            }

            // ?
            for (int i = 0; i < 18; i++)
            {
                stream.WriteByte(0);
            }

            // translated programme title (28 bytes)
            string title = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
            if (title.Length > 28)
            {
                title = title.Substring(0, 28);
            }

            if (!string.IsNullOrWhiteSpace(Configuration.Settings.SubtitleSettings.CurrentCavena89Title) && Configuration.Settings.SubtitleSettings.CurrentCavena89Title.Length <= 28)
            {
                title = Configuration.Settings.SubtitleSettings.CurrentCavena89Title;
            }

            var buffer = Encoding.ASCII.GetBytes(title);
            stream.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 28 - buffer.Length; i++)
            {
                stream.WriteByte(0);
            }

            // translator (28 bytes)
            if (!string.IsNullOrWhiteSpace(Configuration.Settings.SubtitleSettings.CurrentCavena890Translator) && Configuration.Settings.SubtitleSettings.CurrentCavena890Translator.Length <= 28)
            {
                buffer = Encoding.ASCII.GetBytes(Configuration.Settings.SubtitleSettings.CurrentCavena890Translator);
                stream.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 28 - buffer.Length; i++)
                {
                    stream.WriteByte(0);
                }
            }
            else
            {
                for (int i = 0; i < 28; i++)
                {
                    stream.WriteByte(0);
                }
            }

            // ?
            for (int i = 0; i < 9; i++)
            {
                stream.WriteByte(0);
            }

            // translated episode title (11 bytes)
            for (int i = 0; i < 11; i++)
            {
                stream.WriteByte(0);
            }

            // ?
            for (int i = 0; i < 18; i++)
            {
                stream.WriteByte(0);
            }

            // ? + language codes
            buffer = new byte[] { 0xA0, 0x05, 0x04, 0x03, 0x06, 0x06, 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, (byte)_languageIdLine1, (byte)_languageIdLine2 };
            stream.Write(buffer, 0, buffer.Length);

            // comments (24 bytes)
            buffer = Encoding.ASCII.GetBytes("");
            if (!string.IsNullOrWhiteSpace(Configuration.Settings.SubtitleSettings.CurrentCavena89Comment) && Configuration.Settings.SubtitleSettings.CurrentCavena89Comment.Length <= 24)
            {
                buffer = Encoding.ASCII.GetBytes(Configuration.Settings.SubtitleSettings.CurrentCavena89Comment);
            }

            stream.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 24 - buffer.Length; i++)
            {
                stream.WriteByte(0);
            }

            // ??
            buffer = new byte[] { 0x08, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00 };
            stream.Write(buffer, 0, buffer.Length);

            // number of subtitles
            stream.WriteByte((byte)(subtitle.Paragraphs.Count % 256));
            stream.WriteByte((byte)(subtitle.Paragraphs.Count / 256));

            // write font - prefix with binary zeroes
            buffer = GetFontBytesFromLanguageId(_languageIdLine1); // also TBX308VFONTL.V for english...
            for (int i = 0; i < 14 - buffer.Length; i++)
            {
                stream.WriteByte(0);
            }

            stream.Write(buffer, 0, buffer.Length);

            // ?
            for (int i = 0; i < 13; i++)
            {
                stream.WriteByte(0);
            }

            // number of subtitles again
            stream.WriteByte((byte)(subtitle.Paragraphs.Count % 256));
            stream.WriteByte((byte)(subtitle.Paragraphs.Count / 256));


            // number of subtitles again again
            stream.WriteByte((byte)(subtitle.Paragraphs.Count % 256));
            stream.WriteByte((byte)(subtitle.Paragraphs.Count / 256));

            // ?
            for (int i = 0; i < 6; i++)
            {
                stream.WriteByte(0);
            }

            // original programme title (28 chars)
            if (!string.IsNullOrWhiteSpace(Configuration.Settings.SubtitleSettings.CurrentCavena890riginalTitle) && Configuration.Settings.SubtitleSettings.CurrentCavena890riginalTitle.Length <= 28)
            {
                buffer = Encoding.ASCII.GetBytes(Configuration.Settings.SubtitleSettings.CurrentCavena890riginalTitle);
                stream.Write(buffer, 0, buffer.Length);
                for (int i = 0; i < 28 - buffer.Length; i++)
                {
                    stream.WriteByte(0);
                }
            }
            else
            {
                for (int i = 0; i < 28; i++)
                {
                    stream.WriteByte(0);
                }
            }

            // write font (use same font id from line 1)
            buffer = GetFontBytesFromLanguageId(_languageIdLine1);
            stream.Write(buffer, 0, buffer.Length);

            // ?
            stream.WriteByte(0x3d);
            stream.WriteByte(0x8d);

            // start of message time
            string startOfMessage = "10:00:00:00";
            if (Configuration.Settings.SubtitleSettings.Cavena890StartOfMessage != null &&
                Configuration.Settings.SubtitleSettings.Cavena890StartOfMessage.Length == startOfMessage.Length)
            {
                startOfMessage = Configuration.Settings.SubtitleSettings.Cavena890StartOfMessage;
            }

            buffer = Encoding.ASCII.GetBytes(startOfMessage);
            stream.Write(buffer, 0, buffer.Length);

            buffer = new byte[]
            {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x54, 0x44
            };
            stream.Write(buffer, 0, buffer.Length);

            for (int i = 0; i < 92; i++)
            {
                stream.WriteByte(0);
            }

            // paragraphs
            int number = 16;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // number
                stream.WriteByte((byte)(number / 256));
                stream.WriteByte((byte)(number % 256));

                WriteTime(stream, p.StartTime);
                WriteTime(stream, p.EndTime);

                if (p.Text.StartsWith("{\\an1}"))
                {
                    stream.WriteByte(0x50); // left
                }
                else if (p.Text.StartsWith("{\\an3}"))
                {
                    stream.WriteByte(0x52); // left
                }
                else
                {
                    stream.WriteByte(0x54); // center
                }

                buffer = new byte[] { 0, 0, 0, 0, 0, 0, 0 }; // 0x16 }; -- the last two bytes might be something with vertical alignment...
                stream.Write(buffer, 0, buffer.Length);

                bool hasBox = Utilities.RemoveSsaTags(p.Text).StartsWith("<box>");
                var text = p.Text.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
                text = HtmlUtil.RemoveOpenCloseTags(Utilities.RemoveSsaTags(text), HtmlUtil.TagBold, HtmlUtil.TagFont, HtmlUtil.TagBold);
                WriteText(stream, text, p == subtitle.Paragraphs[subtitle.Paragraphs.Count - 1], _languageIdLine1, hasBox);

                number += 16;
            }
            return true;
        }

        private static byte[] GetFontBytesFromLanguageId(int languageId)
        {
            var buffer = Encoding.ASCII.GetBytes("HLV23N.V");
            if (languageId == LanguageIdChineseTraditional || languageId == LanguageIdChineseSimplified)
            {
                buffer = Encoding.ASCII.GetBytes("CCKM44.V");
            }
            else if (languageId == LanguageIdArabic)
            {
                buffer = Encoding.ASCII.GetBytes("ARA19N.V");
            }
            else if (languageId == LanguageIdRussian)
            {
                buffer = Encoding.ASCII.GetBytes("KYRIL4.V");
            }
            else if (languageId == LanguageIdHebrew)
            {
                buffer = Encoding.ASCII.GetBytes("HEBNOA.V");
            }
            else if (languageId == LanguageIdDanish)
            {
                buffer = Encoding.ASCII.GetBytes("VFONTL.V");
            }

            return buffer;
        }

        private static void WriteText(Stream fs, string text, bool isLast, int languageIdLine, bool useBox)
        {
            string line1 = string.Empty;
            string line2 = string.Empty;
            var lines = text.SplitToLines();
            if (lines.Count > 2)
            {
                lines = Utilities.AutoBreakLine(text).SplitToLines();
            }

            if (lines.Count > 1)
            {
                line1 = lines[0];
                line2 = lines[1];
            }
            else
            {
                line2 = lines[0];
            }

            var buffer = GetTextAsBytes(line1, languageIdLine);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00, 00, 00 };
            if (useBox)
            {
                buffer[3] = 0xa0;
            }

            fs.Write(buffer, 0, buffer.Length);

            buffer = GetTextAsBytes(line2, languageIdLine);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 00, 00, 00, 00 };
            if (!isLast)
            {
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        private static byte[] GetTextAsBytes(string text, int languageId)
        {
            var buffer = new byte[51];
            int skipCount = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0x7F;
            }

            if (languageId == LanguageIdChineseTraditional || languageId == LanguageIdChineseSimplified)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }
            }
            else if (languageId == LanguageIdHebrew)
            {
                text = Utilities.ReverseNumbers(text);
                if (!Configuration.Settings.General.RightToLeftMode)
                {
                    text = Utilities.ReverseStartAndEndingForRightToLeft(text);
                }
            }

            var encoding = Encoding.Default;
            int index = 0;

            if (languageId == LanguageIdHebrew)
            {
                text = ReverseAnsi(text);
            }

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
                        {
                            buffer[index] = 0x1B;
                        }
                        else if (current == 'ø')
                        {
                            buffer[index] = 0x1C;
                        }
                        else if (current == 'å')
                        {
                            buffer[index] = 0x1D;
                        }
                        else if (current == 'Æ')
                        {
                            buffer[index] = 0x5B;
                        }
                        else if (current == 'Ø')
                        {
                            buffer[index] = 0x5C;
                        }
                        else if (current == 'Å')
                        {
                            buffer[index] = 0x5D;
                        }

                        // ăĂ şŞ ţŢ (romanian)
                        else if (current == 'ă')
                        {
                            AddTwo(buffer, ref index, 0x89, 0x61);
                        }
                        else if (current == 'Ă')
                        {
                            AddTwo(buffer, ref index, 0x89, 0x41);
                        }
                        else if (current == 'ş')
                        {
                            AddTwo(buffer, ref index, 0x87, 0x73);
                        }
                        else if (current == 'Ş')
                        {
                            AddTwo(buffer, ref index, 0x87, 0x53);
                        }
                        else if (current == 'ţ')
                        {
                            AddTwo(buffer, ref index, 0x87, 0x74);
                        }
                        else if (current == 'Ţ')
                        {
                            AddTwo(buffer, ref index, 0x87, 0x54);
                        }

                        // Next mapping of diacritics is reverse engineered,
                        // and currently only maps characters from latin alphabets according to https://en.wikipedia.org/wiki/Latin_alphabets

                        // capitals with accent grave
                        else if (current == 'À')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x41);
                        }
                        else if (current == 'È')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x45);
                        }
                        else if (current == 'Ì')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x49);
                        }
                        else if (current == 'Ò')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x4F);
                        }
                        else if (current == 'Ù')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x55);
                        }
                        else if (current == 'Ẁ')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x57);
                        }

                        // lowercase with accent grave
                        else if (current == 'à')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x61);
                        }
                        else if (current == 'è')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x65);
                        }
                        else if (current == 'ì')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x69);
                        }
                        else if (current == 'ò')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x6F);
                        }
                        else if (current == 'ù')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x75);
                        }
                        else if (current == 'ẁ')
                        {
                            AddTwo(buffer, ref index, 0x81, 0x75);
                        }

                        // capitals with accent aigu
                        else if (current == 'Á')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x41);
                        }
                        else if (current == 'Ć')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x43);
                        }
                        else if (current == 'É')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x45);
                        }
                        else if (current == 'Í')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x49);
                        }
                        else if (current == 'Ĺ')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x4C);
                        }
                        else if (current == 'Ń')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x4E);
                        }
                        else if (current == 'Ó')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x4F);
                        }
                        else if (current == 'Ŕ')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x52);
                        }
                        else if (current == 'Ś')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x53);
                        }
                        else if (current == 'Ú')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x55);
                        }
                        else if (current == 'Ẃ')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x57);
                        }
                        else if (current == 'Ý')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x59);
                        }
                        else if (current == 'Ź')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x5A);
                        }

                        // lowercase with accent aigu
                        else if (current == 'á')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x61);
                        }
                        else if (current == 'ć')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x63);
                        }
                        else if (current == 'é')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x65);
                        }
                        else if (current == 'í')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x69);
                        }
                        else if (current == 'ĺ')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x6C);
                        }
                        else if (current == 'ń')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x6E);
                        }
                        else if (current == 'ó')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x6F);
                        }
                        else if (current == 'ŕ')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x72);
                        }
                        else if (current == 'ś')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x73);
                        }
                        else if (current == 'ú')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x75);
                        }
                        else if (current == 'ẃ')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x77);
                        }
                        else if (current == 'ý')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x79);
                        }
                        else if (current == 'ź')
                        {
                            AddTwo(buffer, ref index, 0x82, 0x7A);
                        }

                        // capitals with accent circonflexe
                        else if (current == 'Â')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x41);
                        }
                        else if (current == 'Ĉ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x43);
                        }
                        else if (current == 'Ê')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x45);
                        }
                        else if (current == 'Ĝ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x47);
                        }
                        else if (current == 'Ĥ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x48);
                        }
                        else if (current == 'Î')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x49);
                        }
                        else if (current == 'Ĵ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x4A);
                        }
                        else if (current == 'Ô')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x4F);
                        }
                        else if (current == 'Ŝ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x53);
                        }
                        else if (current == 'Û')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x55);
                        }
                        else if (current == 'Ŵ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x57);
                        }
                        else if (current == 'Ŷ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x59);
                        }

                        // lowercase with accent circonflexe
                        else if (current == 'â')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x61);
                        }
                        else if (current == 'ĉ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x63);
                        }
                        else if (current == 'ê')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x65);
                        }
                        else if (current == 'ĝ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x67);
                        }
                        else if (current == 'ĥ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x68);
                        }
                        else if (current == 'î')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x69);
                        }
                        else if (current == 'ĵ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x6A);
                        }
                        else if (current == 'ô')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x6F);
                        }
                        else if (current == 'ŝ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x73);
                        }
                        else if (current == 'û')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x75);
                        }
                        else if (current == 'ŵ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x77);
                        }
                        else if (current == 'ŷ')
                        {
                            AddTwo(buffer, ref index, 0x83, 0x79);
                        }

                        // capitals with caron
                        else if (current == 'Ǎ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x41);
                        }
                        else if (current == 'Č')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x43);
                        }
                        else if (current == 'Ď')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x44);
                        }
                        else if (current == 'Ě')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x45);
                        }
                        else if (current == 'Ǧ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x47);
                        }
                        else if (current == 'Ǐ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x49);
                        }
                        else if (current == 'Ľ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x4C);
                        }
                        else if (current == 'Ň')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x4E);
                        }
                        else if (current == 'Ř')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x52);
                        }
                        else if (current == 'Š')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x53);
                        }
                        else if (current == 'Ť')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x54);
                        }
                        else if (current == 'Ž')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x5A);
                        }

                        // lowercase with caron
                        else if (current == 'ǎ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x61);
                        }
                        else if (current == 'č')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x63);
                        }
                        else if (current == 'ď')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x64);
                        }
                        else if (current == 'ě')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x65);
                        }
                        else if (current == 'ǧ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x67);
                        }
                        else if (current == 'ǐ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x69);
                        }
                        else if (current == 'ľ')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x6C);
                        }
                        else if (current == 'ň')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x6E);
                        }
                        else if (current == 'ř')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x72);
                        }
                        else if (current == 'š')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x73);
                        }
                        else if (current == 'ť')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x74);
                        }
                        else if (current == 'ž')
                        {
                            AddTwo(buffer, ref index, 0x84, 0x7A);
                        }

                        // capitals with tilde
                        else if (current == 'Ã')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x41);
                        }
                        else if (current == 'Ĩ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x49);
                        }
                        else if (current == 'Ñ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x4E);
                        }
                        else if (current == 'Õ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x4F);
                        }
                        else if (current == 'Ũ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x55);
                        }

                        // lowercase with tilde
                        else if (current == 'ã')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x61);
                        }
                        else if (current == 'ĩ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x69);
                        }
                        else if (current == 'ñ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x6E);
                        }
                        else if (current == 'õ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x6F);
                        }
                        else if (current == 'ũ')
                        {
                            AddTwo(buffer, ref index, 0x85, 0x75);
                        }

                        // capitals with trema
                        else if (current == 'Ä')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x41);
                        }
                        else if (current == 'Ë')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x45);
                        }
                        else if (current == 'Ï')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x49);
                        }
                        else if (current == 'Ö')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x4F);
                        }
                        else if (current == 'Ü')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x55);
                        }
                        else if (current == 'Ẅ')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x57);
                        }
                        else if (current == 'Ÿ')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x59);
                        }

                        // lowercase with trema
                        else if (current == 'ä')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x61);
                        }
                        else if (current == 'ë')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x65);
                        }
                        else if (current == 'ï')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x69);
                        }
                        else if (current == 'ö')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x6F);
                        }
                        else if (current == 'ü')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x75);
                        }
                        else if (current == 'ẅ')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x77);
                        }
                        else if (current == 'ÿ')
                        {
                            AddTwo(buffer, ref index, 0x86, 0x79);
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

        private static string ReverseAnsi(string text)
        {
            var sb = new StringBuilder();
            var ansi = new StringBuilder();
            foreach (var ch in text)
            {
                if (ch > 255)
                {
                    if (ansi.Length > 0)
                    {
                        sb.Append(Utilities.ReverseString(ansi.ToString()));
                        ansi.Clear();
                    }
                    sb.Append(ch);
                }
                else
                {
                    ansi.Append(ch);
                }
            }
            if (ansi.Length > 0)
            {
                sb.Append(Utilities.ReverseString(ansi.ToString()));
            }

            return sb.ToString();
        }

        private static void AddTwo(byte[] buffer, ref int index, byte b1, byte b2)
        {
            buffer[index] = b1;
            index++;
            buffer[index] = b2;
        }

        private static void WriteTime(Stream fs, TimeCode timeCode)
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
                if (fi.Length >= 512 && fi.Length < 1024000) // not too small or too big
                {
                    if (!fileName.EndsWith(".890", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return base.IsMine(lines, fileName);
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
            {
                _languageIdLine1 = LanguageIdEnglish;
            }

            Configuration.Settings.SubtitleSettings.CurrentCavena890LanguageIdLine1 = _languageIdLine1;

            _languageIdLine2 = buffer[147];
            if (_languageIdLine2 == 0)
            {
                _languageIdLine2 = LanguageIdEnglish;
            }

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

                byte boxType = buffer[start + textLength + 3];

                string line1 = FixText(buffer, start, textLength, _languageIdLine1);
                string line2 = FixText(buffer, start + textLength + 6, textLength, _languageIdLine2);

                if (lastNumber == number)
                {
                    p = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    string temp = (line1.TrimEnd() + Environment.NewLine + line2).TrimEnd();
                    if (temp.Length > 0)
                    {
                        p.Text = temp;
                    }
                }
                else
                {
                    subtitle.Paragraphs.Add(p);
                    p.StartTime.TotalMilliseconds = (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate) * startFrame;
                    p.EndTime.TotalMilliseconds = (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate) * endFrame;
                    p.Text = (line1.TrimEnd() + Environment.NewLine + line2).TrimEnd();
                }
                if (boxType >= 0xa0 && boxType <= 0xa9 && !string.IsNullOrEmpty(p.Text)) // box
                {
                    if (p.Text.StartsWith("{\\") && p.Text.Contains("}"))
                    {
                        p.Text = p.Text.Insert(p.Text.IndexOf('}', 3) + 1, "<box>") + "</box>";
                    }
                    else
                    {
                        p.Text = "<box>" + p.Text + "</box>";
                    }
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
                var encoding = Encoding.GetEncoding(1252);
                var sb = new StringBuilder();
                for (int i = 0; i < textLength; i++)
                {
                    int b = buffer[start + i];
                    int idx = RussianCodes.IndexOf(b);
                    if (idx >= 0)
                    {
                        sb.Append(RussianLetters[idx]);
                    }
                    else
                    {
                        sb.Append(encoding.GetString(buffer, start + i, 1));
                    }
                }

                text = sb.ToString();

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?
                text = FixColors(text);

                if (text.Contains("<i></i>"))
                {
                    text = text.Replace("<i></i>", "<i>");
                }

                if (text.Contains("<i>") && !text.Contains("</i>"))
                {
                    text += "</i>";
                }
            }
            else if (languageId == LanguageIdHebrew) // (_language == "HEBNOA")
            {
                var encoding = Encoding.GetEncoding(1252);
                var sb = new StringBuilder();
                for (int i = 0; i < textLength; i++)
                {
                    int b = buffer[start + i];
                    int idx = HebrewCodes.IndexOf(b);
                    if (idx >= 0)
                    {
                        sb.Append(HebrewLetters[idx]);
                    }
                    else
                    {
                        sb.Append(encoding.GetString(buffer, start + i, 1));
                    }
                }

                text = sb.ToString();

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?
                text = FixColors(text);

                text = ReverseAnsi(text);
                text = Utilities.ReverseStartAndEndingForRightToLeft(text);
            }
            else if (languageId == LanguageIdChineseTraditional || languageId == LanguageIdChineseSimplified) //  (_language == "CCKM44" || _language == "TVB000")
            {
                int index = start;

                while (textLength >= 1 && index + textLength < buffer.Length && (buffer[index + textLength - 1] == 0))
                {
                    textLength--;
                }

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
                text = FixColors(text);
                text = text.Replace(encoding.GetString(new byte[] { 0x88 }), "<i>");
                text = text.Replace(encoding.GetString(new byte[] { 0x98 }), "</i>");

                if (text.Contains("<i></i>"))
                {
                    text = text.Replace("<i></i>", "<i>");
                }

                if (text.Contains("<i>") && !text.Contains("</i>"))
                {
                    text += "</i>";
                }
            }
            else
            {
                var encoding = Encoding.GetEncoding(1252);
                text = encoding.GetString(buffer, start, textLength).Replace("\0", string.Empty);

                text = text.Replace(encoding.GetString(new byte[] { 0x7F }), string.Empty); // Used to fill empty space upto 51 bytes
                text = text.Replace(encoding.GetString(new byte[] { 0xBE }), string.Empty); // Unknown?
                text = FixColors(text);

                text = text.Replace(encoding.GetString(new byte[] { 0x1B }), "æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x1C }), "ø");
                text = text.Replace(encoding.GetString(new byte[] { 0x1D }), "å");
                text = text.Replace(encoding.GetString(new byte[] { 0x1E }), "Æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x1F }), "Ø");

                text = text.Replace(encoding.GetString(new byte[] { 0x5B }), "Æ");
                text = text.Replace(encoding.GetString(new byte[] { 0x5C }), "Ø");
                text = text.Replace(encoding.GetString(new byte[] { 0x5D }), "Å");

                // capitals with accent grave
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x41 }), "À");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x45 }), "È");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x49 }), "Ì");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x4f }), "Ò");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x55 }), "Ù");

                // lowercase with accent grave
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x61 }), "à");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x65 }), "è");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x69 }), "ì");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x6F }), "ò");
                text = text.Replace(encoding.GetString(new byte[] { 0x81, 0x75 }), "ù");

                // capitals with accent aigu
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x41 }), "Á");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x43 }), "Ć");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x45 }), "É");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x49 }), "Í");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x4C }), "Ĺ");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x4E }), "Ń");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x4F }), "Ó");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x52 }), "Ŕ");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x53 }), "Ś");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x55 }), "Ú");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x57 }), "Ẃ");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x59 }), "Ý");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x5A }), "Ź");

                // lowercase with accent aigu
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x61 }), "á");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x63 }), "ć");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x65 }), "é");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x69 }), "í");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x6C }), "ĺ");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x6E }), "ń");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x6F }), "ó");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x72 }), "ŕ");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x73 }), "ś");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x75 }), "ú");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x77 }), "ẃ");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x79 }), "ý");
                text = text.Replace(encoding.GetString(new byte[] { 0x82, 0x7A }), "ź");

                // capitals with accent circonflexe
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x41 }), "Â");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x43 }), "Ĉ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x45 }), "Ê");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x47 }), "Ĝ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x48 }), "Ĥ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x49 }), "Î");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x4A }), "Ĵ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x4F }), "Ô");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x53 }), "Ŝ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x55 }), "Û");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x57 }), "Ŵ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x59 }), "Ŷ");

                // lowercase with accent circonflexe
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x61 }), "â");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x63 }), "ĉ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x65 }), "ê");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x67 }), "ĝ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x68 }), "ĥ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x69 }), "î");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x6A }), "ĵ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x6F }), "ô");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x73 }), "ŝ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x75 }), "û");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x77 }), "ŵ");
                text = text.Replace(encoding.GetString(new byte[] { 0x83, 0x79 }), "ŷ");

                // capitals with caron
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x41 }), "Ǎ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x43 }), "Č");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x44 }), "Ď");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x45 }), "Ě");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x47 }), "Ǧ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x49 }), "Ǐ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x4C }), "Ľ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x4E }), "Ň");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x52 }), "Ř");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x53 }), "Š");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x54 }), "Ť");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x5A }), "Ž");

                // lowercase with caron
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x61 }), "ǎ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x63 }), "č");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x64 }), "ď");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x65 }), "ě");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x67 }), "ǧ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x69 }), "ǐ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x6C }), "ľ");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x6E }), "ň");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x72 }), "ř");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x73 }), "š");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x74 }), "ť");
                text = text.Replace(encoding.GetString(new byte[] { 0x84, 0x7A }), "ž");

                // capitals with tilde
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x41 }), "Ã");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x49 }), "Ĩ");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x4E }), "Ñ");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x4F }), "Õ");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x55 }), "Ũ");

                // lowercase with tilde
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x61 }), "ã");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x69 }), "ĩ");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x6E }), "ñ");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x6F }), "õ");
                text = text.Replace(encoding.GetString(new byte[] { 0x85, 0x75 }), "ũ");

                // capitals with trema
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x41 }), "Ä");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x45 }), "Ë");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x49 }), "Ï");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x4F }), "Ö");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x55 }), "Ü");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x59 }), "Ÿ");

                // lowercase with trema
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x61 }), "ä");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x65 }), "ë");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x69 }), "ï");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x6F }), "ö");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x75 }), "ü");
                text = text.Replace(encoding.GetString(new byte[] { 0x86, 0x79 }), "ÿ");

                // with ring
                text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x61 }), "å");
                text = text.Replace(encoding.GetString(new byte[] { 0x8C, 0x41 }), "Å");

                text = text.Replace(encoding.GetString(new byte[] { 0x88 }), "<i>");
                text = text.Replace(encoding.GetString(new byte[] { 0x98 }), "</i>");

                // ăĂ şŞ ţŢ (romanian)
                text = text.Replace(encoding.GetString(new byte[] { 0x89, 0x61 }), "ă");
                text = text.Replace(encoding.GetString(new byte[] { 0x89, 0x41 }), "Ă");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x73 }), "ş");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x53 }), "Ş");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x74 }), "ţ");
                text = text.Replace(encoding.GetString(new byte[] { 0x87, 0x54 }), "Ţ");

                if (text.Contains("<i></i>"))
                {
                    text = text.Replace("<i></i>", "<i>");
                }

                if (text.Contains("<i>") && !text.Contains("</i>"))
                {
                    text += "</i>";
                }
            }
            return text;
        }

        private static string FixColors(string text)
        {
            Encoding encoding = Encoding.GetEncoding(1252);
            bool fontColorOn = false;
            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                var s = text.Substring(i, 1);
                if (s == encoding.GetString(new byte[] { 0xf1 }))
                {
                    if (fontColorOn)
                    {
                        sb.Append("</font>"); // white
                    }
                    sb.Append("<font color=\"#FF797D\">"); // red
                    fontColorOn = true;
                }
                else if (s == encoding.GetString(new byte[] { 0xf2 }))
                {
                    if (fontColorOn)
                    {
                        sb.Append("</font>"); // white
                    }
                    sb.Append("<font color=\"#AAEF9E\">"); // green
                    fontColorOn = true;
                }
                else if (s == encoding.GetString(new byte[] { 0xf3 }))
                {
                    if (fontColorOn)
                    {
                        sb.Append("</font>"); // white
                    }
                    sb.Append("<font color=\"#FAFAA8\">"); // yellow
                    fontColorOn = true;
                }
                else if (s == encoding.GetString(new byte[] { 0xf4 }))
                {
                    if (fontColorOn)
                    {
                        sb.Append("</font>"); // white
                    }
                    sb.Append("<font color=\"#9999FF\">"); // purple
                    fontColorOn = true;
                }
                else if (s == encoding.GetString(new byte[] { 0xf5 }))
                {
                    if (fontColorOn)
                    {
                        sb.Append("</font>"); // white
                    }
                    sb.Append("<font color=\"#FFABFB\">"); // magenta
                    fontColorOn = true;
                }
                else if (s == encoding.GetString(new byte[] { 0xf6 }))
                {
                    if (fontColorOn)
                    {
                        sb.Append("</font>"); // white
                    }
                    sb.Append("<font color=\"#A2FEFE\">"); // cyan
                    fontColorOn = true;
                }
                else if (s == encoding.GetString(new byte[] { 0xf7 }))
                {
                    if (fontColorOn)
                    {
                        sb.Append("</font>"); // white
                        fontColorOn = false;
                    }
                }
                else if (s == encoding.GetString(new byte[] { 0xf8 }))
                {
                    sb.Append("<font color=\"#FCC786\">"); // orange
                    fontColorOn = true;
                }
                else
                {
                    sb.Append(s);
                }
            }
            if (fontColorOn)
            {
                sb.Append("</font>"); // white
            }
            return sb.ToString();
        }

    }
}
