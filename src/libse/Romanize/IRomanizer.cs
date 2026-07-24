using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    public interface IRomanizer
    {
        private static readonly JapaneseRomanizer _Japanese = new JapaneseRomanizer();
        private static readonly KoreanRomanizer _Korean = new KoreanRomanizer();
        private static readonly RussianRomanizer _Russian = new RussianRomanizer();

        private static IRomanizer GetRomanizerForChar(char ch)
        {
            if (_Japanese.IsValid(ch)) return _Japanese;
            if (_Korean.IsValid(ch)) return _Korean;
            if (_Russian.IsValid(ch)) return _Russian;
            
            return null;
        }

        CultureInfo Culture { get; }

        bool IsValid(char chr);
        bool IsValid(string text);
        string Romanize(string text);

        public static string RomanizeText(string text, params CultureInfo[] exclude)
        {
            return RomanizeText(text, exclude as IEnumerable<CultureInfo>);
        }
        public static string RomanizeText(string text, IEnumerable<CultureInfo> exclude)
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
                if (romanizer == null || exclude.Contains(romanizer.Culture) is false)
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
}