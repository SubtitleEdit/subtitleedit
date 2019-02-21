using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixTurkishAnsiToUnicode : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixTurkishAnsi;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = p.Text;
                string oldText = text;
                text = text.Replace('Ý', 'İ');
                text = text.Replace('Ð', 'Ğ');
                text = text.Replace('Þ', 'Ş');
                text = text.Replace('ý', 'ı');
                text = text.Replace('ð', 'ğ');
                text = text.Replace('þ', 'ş');
                if (oldText != text && callbacks.AllowFix(p, fixAction))
                {
                    p.Text = text;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, language.FixCommonOcrErrors, language.FixTurkishAnsi);
        }

    }
}
