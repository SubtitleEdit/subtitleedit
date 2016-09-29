using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixUnneededPeriods : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.UnneededPeriod;
            int unneededPeriodsFixed = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    var text = p.Text;

                    if (text.Contains(Environment.NewLine))
                    {
                        text = text.Replace("!." + Environment.NewLine, "!" + Environment.NewLine);
                        text = text.Replace("?." + Environment.NewLine, "?" + Environment.NewLine);
                    }
                    text = FixInvalidDotsAfterQuestionOrExclamationMarks(text);
                    if (text.Contains(". "))
                    {
                        text = text.Replace("!. ", "! ");
                        text = text.Replace("?. ", "? ");
                    }

                    if (!text.Equals(oldText, StringComparison.Ordinal))
                    {
                        unneededPeriodsFixed += oldText.Length - text.Length;
                        p.Text = text;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(unneededPeriodsFixed, language.RemoveUnneededPeriods, string.Format(language.XUnneededPeriodsRemoved, unneededPeriodsFixed));
        }

        private static string FixInvalidDotsAfterQuestionOrExclamationMarks(string text)
        {
            if (!text.EndsWith('.'))
            {
                return text;
            }
            bool trimDots = false;
            int j = text.Length - 1;
            for (; j >= 0; j--)
            {
                char ch = text[j];
                if (ch == '?' || ch == '!')
                {
                    trimDots = true;
                    break;
                }
                // Exit without changing anything.
                if (ch != '.')
                {
                    break;
                }
            }
            if (trimDots)
            {
                text = text.Substring(0, j + 1);
            }
            return text;
        }
    }
}
