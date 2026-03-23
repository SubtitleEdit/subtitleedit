using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public static class TranslationHelper
    {
        public static string PostTranslate(string input, string target)
        {
            var s = input;
            if (target == "da")
            {
                s = s.Replace("Jeg ved.", "Jeg ved det.");
                s = s.Replace(", jeg ved.", ", jeg ved det.");

                s = s.Replace("Jeg er ked af.", "Jeg er ked af det.");
                s = s.Replace(", jeg er ked af.", ", jeg er ked af det.");

                s = s.Replace("Come on.", "Kom nu.");
                s = s.Replace(", come on.", ", kom nu.");
                s = s.Replace("Come on,", "Kom nu,");

                s = s.Replace("Hey ", "Hej ");
                s = s.Replace("Hey,", "Hej,");

                s = s.Replace(" gonna ", " ville ");
                s = s.Replace("Gonna ", "Vil ");

                s = s.Replace("Ked af.", "Undskyld.");

                s = s.Replace("Vente.", "Vent.");
                s = s.Replace("Vente,", "Vent,");
            }

            return FixTags(s);
        }

        private static string FixTags(string s)
        {
            return s.Replace("< font ", "<font ")
            .Replace(" color = ", " color=")
            .Replace(" color =", " color=")
            .Replace("color= \"# ", " color=\"#")
            .Replace("color= \"#", " color=\"#")
            .Replace("</ font >", "</font>")
            .Replace("</ font>", "</font>")

            .Replace("< i >", "<i>")
            .Replace("< / i >", "</i>")
            .Replace("</ i>", "</i>");
        }

        public static string PreTranslate(string input, string source)
        {
            string s = FixInvalidCarriageReturnLineFeedCharacters(input);

            if (source == "en")
            {
                s = Regex.Replace(s, @"\bI'm ", "I am ");
                s = Regex.Replace(s, @"\bI've ", "I have ");
                s = Regex.Replace(s, @"\bI'll ", "I will ");
                s = Regex.Replace(s, @"\b(I|i)t's ", "$1t is ");
                s = Regex.Replace(s, @"\b(Y|y)ou're ", "$1ou are ");
                s = Regex.Replace(s, @"\b(Y|y)ou've ", "$1ou have ");
                s = Regex.Replace(s, @"\b(Y|y)ou'll ", "$1ou will ");
                s = Regex.Replace(s, @"\b(H|h)e's ", "$1e is ");
                s = Regex.Replace(s, @"\b(S|s)he's ", "$1he is ");
                s = Regex.Replace(s, @"\b(W|w)e're ", "$1e are ");
                s = Regex.Replace(s, @"\bwon't ", "will not ");
                s = Regex.Replace(s, @"\bdon't ", "do not ");
                s = Regex.Replace(s, @"\bDon't ", "Do not ");
                s = Regex.Replace(s, @"\b(W|w)e're ", "$1e are ");
                s = Regex.Replace(s, @"\b(T|t)hey're ", "$1hey are ");
                s = Regex.Replace(s, @"\b(W|w)ho's ", "$1ho is ");
                s = Regex.Replace(s, @"\b(T|t)hat's ", "$1hat is ");
                s = Regex.Replace(s, @"\b(W|w)hat's ", "$1hat is ");
                s = Regex.Replace(s, @"\b(W|w)here's ", "$1here is ");
                s = Regex.Replace(s, @"\b(W|w)ho's ", "$1ho is ");
                s = Regex.Replace(s, @"\B'(C|c)ause ", "$1ecause "); // \b (word boundry) does not work with '
            }
            return s;
        }

        private static string FixInvalidCarriageReturnLineFeedCharacters(string input)
        {
            // Fix new line chars (avoid "Specified value has invalid CRLF characters")
            // See https://github.com/SubtitleEdit/subtitleedit/issues/4589
            return string.Join(Environment.NewLine, input.SplitToLines()).Trim();
        }
    }
}
