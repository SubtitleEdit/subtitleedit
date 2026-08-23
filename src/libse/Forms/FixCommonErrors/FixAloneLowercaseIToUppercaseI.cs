using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixAloneLowercaseIToUppercaseI : IFixCommonError
    {
        public static class Language
        {
            public static string FixLowercaseIToUppercaseI { get; set; } = "Fix alone lowercase 'i' to 'I' (English)";
        }

        // Every needle below embeds Environment.NewLine or the target character, so none of them
        // can be a compile-time constant: written inline they were ten fresh strings allocated
        // for every line of the file, just to be handed to Replace. Prepared once instead.
        private static readonly string UppercaseINewLine = ">I" + Environment.NewLine;
        private static readonly string NewLineLowerImSpace = Environment.NewLine + "i'm ";
        private static readonly string NewLineUpperImSpace = Environment.NewLine + "I'm ";
        private static readonly string NewLineLowerImPeriod = Environment.NewLine + "i'm.";
        private static readonly string NewLineUpperImPeriod = Environment.NewLine + "I'm.";

        /// <summary>Characters that stop the "i-l" -&gt; "I-l" fix; see the match loop below.</summary>
        private static readonly string LittleIStopChars = Environment.NewLine + @" <>!.?:;,";

        /// <summary>
        /// The four html-tag needles for one target character, prepared once. A single immutable
        /// instance swapped by reference keeps this thread-safe without locking; every caller in
        /// the app passes 'i', so the memo hits on all but the first call.
        /// </summary>
        private sealed class TargetNeedles
        {
            public readonly char Target;
            public readonly string CloseTag;
            public readonly string Space;
            public readonly string ZeroWidthSpace;
            public readonly string ZeroWidthNoBreakSpace;

            public TargetNeedles(char target)
            {
                Target = target;
                CloseTag = ">" + target + "</";
                Space = ">" + target + " ";
                ZeroWidthSpace = ">" + target + "\u200B" + Environment.NewLine;
                ZeroWidthNoBreakSpace = ">" + target + "\uFEFF" + Environment.NewLine;
            }
        }

        private static TargetNeedles _needles = new TargetNeedles('i');

        private static TargetNeedles GetNeedles(char target)
        {
            var needles = _needles;
            if (needles.Target != target)
            {
                needles = new TargetNeedles(target);
                _needles = needles;
            }

            return needles;
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.FixLowercaseIToUppercaseI;
            int iFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string oldText = p.Text;
                string s = p.Text;
                if (s.Contains('i'))
                {
                    s = FixAloneLowercaseIToUppercaseLine(RegexUtils.LittleIRegex, oldText, s, 'i');
                    if (s != oldText && callbacks.AllowFix(p, fixAction))
                    {
                        p.Text = s;
                        iFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(iFixes, Language.FixLowercaseIToUppercaseI);
        }

        public static string FixAloneLowercaseIToUppercaseLine(Regex re, string oldText, string input, char target)
        {
            //html tags
            var needles = GetNeedles(target);
            var s = input.Replace(needles.CloseTag, ">I</")
                         .Replace(needles.Space, ">I ")
                         .Replace(needles.ZeroWidthSpace, UppercaseINewLine) // Zero Width Space
                         .Replace(needles.ZeroWidthNoBreakSpace, UppercaseINewLine); // Zero Width No-Break Space

            s = s.Replace(" i-i ", " I-I ");
            s = s.Replace(" i-i-i ", " I-I-I ");
            s = s.Replace(" i'm ", " I'm ");
            s = s.Replace(NewLineLowerImSpace, NewLineUpperImSpace);
            s = s.Replace(NewLineLowerImPeriod, NewLineUpperImPeriod);
            s = s.Replace(" i'm.", " I'm.");
            s = s.Replace(" i'm,", " I'm,");
            s = s.Replace("-i'm-", "-I'm-");
            s = s.Replace("-i'm ", "-I'm ");
            s = s.Replace("-i'm.", "-I'm.");

            // reg-ex
            var match = re.Match(s);
            var assaDrawStart = s.IndexOf("{\\p1", StringComparison.Ordinal);
            var assaDrawEnd = s.IndexOf("{\\p0}", StringComparison.Ordinal);
            while (match.Success)
            {
                if (s[match.Index] == target && !s.Substring(match.Index).StartsWith("i.e.", StringComparison.Ordinal) &&
                    !s.Substring(match.Index).StartsWith("i-", StringComparison.Ordinal) &&
                    !(assaDrawStart >= 0 && match.Index > assaDrawStart && match.Index < assaDrawEnd))
                {
                    var prev = '\0';
                    var next = '\0';
                    if (match.Index > 0)
                    {
                        prev = s[match.Index - 1];
                    }

                    if (match.Index + 1 < s.Length)
                    {
                        next = s[match.Index + 1];
                    }

                    var wholePrev = string.Empty;
                    if (match.Index > 1)
                    {
                        wholePrev = s.Substring(0, match.Index - 1);
                    }

                    if (prev != '>' && next != '>' && next != '}' && !wholePrev.TrimEnd().EndsWith("...", StringComparison.Ordinal))
                    {
                        var fix = prev != '.' && prev != '\'';

                        if (prev == ' ' && next == '.')
                        {
                            fix = false;
                        }

                        if (prev == '-' && match.Index > 2)
                        {
                            fix = false;
                        }

                        if (fix && next == '-' && match.Index < s.Length - 5 && s[match.Index + 2] == 'l' && !LittleIStopChars.Contains(s[match.Index + 3]))
                        {
                            fix = false;
                        }

                        if (fix)
                        {
                            string temp = s.Substring(0, match.Index) + "I";
                            if (match.Index + 1 < oldText.Length)
                            {
                                temp += s.Substring(match.Index + 1);
                            }

                            s = temp;
                        }
                    }
                }
                match = match.NextMatch();
            }

            return s;
        }
    }
}
