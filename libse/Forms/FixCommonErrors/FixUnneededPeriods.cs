using System;

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

        public static string RemoveDotAfterPunctuation(string inp)
        {
            for (int i = inp.Length - 1; i > 0; i--)
            {
                // Expecting pre characters: [?!]
                if (inp[i] == '.' && (inp[i - 1] == '?' || inp[i - 1] == '!'))
                {
                    int j = i;
                    // Fix recursive dot after ?/!
                    while (j + 1 < inp.Length && inp[j + 1] == '.')
                    {
                        j++;
                    }
                    // Expecting post characters: [\r\n ]
                    if (j + 1 == inp.Length || inp[j + 1] == ' ' || inp[j + 1] == '\r' || inp[j + 1] == '\n')
                    {
                        inp = inp.Remove(i, j - i + 1);
                    }
                }
            }
            return inp;
        }

    }
}
