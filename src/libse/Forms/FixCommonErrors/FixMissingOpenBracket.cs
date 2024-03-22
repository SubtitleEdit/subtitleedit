using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixMissingOpenBracket : IFixCommonError
    {
        public static class Language
        {
            public static string FixMissingOpenBracket { get; set; } = "Fix missing [ or ( in line";
        }

        private static bool IsIgnorable(char ch) => char.IsWhiteSpace(ch) || ch == '-';

        private static char GetOpeningPair(char tag) => tag == ')' ? '(' : '[';

        private static int CalcInsertPositionFromBeginning(string input)
        {
            var len = input.Length;
            var i = 0;

            // skip asa tag
            while (i < len && input[i] == '{')
            {
                i = input.IndexOf('}', i + 1) + 1;
                if (i == 0)
                {
                    break;
                }
            }

            while (i < len && IsIgnorable(input[i]))
            {
                i++;
            }

            // skip html tags
            while (i < len && input[i] == '<')
            {
                i = input.IndexOf('>', i + 1) + 1;
                if (i == 0)
                {
                    break;
                }
            }

            // skip anything that is not a letter or digit
            while (i < len && !char.IsLetterOrDigit(input[i]))
            {
                i++;
            }

            return i;
        }

        private static string RestoreMissingOpenParenthesis(string input)
        {
            var len = input.Length;

            // empty string
            if (len == 0)
            {
                return input;
            }

            var closeTags = new[] { ']', ')' };
            // ignore line if contains opening
            if (input.Any(ch => ch == '(' || ch == '['))
            {
                return input;
            }

            var ci = input.IndexOfAny(closeTags);
            // invalid position
            if (ci < 1)
            {
                return input;
            }

            var k = ci - 1;
            // jump backward if uppercase or any one of the ignorable chars 
            while (k > 0 && (char.IsUpper(input[k]) || IsIgnorable(input[k])))
            {
                k--;
            }

            // note if we have case like: "Hey, FOO)." then we want to insert the open before the "Hey"
            if (k > 0 && input[k] == ',')
            {
                k--;
                while (k > 0 && char.IsLetterOrDigit(input[k]))
                {
                    k--;
                }
            }

            // try landing on white-space char
            if (k >= 0 && k + 1 < len && input[k] != ' ' && input[k + 1] == ' ')
            {
                k++;
            }

            // try finding first valid char (this is used to not insert '(' or '[') in to left of a white-space
            while (k < ci && char.IsWhiteSpace(input[k]))
            {
                k++;
            }

            // FO) => (FO)
            if (ci - k > 1)
            {
                input = input.Insert(k, GetOpeningPair(input[ci]).ToString());
            }
            else
            {
                // recalculate value of k from beginning
                k = CalcInsertPositionFromBeginning(input);
                if (k < ci)
                {
                    input = input.Insert(k, GetOpeningPair(input[ci]).ToString());
                }
            }

            return input;
        }

        private static bool IsPerLineRestoration(string[] lines) => lines.Length == 1 || lines.All(l => l.HasSentenceEnding());

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.FixMissingOpenBracket;
            var fixCount = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];

                if (callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    var text = p.Text;

                    // split only if both lines are closed
                    var lines = p.Text.SplitToLines().ToArray();
                    // logic to perform for when line is/are closed.
                    if (IsPerLineRestoration(lines))
                    {
                        var count = lines.Length;
                        for (var j = 0; j < count; j++)
                        {
                            lines[j] = RestoreMissingOpenParenthesis(lines[j]);
                        }

                        // rebuild the text
                        text = count > 1 ? string.Join(Environment.NewLine, lines) : lines[0];
                    }
                    else
                    {
                        // handles 2+ lines even if the their adjacent is not closed 
                        text = RestoreMissingOpenParenthesis(text);
                    }

                    if (oldText.Length != text.Length)
                    {
                        fixCount++;
                        p.Text = text;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }

            callbacks.UpdateFixStatus(fixCount, Language.FixMissingOpenBracket);
        }
    }
}