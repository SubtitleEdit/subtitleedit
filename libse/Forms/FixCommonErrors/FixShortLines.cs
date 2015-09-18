using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortLines : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.MergeShortLine;
            int noOfShortLines = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string oldText = p.Text;
                var text = Helper.FixShortLines(p.Text);
                if (callbacks.AllowFix(p, fixAction) && oldText != text)
                {
                    p.Text = text;
                    noOfShortLines++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            if (noOfShortLines > 0)
            {
                callbacks.AddToTotalFixes(noOfShortLines);
                callbacks.LogStatus(language.RemoveLineBreaks, string.Format(language.XLinesUnbreaked, noOfShortLines));
            }
        }
    }
}
