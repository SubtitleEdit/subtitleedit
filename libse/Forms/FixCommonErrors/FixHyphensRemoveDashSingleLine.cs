using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixHyphensRemoveDashSingleLine : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.RemoveHyphensSingleLine;
            int iFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (Helper.IsOneSentence(p.Text) && callbacks.AllowFix(p, fixAction))
                {
                    string oldText = p.Text;
                    string text = Helper.FixHyphensRemoveForSingleLine(subtitle, p.Text, i);
                    if (text != oldText)
                    {
                        p.Text = text;
                        iFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(iFixes, language.RemoveHyphensSingleLine, language.XHyphensSingleLineRemoved);
        }
    }
}
