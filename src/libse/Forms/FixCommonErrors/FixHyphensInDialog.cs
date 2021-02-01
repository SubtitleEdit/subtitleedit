using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixHyphensInDialog : IFixCommonError
    {
        public static class Language
        {
            public static string FixHyphensInDialogs { get; set; } = "Fix dash in dialogs via style: {0}";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = string.Format(Language.FixHyphensInDialogs, Configuration.Settings.General.DialogStyle);
            int iFixes = 0;
            var dialogHelper = new DialogSplitMerge
            {
                DialogStyle = Configuration.Settings.General.DialogStyle,
                TwoLetterLanguageCode = callbacks.Language,
                ContinuationStyle = Configuration.Settings.General.ContinuationStyle
            };
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    string oldText = p.Text;
                    var prev = subtitle.GetParagraphOrDefault(i - 1);
                    string text = dialogHelper.FixDashesAndSpaces(p.Text, p, prev);
                    if (text != oldText)
                    {
                        p.Text = text;
                        iFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(iFixes, fixAction);
        }
    }
}
