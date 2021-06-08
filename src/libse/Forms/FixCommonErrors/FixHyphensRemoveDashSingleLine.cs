using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixHyphensRemoveDashSingleLine : IFixCommonError
    {
        public static class Language
        {
            public static string RemoveHyphensSingleLine { get; set; } = "Remove dialog dashes in single lines";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            int iFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (Helper.IsOneSentence(p.Text) && callbacks.AllowFix(p, Language.RemoveHyphensSingleLine))
                {
                    string oldText = p.Text;
                    string text = Helper.FixHyphensRemoveForSingleLine(subtitle, p.Text, i);
                    if (text != oldText)
                    {
                        p.Text = text;
                        iFixes++;
                        callbacks.AddFixToListView(p, Language.RemoveHyphensSingleLine, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(iFixes, Language.RemoveHyphensSingleLine);
        }
    }
}
