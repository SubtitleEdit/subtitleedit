using System.Text.RegularExpressions;
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
            var iFixes = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (Helper.IsOneSentence(p.Text) && !IsDialogInOneLine(p.Text) && callbacks.AllowFix(p, Language.RemoveHyphensSingleLine))
                {
                    var oldText = p.Text;
                    var text = Helper.FixHyphensRemoveForSingleLine(subtitle, p.Text, i);
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

        private static bool IsDialogInOneLine(string text)
        {
            if (Utilities.CountTagInText(text, '-') == 1)
            {
                return false;
            }

            var textNoSpace = text.RemoveChar(' ');
            return Regex.IsMatch(textNoSpace, "-.*[/?/.!]-[A-Z]") ||
                   Regex.IsMatch(textNoSpace, "-.*[/?/.!]<i>-[A-Z]");
        }
    }
}
