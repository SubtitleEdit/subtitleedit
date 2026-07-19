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
            public static readonly Regex DanishLetterIRegex = new Regex(@"\bi(dag|går|morgen|alt|gang|stand|øvrigt)\b", RegexOptions.Compiled);

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
                    RegexFactory(@"\b[Hh]vis i (?:altså|bare|forstår|får|går|ikke|lovede|lover|overholder|overtræder|slipper|taber|vandt|vinder|\n)\b"),
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
                    RegexFactory(@"\b[Ss]kal i (?:alle|ikke|allesammen|begge dø|bare|dele|dø|hilse|se|gøre|lave|høre|kaste|fordele|fordeles|fortælle|gøre|have|ikke|klare|klatre|larme|lave|løfte|med|på|til|ud)\b"),
                    RegexFactory(@"\b[Ss]lap i (?:ud|væk)\b"),
                    RegexFactory(@"\b[Ss]nart er i\b"),
                    RegexFactory(@"\b[Ss]om i (?:måske|nok|ved)\b"),
                    RegexFactory(@"\b[Ss]pis i (?:bare|dem)\b"),
                    RegexFactory(@"\b[Ss]ynes i (?:at|det)\b"),
                    RegexFactory(@"\b[Ss]ynes i\b"), // Remove line above if this one should be kept.
                    RegexFactory(@"\b[Ss]ætter i en\b"),
                    RegexFactory(@"\bSå i (?:at|det|noget|sad)\b"),
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

        public static string BuildWholeWordPattern(string searchText)
        {
            var escaped = Regex.Escape(searchText);
            var prefix = searchText.Length > 0 && (char.IsLetterOrDigit(searchText[0]) || searchText[0] == '_') ? @"\b" : @"(?<!\w)";
            var suffix = searchText.Length > 0 && (char.IsLetterOrDigit(searchText[searchText.Length - 1]) || searchText[searchText.Length - 1] == '_') ? @"\b" : @"(?!\w)";
            return $"{prefix}{escaped}{suffix}";
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
        /// Normalizes the new-line escapes a user typed in a regular expression (<c>\r\n</c>, <c>\n</c>
        /// or <c>\r</c>) to a single line-feed, so a rule matches regardless of which escape was typed
        /// and regardless of the platform line-ending. Subtitle text is matched line-feed normalized too
        /// (see <see cref="ReplaceNewLineSafe"/>), keeping the pattern and the text in sync. The previous
        /// behavior expanded these to Environment.NewLine (CRLF on Windows), which no longer matched the
        /// line-feed text used in v5, so cross-line regex rules silently stopped working (#11956).
        /// </summary>
        public static string FixNewLine(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return pattern;
            }

            return pattern.Replace("\\r\\n", "\n").Replace("\\n", "\n").Replace("\\r", "\n");
        }

        public static string EscapeNewLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text.Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "\\n");
        }

        /// <summary>
        /// Performs replace on regular expression. Line breaks are normalized to "\n" for both the match
        /// and the returned text, so the pattern and the text agree regardless of \n vs \r\n.
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
        /// Performs replace on regular expression. The match runs against line-feed-normalized text,
        /// so the pattern's \n (see <see cref="FixNewLine"/>) matches regardless of \n vs \r\n; the
        /// returned text uses <see cref="Environment.NewLine"/> like all other paragraph text.
        /// </summary>
        /// <param name="regularExpression">Regular expression to perform replace on</param>
        /// <param name="text">Text perform replace on</param>
        /// <param name="replaceWith">Pattern to replace with (new lines should be "\n")</param>
        /// <param name="count">Maximum number of times replacement can occur</param>
        /// <param name="startIndex">Starting index of replace operation</param>
        /// <returns>Input string with regular expression replace applied</returns>
        public static string ReplaceNewLineSafe(Regex regularExpression, string text, string replaceWith, int count, int startIndex)
        {
            // Match with line-feed-normalized line breaks so a pattern's \n (see FixNewLine)
            // matches regardless of whether the in-memory text uses \n or \r\n (#11956).

            // Fast path (runs per replace rule per subtitle line): text without \r or U+2028 round-trips
            // unchanged through the SplitToLines+Join normalization, so skip the four allocations.
            if (!ContainsNonLineFeedNewLine(text) && !ContainsNonLineFeedNewLine(replaceWith))
            {
                return RestorePlatformNewLines(regularExpression.Replace(text, replaceWith, count, startIndex));
            }

            text = regularExpression.Replace(string.Join("\n", text.SplitToLines()), replaceWith, count, startIndex);
            return RestorePlatformNewLines(text);
        }

        /// <summary>
        /// Re-joins line-feed-normalized text with <see cref="Environment.NewLine"/>. The rest of
        /// libse keeps paragraph text with platform newlines, and the format writers rely on it -
        /// e.g. ASSA folds line breaks via <c>Replace(Environment.NewLine, "\\N")</c>, so a bare
        /// \n left behind by the match normalization above was written as a physical newline
        /// inside the Dialogue line, corrupting the saved file on Windows (#12620).
        /// </summary>
        private static string RestorePlatformNewLines(string text)
        {
            if (Environment.NewLine == "\n" || text.IndexOf('\n') < 0)
            {
                return text;
            }

            return string.Join(Environment.NewLine, text.SplitToLines());
        }

        public static int CountNewLineSafe(Regex regularExpression, string text)
        {
            if (!ContainsNonLineFeedNewLine(text))
            {
                return regularExpression.Matches(text).Count;
            }

            return regularExpression.Matches(string.Join("\n", text.SplitToLines())).Count;
        }

        private static bool ContainsNonLineFeedNewLine(string text)
        {
            return text.IndexOf('\r') >= 0 || text.IndexOf('\u2028') >= 0;
        }
    }
}
