using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class RegexUtils
    {
        // Others classes may want to use this regex.
        public static readonly Regex LittleIRegex = new Regex(@"\bi\b", RegexOptions.Compiled);

        /// <summary>
        /// Will be constructed only once.
        /// </summary>
        public static class DanishLetterI
        {
            public static readonly Regex RegExIDag = new Regex(@"\bidag\b", RegexOptions.Compiled);
            public static readonly Regex RegExIGaar = new Regex(@"\bigår\b", RegexOptions.Compiled);
            public static readonly Regex RegExIMorgen = new Regex(@"\bimorgen\b", RegexOptions.Compiled);
            public static readonly Regex RegExIAlt = new Regex(@"\bialt\b", RegexOptions.Compiled);
            public static readonly Regex RegExIGang = new Regex(@"\bigang\b", RegexOptions.Compiled);
            public static readonly Regex RegExIStand = new Regex(@"\bistand\b", RegexOptions.Compiled);
            public static readonly Regex RegExIOevrigt = new Regex(@"\biøvrigt\b", RegexOptions.Compiled);

            private static readonly IList<Regex> RegexList;

            public static IEnumerable<Regex> DanishCompiledRegexList => RegexList;

            static DanishLetterI()
            {
                // Not a complete list, more phrases will come.
                RegexList = new[]
                {
                    RegexFactory(@"\b, er i alle\b"),
                    RegexFactory(@", i (?:ved nok|ved, |ved.|ikke blev)\b"),
                    RegexFactory(@"\b i føler at\b"),
                    RegexFactory(@"\badvarede i (?:os|dem)\b"),
                    RegexFactory(@"\bat i (?:aldrig|alle bliver|alle er|alle forventer|alle gør|alle har|alle ved|alle vil|bare|bager|bruger|dræber|dræbte|fandt|fik|finder|forstår|får|ikke|kom|kommer|næsten er|næsten fik|næsten har|næsten skulle|næsten var|også får|også gør|også mener|også siger|også tror|rev|river|samarbejder|snakkede|scorer|siger|skal|skulle|to ikke|to siger|to har|to er|to bager|to skal|to gør|to får|udnyttede|udnytter|vil|ville)\b"),
                    RegexFactory(@"\b[Aa]t i (?:hver især|ikke)\b"),
                    RegexFactory(@"\b[Bb]ehandler i mig\b"),
                    RegexFactory(@"\bbliver i (?:rige|ikke|indkvarteret|indlogeret)\b"),
                    RegexFactory(@"\bburde i (?:gøre|ikke|købe|løbe|se|sig|tage)\b"),
                    RegexFactory(@"\bfanden vil i\?"),
                    RegexFactory(@"\b[Ff]or ser i\b"),
                    RegexFactory(@"\b[Db]a i (?:ankom|forlod|fik|gik|kom|dræbte|ikke|gjorde|har|havde|så)\b"),
                    RegexFactory(@"\b[Dd]et (?:får i|har i|må i|gør i|gjorde i)\b"),
                    RegexFactory(@"\b[Dd]et må i (?:fandme|ikke|sgu|gerne|selv|da|faktisk)\b"),
                    RegexFactory(@"\b[Dd]et Det kan i sgu"),
                    RegexFactory(@"\bend i (?:aner|tror|ved)\b"),
                    RegexFactory(@"\bellers får i "),
                    RegexFactory(@"\b[Ee]r i (?:alle|allerede|allesammen|der|fra|gennem|glade|gået|her|imod|klar|mætte|med|mod|okay|på|parate|sikker|sikre|skøre|stadig|sultne|tilfredse|to|ved at|virkelig|vågne)\b"),
                    RegexFactory(@"\b[Ff]ordi i (?:ventede|deltog)\b"),
                    RegexFactory(@"\b[Ff]orstår i\b"),
                    RegexFactory(@"\b[Ff]orhandler i stadig"),
                    RegexFactory(@"\b[Ff]ør i (?:får|kommer|tager|alle|fratrukket|ikke|klø|point)\b"), // Note: Some combination weren't using both (f and F) is it okay?
                    RegexFactory(@"\b[Gg]å i bare\b"),
                    RegexFactory(@"\b[Gg]år i ind\b"),
                    RegexFactory(@"\b[Gg]ider i (?:at|ikke|lige|godt)\b"),
                    RegexFactory(@"\b[Gg]ik i (?:lige|hjem|over|forbi|ind|uden)\b"),
                    RegexFactory(@"\b[Gg]jorde i (?:det|ikke)\b"),
                    RegexFactory(@"\b[Gg]lor i (?:på|allesammen på|alle på)\b"),
                    RegexFactory(@"\b[Gg]iver i mig\b"),
                    RegexFactory(@"\b[Hh]ørte i det\b"),
                    RegexFactory(@"\b[Hh]ører i(?:, | ikke)\b"),
                    RegexFactory(@"\b[Hh]ar i (?:ødelagt|fået|det|gjort|ikke|nogen|nok|ordnet|spist|tænkt|tabt)\b"),
                    RegexFactory(@"\bHar i\b"),
                    RegexFactory(@"\bhelvede vil i\?"),
                    RegexFactory(@"\b[Hh]older i (?:fast|godt fast)\b"),
                    RegexFactory(@"\b[Hh]er har i\b"),
                    RegexFactory(@"\b[Hh]vad fanden (?:har|tror|vil|gør|har|) i\b"),
                    RegexFactory(@"\b[Hh]vad i ikke\b"),
                    RegexFactory(@"\b[Hh]vad (?:laver|lavede|mener|siger|skal|snakker|sløver|synes|vil) i\b"),
                    RegexFactory(@"\b[Hh]vem (?:er|fanden tror|tror) i\b"),
                    RegexFactory(@"\b[Hh]vilken slags (?:mennesker|folk) er i\?"),
                    RegexFactory(@"\b[Hh]vis i (?:altså|bare|forstår|får|går|ikke|lovede|lover|overholder|overtræder|slipper|taber|vandt|vinder)\b"),
                    RegexFactory(@"\b[Hh]vor (?:er|får|gamle er) i\b"),
                    RegexFactory(@"\b[Hh]vor i (?:begyndte|startede)\b"),
                    RegexFactory(@"\b[Hh]vor (?:skal|var) i\b"),
                    RegexFactory(@"\b[Hh]vordan (?:har|hørte|kunne) i\b"),
                    RegexFactory(@"\b[Hh]vordan i (?:når|nåede)\b"),
                    RegexFactory(@"\b[Hh]vorfor afleverer i det\b"),
                    RegexFactory(@"\b[Hh]vorfor (?:gør|gjorde|græder|har|kom|kommer|løb|lover|lovede|skal|skulle|sagde|synes) i\b"),
                    RegexFactory(@"\b[Hh]vornår (?:kom|ville|giver|gav|rejser|skal|skulle) i\b"),
                    RegexFactory(@"\bi (?:altid|ankomme|ankommer|bare kunne|bare havde|bare gjorde|begge er|begge gør|begge har|begge var|begge vill|behøver ikke gemme|behøver ikke prøve|behøver ikke skjule|behandlede|behandlede|behandler|beskidte dyr|blev|blive|bliver|burde|er|fyrer|gør|gav|gerne|giver|gjorde|hører|hørte|har|havde|igen bliver|igen burder|igen finder|igen gør|igen kommer|igen prøver|igen siger|igen skal|igen vil|ikke gerne|ikke kan|ikke kommer|ikke vil|kan|kender|kom|komme|kommer|kunne|morer jer|må gerne|må give|må da|nåede|når|prøve|prøvede|prøver|sagde|scorede|ser|set|siger|sikkert alle|sikkert ikke gør|sikkert ikke kan|sikkert ikke vil|skal|skulle|små stakler|stopper|synes|troede|tror|var|vel|vil|ville)\b"),
                    RegexFactory(@"\b[Kk]an i (?:lugte|overleve|spise|se|smage|forstå|godt|gøre|huske|ikke|lide|leve|love|måske|nok|se|sige|tilgive|tygge|to ikke|tro)\b"), // Review
                    RegexFactory(@"\b[Kk]ørte i (?:hele|ikke)\b"),
                    RegexFactory(@"\b[Kk]ender i (?:hinanden|to hinanden)\b"),
                    RegexFactory(@"\bKender i "),
                    RegexFactory(@"\b[Kk]endte i (?: hinanden)?\b"),
                    RegexFactory(@"\b[Kk](?:iggede|igger) i på"),
                    RegexFactory(@"\b[Kk]ommer i (?:her|ofte|sammen|tit)\b"),
                    RegexFactory(@"\b[Kk]unne i (?:fortælle|give|gøre|ikke|lide|mødes|se)\b"),
                    RegexFactory(@"\b[Ll]eder i efter\b"),
                    RegexFactory(@"\b[Ll]aver i (?:ikke|her)\b"),
                    RegexFactory(@"\b[Ll]igner i (?:far|hinanden|mor)\b"),
                    RegexFactory(@"\b[Ll]aver i ikke\b"),
                    RegexFactory(@"\blaver i her\b"),
                    RegexFactory(@"\bLover i\b"),
                    RegexFactory(@"\b[Ll]ykke(?:s|des) i med\b"),
                    RegexFactory(@"\b[Ll]øb i hellere\b"),
                    RegexFactory(@"\b[Mm]ødte i "),
                    RegexFactory(@"\b[Mm]angler i en\b"),
                    RegexFactory(@"\b[Mm]en i (?:gutter|drenge|fyre|står)\b"),
                    RegexFactory(@"\b[Mm]ener i (?:at|det|virkelig)\b"),
                    RegexFactory(@"\b[Mm]ens i (?:sov|stadig|lå)\b"),
                    RegexFactory(@"\b[Mm]ister i point\b"),
                    RegexFactory(@"\b[Mm]å i (?:alle|gerne|godt|vide|ikke)\b"),
                    RegexFactory(@"\b[Nn]u (?:løber|siger|skal) i\b"),
                    RegexFactory(@"\b[Nn]år i\b"),
                    RegexFactory(@"\b[Oo]m i ikke\b"),
                    RegexFactory(@"\b[Oo]pgiver i\b"),
                    RegexFactory(@"\b[Oo]vergiver i jer\b"),
                    RegexFactory(@"\bpersoner i lukker\b"),
                    RegexFactory(@"\b[Pp]å (?:i ikke|at i ikke)\b"),
                    RegexFactory(@"\b[Ss]agde i ikke\b"),
                    RegexFactory(@"\b[Ss]amlede i ham\b"),
                    RegexFactory(@"\bS(?:er|iger) i\b"),
                    RegexFactory(@"\b[Ss]ik(?:ker|re) på i ikke\b"), // Sikker or Sikre...
                    RegexFactory(@"\b[Ss]kal i (?:alle|allesammen|begge dø|bare|dele|dø|hilse|se|gøre|lave|høre|kaste|fordele|fordeles|fortælle|gøre|have|ikke|klare|klatre|larme|lave|løfte|med|på|til|ud)\b"),
                    RegexFactory(@"\b[Ss]lap i (?:ud|væk)\b"),
                    RegexFactory(@"\b[Ss]nart er i\b"),
                    RegexFactory(@"\b[Ss]om i (?:måske|nok|ved)\b"),
                    RegexFactory(@"\b[Ss]pis i (?:bare|dem)\b"),
                    RegexFactory(@"\b[Ss]ynes i (?:at|det)\b"),
                    RegexFactory(@"\b[Ss]ynes i\b"), // Remove line above if this one should be kept.
                    RegexFactory(@"\b[Ss]ætter i en\b"),
                    RegexFactory(@"\bSå i (?:at|det|noget)\b"),
                    RegexFactory(@"\b[Ss]å tager i\b"),
                    RegexFactory(@"\b[Tt]ænder i på\b"),
                    RegexFactory(@"\b[Tt]og i (?:bilen|liften|toget)\b"),
                    RegexFactory(@"\b[Tt]ræder i frem\b"),
                    RegexFactory(@"\b[Tt]ror i (?:at|det|jeg|på|virkelig)\b"),
                    RegexFactory(@"\b[Tr]ror i(?:, | på\b)"),
                    RegexFactory(@"\b[Vv]ar i blevet\b"),
                    RegexFactory(@"\b[Vv]ed i (?:alle|allesammen|er|ikke|hvad|hvem|hvor|hvorfor|hvordan|var|ville|har|havde|hvem|hvad|hvor|mente|tror)\b"),
                    RegexFactory(@"\b[Vv]enter i på\b"),
                    RegexFactory(@"\b[Ff]orventer i\b"),
                    RegexFactory(@"\b[Vv]il i (?:besegle|dræbe|fjerne|fortryde|gerne|godt|have|høre|ikke|købe|kaste|møde|måske|savne|se|sikkert|smage|virkelig|være)\b"),
                    RegexFactory(@"\b[Vv]ille i (?:blive|dræbe|få|gøre|høre|ikke|kaste|komme|mene|nå|savne|se|sikkert|synes|tage|tro|være)\b"),
                    RegexFactory(@"\b[Vv]iste i(?:, at| at)\b"),
                    RegexFactory(@"\bvover i\b"),
                    RegexFactory(@"\bføler i (?:ingen|ingenting|at|absolut|slet|ikke|sikkert|også)\b"),
                };
            }

            /// <summary>
            /// Returns an instance of compiled regex using given pattern.
            /// </summary>
            /// <param name="pattern">Pattern to be used to create instance of Regex.</param>
            /// <returns>Compiled regex using given pattern.</returns>
            private static Regex RegexFactory(string pattern)
            {
                pattern = ExpandWhiteSpace(pattern);
                return new Regex(pattern, RegexOptions.Compiled);
            }

            /// <summary>
            /// Converts any White-Space (U+0020) present in pattern to match: White-Space (U+0020),
            /// Carriage-Return (U+000D) and Line-Feed (U+000A) minimum once or all its successors.
            /// </summary>
            private static string ExpandWhiteSpace(string pattern) => pattern.Replace(" ", "[ \r\n]+");
        }

        public static bool IsValidRegex(string testPattern)
        {
            if (string.IsNullOrEmpty(testPattern))
            {
                return false;
            }
            try
            {
                Regex.Match(string.Empty, testPattern);
                return true;
            }
            catch (ArgumentException) // invalid pattern e.g: [
            {
                return false;
            }
        }

        public static Regex MakeWordSearchRegex(string word)
        {
            string s = word.Replace("\\", "\\\\");
            s = s.Replace("*", "\\*");
            s = s.Replace(".", "\\.");
            s = s.Replace("?", "\\?");
            return new Regex(@"\b" + s + @"\b", RegexOptions.Compiled);
        }

        public static Regex MakeWordSearchRegexWithNumbers(string word)
        {
            string s = word.Replace("\\", "\\\\");
            s = s.Replace("*", "\\*");
            s = s.Replace(".", "\\.");
            s = s.Replace("?", "\\?");
            return new Regex(@"[\b ,\.\?\!]" + s + @"[\b !\.,\r\n\?]", RegexOptions.Compiled);
        }

        public static string GetRegExGroup(string pattern)
        {
            var start = pattern.IndexOf("(?<", StringComparison.Ordinal);
            if (start < 0)
            {
                return null;
            }

            start += 3;
            var end = pattern.IndexOf('>', start);
            if (end <= start)
            {
                return null;
            }

            return pattern.Substring(start, end - start);
        }

        /// <summary>
        /// Changes "\\r\\n" and "\\n" to "\n", which hopefully makes it simpler for
        /// the user who can use both "\\n" and "\\r\\n" for new line.
        /// </summary>
        public static string FixNewLine(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return pattern;
            }

            return pattern.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
        }

        /// <summary>
        /// Performs replace on regular expression. Line breaks are converted to just "\n" during the replace
        /// and line breaks are returned as Environment.NewLine.
        /// </summary>
        /// <param name="regularExpression">Regular expression to perform replace on</param>
        /// <param name="text">Text perform replace on</param>
        /// <param name="replaceWith">Pattern to replace with (new lines should be "\n")</param>
        /// <returns>Input string with regular expression replace applied</returns>
        public static string ReplaceNewLineSafe(Regex regularExpression, string text, string replaceWith)
        {
            return ReplaceNewLineSafe(regularExpression, text, replaceWith, int.MaxValue, 0);
        }

        /// <summary>
        /// Performs replace on regular expression. Line breaks are converted to just "\n" during the replace
        /// and line breaks are returned as Environment.NewLine.
        /// </summary>
        /// <param name="regularExpression">Regular expression to perform replace on</param>
        /// <param name="text">Text perform replace on</param>
        /// <param name="replaceWith">Pattern to replace with (new lines should be "\n")</param>
        /// <param name="count">Maximum number of times replacement can occur</param>
        /// <param name="startIndex">Starting index of replace operation</param>
        /// <returns>Input string with regular expression replace applied</returns>
        public static string ReplaceNewLineSafe(Regex regularExpression, string text, string replaceWith, int count, int startIndex)
        {
            text = regularExpression.Replace(string.Join(Environment.NewLine, text.SplitToLines()), replaceWith, count, startIndex);
            return string.Join(Environment.NewLine, text.SplitToLines());
        }

    }
}
