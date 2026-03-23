using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixDoubleApostrophes : IFixCommonError
    {
        public static class Language
        {
            public static string FixDoubleApostrophes { get; set; } = "Fix double apostrophe characters ('') to a single quote (\")";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.FixDoubleApostrophes;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];

                if (p.Text.Contains("''"))
                {
                    if (callbacks.AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Replace("''", "\"");
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                        fixCount++;
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, Language.FixDoubleApostrophes);
        }
    }
}
