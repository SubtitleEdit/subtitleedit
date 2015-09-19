using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class RemoveSpaceBetweenNumbers : IFixCommonError
    {

        private static readonly Regex RemoveSpaceBetweenNumbersRegEx = new Regex(@"\d \d", RegexOptions.Compiled);

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.RemoveSpaceBetweenNumber;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = p.Text;
                Match match = RemoveSpaceBetweenNumbersRegEx.Match(text);
                int counter = 0;
                while (match.Success && counter < 100 && text.Length > match.Index + 1)
                {
                    string temp = text.Substring(match.Index + 2);
                    if (temp != "1/2" &&
                        !temp.StartsWith("1/2 ", StringComparison.Ordinal) &&
                        !temp.StartsWith("1/2.", StringComparison.Ordinal) &&
                        !temp.StartsWith("1/2!", StringComparison.Ordinal) &&
                        !temp.StartsWith("1/2?", StringComparison.Ordinal) &&
                        !temp.StartsWith("1/2<", StringComparison.Ordinal))
                    {
                        text = text.Remove(match.Index + 1, 1);
                    }
                    if (text.Length > match.Index + 1)
                        match = RemoveSpaceBetweenNumbersRegEx.Match(text, match.Index + 2);
                    counter++;
                }
                if (callbacks.AllowFix(p, fixAction) && p.Text != text)
                {
                    string oldText = p.Text;
                    p.Text = text;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, language.FixCommonOcrErrors, string.Format(language.RemoveSpaceBetweenNumbersFixed, noOfFixes));
        }

    }
}
