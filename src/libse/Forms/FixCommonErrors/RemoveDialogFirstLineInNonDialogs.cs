using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class RemoveDialogFirstLineInNonDialogs : IFixCommonError
    {
        public static class Language
        {
            public static string RemoveDialogFirstInNonDialogs { get; set; } = "Remove start dash in non dialogs";
        }

        /// <summary>
        /// Dash characters that can be used as dialog marker - see issue #12748.
        /// </summary>
        private static readonly char[] DashChars =
        {
            '-', // Hyphen-Minus (-, U+002D)
            '‐', // Hyphen (‐, U+2010)
            '‑', // Non-breaking hyphen (‑, U+2011)
            '‒', // Figure dash (‒, U+2012)
            '–', // En dash (–, U+2013)
            '—', // Em dash (—, U+2014)
            '―', // Horizontal bar (―, U+2015)
            '−', // Minus sign (−, U+2212)
        };

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.RemoveDialogFirstInNonDialogs;
            var noOfFixes = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                var oldText = p.Text;
                var noHtml = HtmlUtil.RemoveHtmlTags(oldText, true).TrimStart();
                if (noHtml.Length == 0 || !IsDash(noHtml[0]))
                {
                    continue;
                }

                var text = RemoveDash(oldText, noHtml, noHtml[0]);
                if (oldText != text && callbacks.AllowFix(p, fixAction))
                {
                    p.Text = text;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }

            callbacks.UpdateFixStatus(noOfFixes, Language.RemoveDialogFirstInNonDialogs);
        }

        private static string RemoveDash(string text, string noHtml, char dashChar)
        {
            if (CountDashes(noHtml) == 1)
            {
                return RemoveFirstDash(text, dashChar);
            }

            // more than one dash - only remove the first one if the others cannot be a dialog marker
            if (StartsSentenceWithDash(noHtml))
            {
                return text;
            }

            var lines = noHtml.SplitToLines();
            if (lines.Count > 3)
            {
                return text;
            }

            for (var i = 1; i < lines.Count; i++)
            {
                var line = lines[i].TrimStart();
                if (line.Length > 0 && IsDash(line[0]))
                {
                    return text;
                }
            }

            return RemoveFirstDash(text, dashChar);
        }

        private static string RemoveFirstDash(string text, char dashChar)
        {
            var idx = IndexOfDashOutsideTag(text, dashChar);
            if (idx < 0)
            {
                return text;
            }

            if (idx + 1 < text.Length && text[idx + 1] == ' ')
            {
                text = text.Remove(idx + 1, 1);
            }

            text = text.Remove(idx, 1);

            if (idx > 0 && text[idx - 1] == ' ')
            {
                text = text.Remove(idx - 1, 1);
            }

            return text;
        }

        /// <summary>
        /// Index of the first dash which is not inside an html/assa tag.
        /// </summary>
        private static int IndexOfDashOutsideTag(string text, char dashChar)
        {
            var insideTag = false;
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '<' || ch == '{')
                {
                    insideTag = true;
                }
                else if (ch == '>' || ch == '}')
                {
                    insideTag = false;
                }
                else if (!insideTag && ch == dashChar)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// True if a dash follows the end of a sentence, like "Hi there. - Hello!".
        /// </summary>
        private static bool StartsSentenceWithDash(string text)
        {
            for (var i = 2; i < text.Length; i++)
            {
                if (text[i - 1] == ' ' && IsDash(text[i]) &&
                    (text[i - 2] == '.' || text[i - 2] == '!' || text[i - 2] == '?'))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CountDashes(string text)
        {
            var count = 0;
            foreach (var ch in text)
            {
                if (IsDash(ch))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsDash(char ch)
        {
            foreach (var dashChar in DashChars)
            {
                if (ch == dashChar)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
