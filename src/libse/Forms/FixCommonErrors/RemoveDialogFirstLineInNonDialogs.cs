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
                var count = Utilities.CountTagInText(text, '-');
                if (count == 0 || !noHtml.StartsWith('-'))
                {
                    continue;
                }

                if (count == 1)
                {
                    text = RemoveFirstDash(text);
                }
                else if (count > 1)
                {
                    var lines = noHtml.SplitToLines();
                    if (lines.Count == 1)
                    {
                        if (!noHtml.Contains(". -") && !noHtml.Contains("! -") && !noHtml.Contains("? -"))
                        {
                            text = RemoveFirstDash(text);
                        }
                    }
                    else if (lines.Count == 2)
                    {
                        if (!noHtml.Contains(". -") && !noHtml.Contains("! -") && !noHtml.Contains("? -") && !lines[1].StartsWith('-'))
                        {
                            text = RemoveFirstDash(text);
                        }
                    }
                    else if (lines.Count == 3)
                    {
                        if (!noHtml.Contains(". -") && !noHtml.Contains("! -") && !noHtml.Contains("? -") && 
                            !lines[1].StartsWith('-') && 
                            !lines[2].StartsWith('-'))
                        {
                            text = RemoveFirstDash(text);
                        }
                    }
                }

                if (oldText != text && callbacks.AllowFix(p, fixAction))
                {
                    p.Text = text;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, Language.RemoveDialogFirstInNonDialogs);
        }

        private static string RemoveFirstDash(string text)
        {
            var idx = text.IndexOf('-');
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
