using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly Lazy<Dictionary<Regex, string>> LazyCommaRegex = new Lazy<Dictionary<Regex, string>>(() => new Dictionary<Regex, string>
        {
            { new Regex(@"([\p{L}\d\s]) *،،([\p{L}\d\s])", RegexOptions.Compiled), "$1،$2" },
            { new Regex(@"([\p{L}\d\s]) *، *، *،([\p{L}\d\s])", RegexOptions.Compiled), "$1...$2" },
            { new Regex(@"([\p{L}\d\s]) *، *، *،$", RegexOptions.Compiled), "$1..." },
            { new Regex(@"([\p{L}\d\s]) *،\s+،([\p{L}\d\s])", RegexOptions.Compiled), "$1،$2" },
            { new Regex(@"،(\p{L})", RegexOptions.Compiled), "، $1" },
        });

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var commaDouble = new Regex(@"([\p{L}\d\s]),,([\p{L}\d\s])");
            var commaTriple = new Regex(@"([\p{L}\d\s]) *, *, *,([\p{L}\d\s])");
            var commaTripleEndOfLine = new Regex(@"([\p{L}\d\s]), *, *,$");
            var commaWhiteSpaceBetween = new Regex(@"([\p{L}\d\s]),\s+,([\p{L}\d\s])");
            var commaFollowedByLetter = new Regex(@",(\p{L})");

            var fixAction = Language.FixCommas;
            var fixCount = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var s = p.Text;
                    var oldText = s;

                    if (p.Text.Contains(','))
                    {
                        s = commaDouble.Replace(s, "$1,$2");
                        s = commaTriple.Replace(s, "$1...$2");
                        s = commaTripleEndOfLine.Replace(s, "$1...");
                        s = commaWhiteSpaceBetween.Replace(s, "$1,$2");

                        var match = commaFollowedByLetter.Match(s);
                        if (match.Success && (!(match.Index > 0 && s[match.Index - 1] == 'ό' && s.Substring(match.Index).StartsWith(",τι", StringComparison.OrdinalIgnoreCase)) || callbacks.Language != "el"))
                        {
                            s = commaFollowedByLetter.Replace(s, ", $1");
                        }

                        s = RemoveCommaBeforeSentenceEndingChar(s, ',');
                    }

                    if (p.Text.Contains('،'))
                    {
                        foreach (var keyValue in LazyCommaRegex.Value)
                        {
                            s = keyValue.Key.Replace(s, keyValue.Value);
                        }

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
            // foo,,,,! bar => foo! bar
            var len = input.Length;
            var chars = input.ToCharArray();
            int pos = 0;
            for (int i = 0; i < len; i++)
            {
                var ch = input[i];
                if (ch == comma && IsNextCharSentenceClosingSymbol(i, chars))
                {
                    continue;
                }

                chars[pos++] = ch;
            }

            return new string(chars, 0, pos);

            bool IsNextCharSentenceClosingSymbol(int index, char[] characters)
            {
                return index + 1 < characters.Length && StringExtensions.NeutralSentenceEndingChars.Contains(characters[index + 1]);
            }
        }
    }
}
