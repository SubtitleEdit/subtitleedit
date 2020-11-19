using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixUnnecessaryLeadingDots : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixUnnecessaryLeadingDots;
            int fixCount = 0;

            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                var text = ContinuationUtilities.SanitizeString(p.Text);
                if ((text.StartsWith("..") || text.StartsWith("…")) && callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;

                    Paragraph pPrev = subtitle.Paragraphs[i - 1];
                    var previousText = ContinuationUtilities.SanitizeString(pPrev.Text);

                    if (previousText.EndsWith("..") || previousText.EndsWith("…"))
                    {
                        // Remove starting dots

                        // Get first word
                        string[] split = text.Split(Convert.ToChar(" "));
                        string firstWord = split[0];
                        string newFirstWord = firstWord;

                        // Remove dots
                        if (newFirstWord.StartsWith("..."))
                        {
                            newFirstWord = newFirstWord.Substring(3);
                        }
                        if (newFirstWord.StartsWith(".."))
                        {
                            newFirstWord = newFirstWord.Substring(2);
                        }
                        if (newFirstWord.StartsWith("…"))
                        {
                            newFirstWord = newFirstWord.Substring(1);
                        }
                        newFirstWord = newFirstWord.Trim();

                        var newText = ContinuationUtilities.ReplaceFirstOccurrence(oldText, firstWord, newFirstWord);

                        // Commit
                        p.Text = newText;
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, newText);
                    }
                }
            }

            callbacks.UpdateFixStatus(fixCount, language.FixUnnecessaryLeadingDots, language.XFixUnnecessaryLeadingDots);
        }
    }
}
