using Nikse.SubtitleEdit.Core.Interfaces;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixCommas : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var commaDouble = new Regex(@"([\p{L}\d\s]),,([\p{L}\d\s])");
            var commaTriple = new Regex(@"([\p{L}\d\s]) *, *, *,([\p{L}\d\s])");
            var commaTripleEndOfLine = new Regex(@"([\p{L}\d\s]), *, *,$");
            var commaWhiteSpaceBetween = new Regex(@"([\p{L}\d\s]),\s+,([\p{L}\d\s])");
            var commaFollowedByLetter = new Regex(@",(\p{L})");

            // cache for inverted regex replacement
            IDictionary<Regex, string> invertedRegex = null;

            string fixAction = Configuration.Settings.Language.FixCommonErrors.FixCommas;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var s = p.Text;
                    var oldText = s;

                    if (HashComma(p.Text))
                    {
                        s = commaDouble.Replace(s, "$1,$2");
                        s = commaTriple.Replace(s, "$1...$2");
                        s = commaTripleEndOfLine.Replace(s, "$1...");
                        s = commaWhiteSpaceBetween.Replace(s, "$1,$2");
                        s = commaFollowedByLetter.Replace(s, ", $1");

                        s = RemovePrefixCommaSymbol(s, ',');
                    }

                    if (HashInvertedComma(p.Text))
                    {
                        invertedRegex = invertedRegex ?? GetInvertedRegexList();
                        foreach (var kvp in invertedRegex)
                        {
                            s = kvp.Key.Replace(s, kvp.Value);
                        }

                        s = RemovePrefixCommaSymbol(s, '،');
                    }

                    if (oldText != s)
                    {
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, s);
                        p.Text = s;
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, Configuration.Settings.Language.FixCommonErrors.FixCommas, fixCount.ToString());
        }

        private IDictionary<Regex, string> GetInvertedRegexList()
        {
            return new Dictionary<Regex, string>
            {
                { new Regex(@"([\p{L}\d\s]) *،،([\p{L}\d\s])", RegexOptions.Compiled | RegexOptions.RightToLeft), "$1،$2"},
                { new Regex(@"([\p{L}\d\s]) *، *، *،([\p{L}\d\s])", RegexOptions.Compiled | RegexOptions.RightToLeft), "$1...$2"},
                { new Regex(@"([\p{L}\d\s])( *، *، *،)$", RegexOptions.Compiled | RegexOptions.RightToLeft), "$1..."},
                { new Regex(@"([\p{L}\d\s]) *،\s+،([\p{L}\d\s])", RegexOptions.Compiled | RegexOptions.RightToLeft), "$1،$2"},
                { new Regex(@"،(\p{L})", RegexOptions.Compiled | RegexOptions.RightToLeft), "، $1" }
            };
        }

        private string RemovePrefixCommaSymbol(string s, char comma)
        {
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char ch = s[i];
                if (i - 1 >= 0 && s[i - 1] == comma && IsCloseChar(ch))
                {
                    int k = i;

                    do
                    {
                        i--;
                    } while (i - 1 >= 0 && s[i - 1] == comma);

                    // remove commas
                    if (k - i > 0)
                    {
                        s = s.Remove(i, k - i);
                    }
                }
            }

            return s;
        }

        private static bool IsCloseChar(char ch) => ch == '.' || ch == '!' || ch == '?' || ch == ')' || ch == ']';

        private static bool HashComma(string input) => input.Contains(',');

        private static bool HashInvertedComma(string input) => input.Contains('،');

    }
}
