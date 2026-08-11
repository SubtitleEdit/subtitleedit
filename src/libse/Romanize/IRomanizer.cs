using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    public interface IRomanizer
    {
        private static readonly DevanagariRomanizer _Devanagari = new DevanagariRomanizer();
        private static readonly KanaRomanizer _Kana = new KanaRomanizer();
        private static readonly GeezRomanizer _Geez = new GeezRomanizer();
        private static readonly GreekRomanizer _Greek = new GreekRomanizer();
        private static readonly HangulRomanizer _Hangul = new HangulRomanizer();
        private static readonly CyrillicRomanizer _Cyrillic = new CyrillicRomanizer();

        private static IRomanizer GetRomanizerForChar(char ch)
        {
            if (_Devanagari.IsValid(ch)) return _Devanagari;
            if (_Kana.IsValid(ch)) return _Kana;
            if (_Geez.IsValid(ch)) return _Geez;
            if (_Greek.IsValid(ch)) return _Greek;
            if (_Hangul.IsValid(ch)) return _Hangul;
            if (_Cyrillic.IsValid(ch)) return _Cyrillic;
            
            return null;
        }

        RomanizerLanguages Language { get; }

        bool IsValid(char chr);
        bool IsValid(string text);
        string Romanize(string text);

        public static string RomanizeText(string text, params RomanizerLanguages[] exclude)
        {
            return RomanizeText(text, exclude as IEnumerable<RomanizerLanguages>);
        }
        public static string RomanizeText(string text, IEnumerable<RomanizerLanguages> exclude)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new System.Text.StringBuilder(text.Length);
            var i = 0;
            while (i < text.Length)
            {
                var romanizer = GetRomanizerForChar(text[i]);
                if (romanizer == null || exclude.Contains(romanizer.Language) is false)
                {
                    sb.Append(text[i]);
                    i++;
                    continue;
                }

                var start = i;
                while (i < text.Length && GetRomanizerForChar(text[i]) == romanizer)
                {
                    i++;
                }

                sb.Append(romanizer.Romanize(text.Substring(start, i - start)));
            }

            return sb.ToString();
        }
    }

    public enum RomanizerLanguages
    {
        Cyrillic,
        Devanagari,
        Geez,
        Greek,
        Hangul,
        Kana,
    }
}