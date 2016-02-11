using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Logic.Ocr;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixOcrErrorsViaReplaceList : IFixCommonError
    {
        public string ThreeLettersISOLanguageName { get; set; }
        public Form ParentForm { get; set; }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            using (var ocrFixEngine = new OcrFixEngine(ThreeLettersISOLanguageName, null, ParentForm))
            {
                var language = Configuration.Settings.Language.FixCommonErrors;
                var fixAction = language.FixCommonOcrErrors;
                int noOfFixes = 0;
                string lastLine = string.Empty;
                for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    var p = subtitle.Paragraphs[i];
                    string text = ocrFixEngine.FixOcrErrors(p.Text, i, lastLine, false, OcrFixEngine.AutoGuessLevel.Cautious);
                    lastLine = text;
                    if (callbacks.AllowFix(p, fixAction) && p.Text != text)
                    {
                        string oldText = p.Text;
                        p.Text = text;
                        noOfFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    Application.DoEvents();
                }
                callbacks.UpdateFixStatus(noOfFixes, language.CommonOcrErrorsFixed, noOfFixes.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
    }
}
