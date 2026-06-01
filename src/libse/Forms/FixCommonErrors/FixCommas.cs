using System;
using Nikse.SubtitleEdit.Core.Interfaces;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixCommas : IFixCommonError
    {
        public static class Language
        {
            public static string FixCommas { get; set; } = "Fix commas";
        }

        // Hoisted out of Fix() so each Fix() call reuses the same compiled NFA instead of
        // recompiling per-call. The Arabic variants below used to be re-allocated *per
        // paragraph* — for a 1000-line Arabic subtitle that's ~5000 unnecessary Regex+compile
        // cycles per FCE run.
        private static readonly Regex CommaDouble = new Regex(@"([\p{L}\d\s]),,([\p{L}\d\s])", RegexOptions.Compiled);
        private static readonly Regex CommaTriple = new Regex(@"([\p{L}\d\s]) *, *, *,([\p{L}\d\s])", RegexOptions.Compiled);
        private static readonly Regex CommaTripleEndOfLine = new Regex(@"([\p{L}\d\s]), *, *,$", RegexOptions.Compiled);
        private static readonly Regex CommaWhiteSpaceBetween = new Regex(@"([\p{L}\d\s]),\s+,([\p{L}\d\s])", RegexOptions.Compiled);
        private static readonly Regex CommaFollowedByLetter = new Regex(@",(\p{L})", RegexOptions.Compiled);

        private static readonly Regex CommaDoubleAr = new Regex(@"([\p{L}\d\s]) *،،([\p{L}\d\s])", RegexOptions.Compiled);
        private static readonly Regex CommaTripleAr = new Regex(@"([\p{L}\d\s]) *، *، *،([\p{L}\d\s])", RegexOptions.Compiled);
        private static readonly Regex CommaTripleEndOfLineAr = new Regex(@"([\p{L}\d\s]) *، *، *،$", RegexOptions.Compiled);
        private static readonly Regex CommaWhiteSpaceBetweenAr = new Regex(@"([\p{L}\d\s]) *،\s+،([\p{L}\d\s])", RegexOptions.Compiled);
        private static readonly Regex CommaFollowedByLetterAr = new Regex(@"،(\p{L})", RegexOptions.Compiled);

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {

            var fixAction = Language.FixCommas;
            var fixCount = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var s = p.Text;
                    var oldText = s;

                    if (p.Text.IndexOf(',') >= 0)
                    {
                        s = CommaDouble.Replace(s, "$1,$2");
                        s = CommaTriple.Replace(s, "$1...$2");
                        s = CommaTripleEndOfLine.Replace(s, "$1...");
                        s = CommaWhiteSpaceBetween.Replace(s, "$1,$2");

                        var match = CommaFollowedByLetter.Match(s);
                        if (match.Success && (!(match.Index > 0 && s[match.Index-1] == 'ό' && s.Substring(match.Index).StartsWith(",τι", StringComparison.OrdinalIgnoreCase)) || callbacks.Language != "el"))
                        {
                            s = CommaFollowedByLetter.Replace(s, ", $1");
                        }

                        s = RemoveCommaBeforeSentenceEndingChar(s, ',');
                    }

                    if (p.Text.IndexOf('،') >= 0)
                    {
                        s = CommaDoubleAr.Replace(s, "$1،$2");
                        s = CommaTripleAr.Replace(s, "$1...$2");
                        s = CommaTripleEndOfLineAr.Replace(s, "$1...");
                        s = CommaWhiteSpaceBetweenAr.Replace(s, "$1،$2");
                        s = CommaFollowedByLetterAr.Replace(s, "، $1");

                        s = RemoveCommaBeforeSentenceEndingChar(s, '،');
                    }

                    if (oldText != s)
                    {
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, s);
                        p.Text = s;
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, Language.FixCommas);
        }

        private static string RemoveCommaBeforeSentenceEndingChar(string input, char comma)
        {
            var s = input;
            for (var i = s.Length - 1; i >= 0; i--)
            {
                var ch = s[i];
                if (i - 1 >= 0 && s[i - 1] == comma && IsSentenceEndingChar(ch))
                {
                    var k = i;

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

        private static bool IsSentenceEndingChar(char ch) => ch == '.' || ch == '!' || ch == '?' || ch == ')' || ch == ']' || ch == '؟';
    }
}
