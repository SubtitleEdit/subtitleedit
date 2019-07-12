using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixLongLines : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.BreakLongLine;
            int noOfLongLines = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction) && ShouldBalance(p.Text))
                {
                    string oldText = p.Text;
                    p.Text = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                    if (oldText != p.Text)
                    {
                        noOfLongLines++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    else
                    {
                        callbacks.LogStatus(fixAction, string.Format(language.UnableToFixTextXY, i + 1, p));
                        callbacks.AddToTotalErrors(1);
                    }
                }
            }

            callbacks.UpdateFixStatus(noOfLongLines, language.BreakLongLines, string.Format(language.XLineBreaksAdded, noOfLongLines));
        }

        private static bool ShouldBalance(string input)
        {
            string noTagsText = HtmlUtil.RemoveHtmlTags(input, true);

            // ignore newline chars
            int totalNewLineChars = Utilities.CountTagInText(noTagsText, System.Environment.NewLine);
            int nlLen = totalNewLineChars * System.Environment.NewLine.Length;
            if (noTagsText.Length - nlLen > Configuration.Settings.General.SubtitleLineMaximumLength * totalNewLineChars)
            {
                return true;
            }

            foreach (string noTagsLine in noTagsText.SplitToLines())
            {
                if (noTagsLine.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
