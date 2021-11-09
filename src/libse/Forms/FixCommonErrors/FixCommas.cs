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

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var commaDouble = new Regex(@"([\p{L}\d\s]),,([\p{L}\d\s])");
            var commaTriple = new Regex(@"([\p{L}\d\s]) *, *, *,([\p{L}\d\s])");
            var commaTripleEndOfLine = new Regex(@"([\p{L}\d\s]), *, *,$");
            var commaWhiteSpaceBetween = new Regex(@"([\p{L}\d\s]),\s+,([\p{L}\d\s])");
            var commaFollowedByLetter = new Regex(@",(\p{L})");

            string fixAction = Language.FixCommas;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var s = p.Text;
                    var oldText = s;

                    if (p.Text.IndexOf(',') >= 0)
                    {
                        s = commaDouble.Replace(s, "$1,$2");
                        s = commaTriple.Replace(s, "$1...$2");
                        s = commaTripleEndOfLine.Replace(s, "$1...");
                        s = commaWhiteSpaceBetween.Replace(s, "$1,$2");
                        s = commaFollowedByLetter.Replace(s, ", $1");

                        s = RemoveCommaBeforeSentenceEndingChar(s, ',');
                    }

                    if (p.Text.IndexOf('،') >= 0)
                    {
                        var commaDoubleAr = new Regex(@"([\p{L}\d\s]) *،،([\p{L}\d\s])");
                        var commaTripleAr = new Regex(@"([\p{L}\d\s]) *، *، *،([\p{L}\d\s])");
                        var commaTripleEndOfLineAr = new Regex(@"([\p{L}\d\s]) *، *، *،$");
                        var commaWhiteSpaceBetweenAr = new Regex(@"([\p{L}\d\s]) *،\s+،([\p{L}\d\s])");
                        var commaFollowedByLetterAr = new Regex(@"،(\p{L})");
                        s = commaDoubleAr.Replace(s, "$1،$2");
                        s = commaTripleAr.Replace(s, "$1...$2");
                        s = commaTripleEndOfLineAr.Replace(s, "$1...");
                        s = commaWhiteSpaceBetweenAr.Replace(s, "$1،$2");
                        s = commaFollowedByLetterAr.Replace(s, "، $1");

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
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char ch = s[i];
                if (i - 1 >= 0 && s[i - 1] == comma && IsSentenceEndingChar(ch))
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

        private static bool IsSentenceEndingChar(char ch) => ch == '.' || ch == '!' || ch == '?' || ch == ')' || ch == ']' || ch == '؟';
    }
}
