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

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.RemoveDialogFirstInNonDialogs;
            var noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var oldText = p.Text;
                var text = p.Text;
                var noHtml = HtmlUtil.RemoveHtmlTags(text, true).TrimStart();


                var count = Utilities.CountTagInText(text, '-') + Utilities.CountTagInText(text, '‐');
                if (count == 0 || !noHtml.StartsWith('-') && !noHtml.StartsWith('‐'))
                {
                    continue;
                }

                // test the two different dashes
                text = RemoveDash(text, noHtml, '-'); 
                text = RemoveDash(text, noHtml, '‐');

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
            var count = Utilities.CountTagInText(text, dashChar);
            if (count == 1)
            {
                text = RemoveFirstDash(text, dashChar);
            }
            else if (count > 1)
            {
                var lines = noHtml.SplitToLines();
                if (lines.Count == 1)
                {
                    if (!noHtml.Contains(". " + dashChar) && !noHtml.Contains("! " + dashChar) && !noHtml.Contains("? " + dashChar))
                    {
                        text = RemoveFirstDash(text, dashChar);
                    }
                }
                else if (lines.Count == 2)
                {
                    if (!noHtml.Contains(". " + dashChar) && !noHtml.Contains("! " + dashChar) && !noHtml.Contains("? " + dashChar) && !lines[1].StartsWith(dashChar))
                    {
                        text = RemoveFirstDash(text, dashChar);
                    }
                }
                else if (lines.Count == 3)
                {
                    if (!noHtml.Contains(". " + dashChar) && !noHtml.Contains("! " + dashChar) && !noHtml.Contains("? " + dashChar) &&
                        !lines[1].StartsWith(dashChar) &&
                        !lines[2].StartsWith(dashChar))
                    {
                        text = RemoveFirstDash(text, dashChar);
                    }
                }
            }

            return text;
        }

        private static string RemoveFirstDash(string text, char dashChar)
        {
            var idx = text.IndexOf(dashChar);
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
    }
}
