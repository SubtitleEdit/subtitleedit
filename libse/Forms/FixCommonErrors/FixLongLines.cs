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
                var lines = p.Text.SplitToLines();
                bool tooLong = false;
                foreach (string line in lines)
                {
                    if (HtmlUtil.RemoveHtmlTags(line, true).Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        tooLong = true;
                        break;
                    }
                }
                if (callbacks.AllowFix(p, fixAction) && tooLong)
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
    }
}
