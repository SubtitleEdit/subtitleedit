using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class RemoveSpaceBetweenNumbers : IFixCommonError
    {
        public static class Language
        {
            public static string RemoveSpaceBetweenNumber { get; set; } = "Remove space between numbers";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, Language.RemoveSpaceBetweenNumber))
                {
                    var text = Utilities.RemoveSpaceBetweenNumbers(p.Text);
                    if (text != p.Text)
                    {
                        var oldText = p.Text;
                        p.Text = text;
                        noOfFixes++;
                        callbacks.AddFixToListView(p, Language.RemoveSpaceBetweenNumber, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, Language.RemoveSpaceBetweenNumber);
        }
    }
}
