using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.UiLogic.Translate
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

        private static readonly (Regex Regex, string Replacement)[] EnglishContractions =
        {
            (new Regex(@"\bI'm ", RegexOptions.Compiled), "I am "),
            (new Regex(@"\bI've ", RegexOptions.Compiled), "I have "),
            (new Regex(@"\bI'll ", RegexOptions.Compiled), "I will "),
            (new Regex(@"\b(I|i)t's ", RegexOptions.Compiled), "$1t is "),
            (new Regex(@"\b(Y|y)ou're ", RegexOptions.Compiled), "$1ou are "),
            (new Regex(@"\b(Y|y)ou've ", RegexOptions.Compiled), "$1ou have "),
            (new Regex(@"\b(Y|y)ou'll ", RegexOptions.Compiled), "$1ou will "),
            (new Regex(@"\b(H|h)e's ", RegexOptions.Compiled), "$1e is "),
            (new Regex(@"\b(S|s)he's ", RegexOptions.Compiled), "$1he is "),
            (new Regex(@"\b(W|w)e're ", RegexOptions.Compiled), "$1e are "),
            (new Regex(@"\bwon't ", RegexOptions.Compiled), "will not "),
            (new Regex(@"\bdon't ", RegexOptions.Compiled), "do not "),
            (new Regex(@"\bDon't ", RegexOptions.Compiled), "Do not "),
            (new Regex(@"\b(W|w)e're ", RegexOptions.Compiled), "$1e are "),
            (new Regex(@"\b(T|t)hey're ", RegexOptions.Compiled), "$1hey are "),
            (new Regex(@"\b(W|w)ho's ", RegexOptions.Compiled), "$1ho is "),
            (new Regex(@"\b(T|t)hat's ", RegexOptions.Compiled), "$1hat is "),
            (new Regex(@"\b(W|w)hat's ", RegexOptions.Compiled), "$1hat is "),
            (new Regex(@"\b(W|w)here's ", RegexOptions.Compiled), "$1here is "),
            (new Regex(@"\b(W|w)ho's ", RegexOptions.Compiled), "$1ho is "),
            (new Regex(@"\B'(C|c)ause ", RegexOptions.Compiled), "$1ecause "),
        };

        public static string PreTranslate(string input, string source)
        {
            string s = FixInvalidCarriageReturnLineFeedCharacters(input);

            if (source == "en" && s.IndexOf('\'') >= 0)
            {
                // Every pattern needs an apostrophe, so lines without one skip the whole table.
                // These were 21 static Regex.Replace calls per line - more distinct patterns than
                // Regex.CacheSize (15) holds, so the cache thrashed and re-parsed them.
                foreach (var (regex, replacement) in EnglishContractions)
                {
                    s = regex.Replace(s, replacement);
                }
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
