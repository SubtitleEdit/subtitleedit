using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixHyphensInDialog : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = string.Format(language.FixHyphensInDialogs, Configuration.Settings.General.DialogStyle);
            int iFixes = 0;
            var dialogHelper = new DialogSplitMerge { DialogStyle = Configuration.Settings.General.DialogStyle };
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    string oldText = p.Text;
                    string text = dialogHelper.FixDashesAndSpaces(p.Text);
                    if (text != oldText)
                    {
                        p.Text = text;
                        iFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(iFixes, fixAction, language.XHyphensInDialogsFixed);
        }
    }
}
