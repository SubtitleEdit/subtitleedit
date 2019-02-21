using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class RemoveSpaceBetweenNumbers : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            var fixAction = language.RemoveSpaceBetweenNumber;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var text = Utilities.RemoveSpaceBetweenNumbers(p.Text);
                    if (text != p.Text)
                    {
                        var oldText = p.Text;
                        p.Text = text;
                        noOfFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, language.FixCommonOcrErrors, string.Format(language.RemoveSpaceBetweenNumbersFixed, noOfFixes));
        }

    }
}
