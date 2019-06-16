using System;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class Fix3PlusLines : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.Fix3PlusLine;
            int fixCount = 0;

            const int MaxLine = 2;
            int maxLinesBak = Configuration.Settings.General.MaxNumberOfLines;
            Configuration.Settings.General.MaxNumberOfLines = MaxLine;

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction) && Utilities.GetNumberOfLines(p.Text) > MaxLine)
                {
                    string oldText = p.Text;
                    p.Text = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                    if (!oldText.Equals(p.Text, StringComparison.Ordinal))
                    {
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    else
                    {
                        callbacks.AddToTotalErrors(1); // unable to fix
                    }
                }
            }

            Configuration.Settings.General.MaxNumberOfLines = maxLinesBak;
            callbacks.UpdateFixStatus(fixCount, language.Fix3PlusLines, language.X3PlusLinesFixed);
        }
    }
}
