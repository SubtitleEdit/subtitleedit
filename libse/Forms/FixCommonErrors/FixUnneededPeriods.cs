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
                var oldText = p.Text;
                if (callbacks.AllowFix(p, fixAction))
                {
                    if (p.Text.Contains("!." + Environment.NewLine))
                    {
                        p.Text = p.Text.Replace("!." + Environment.NewLine, "!" + Environment.NewLine);
                        unneededPeriodsFixed++;
                    }
                    if (p.Text.Contains("?." + Environment.NewLine))
                    {
                        p.Text = p.Text.Replace("?." + Environment.NewLine, "?" + Environment.NewLine);
                        unneededPeriodsFixed++;
                    }
                    if (p.Text.EndsWith("!.", StringComparison.Ordinal))
                    {
                        p.Text = p.Text.TrimEnd('.');
                        unneededPeriodsFixed++;
                    }
                    if (p.Text.EndsWith("?.", StringComparison.Ordinal))
                    {
                        p.Text = p.Text.TrimEnd('.');
                        unneededPeriodsFixed++;
                    }
                    if (p.Text.Contains("!. "))
                    {
                        p.Text = p.Text.Replace("!. ", "! ");
                        unneededPeriodsFixed++;
                    }
                    if (p.Text.Contains("?. "))
                    {
                        p.Text = p.Text.Replace("?. ", "? ");
                        unneededPeriodsFixed++;
                    }

                    if (p.Text != oldText)
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            if (unneededPeriodsFixed > 0)
            {
                callbacks.AddToTotalFixes(unneededPeriodsFixed);
                callbacks.LogStatus(language.RemoveUnneededPeriods, string.Format(language.XUnneededPeriodsRemoved, unneededPeriodsFixed));
            }
        }
    }
}
