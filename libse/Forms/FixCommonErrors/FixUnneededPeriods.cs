using System;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixUnneededPeriods : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.UnneededPeriod;
            int removedCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    // Returns processed text.
                    string procText = RemoveDotAfterPunctuation(p.Text);
                    int diff = p.Text.Length - procText.Length;
                    if (diff > 0)
                    {
                        // Calculate total removed dots.
                        removedCount += diff;
                        callbacks.AddFixToListView(p, fixAction, p.Text, procText);
                        p.Text = procText;
                    }
                }
            }
            callbacks.UpdateFixStatus(removedCount, language.RemoveUnneededPeriods, string.Format(language.XUnneededPeriodsRemoved, removedCount));
        }

        public static string RemoveDotAfterPunctuation(string input)
        {
            for (int i = input.Length - 1; i > 0; i--)
            {
                // Expecting pre characters: [?!]
                if (input[i] == '.' && (input[i - 1] == '?' || input[i - 1] == '!'))
                {
                    int j = i;
                    // Fix recursive dot after ?/!
                    while (j + 1 < input.Length && input[j + 1] == '.')
                    {
                        j++;
                    }
                    // Expecting post characters: [\r\n ]
                    if (j + 1 == input.Length || input[j + 1] == ' ' || input[j + 1] == '\r' || input[j + 1] == '\n')
                    {
                        input = input.Remove(i, j - i + 1);
                    }
                }
            }
            return input;
        }

    }
}
