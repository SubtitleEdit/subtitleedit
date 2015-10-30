using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixStartWithUppercaseLetterAfterPeriodInsideParagraph : IFixCommonError
    {
        private readonly static char[] ExpectedChars = { '.', '!', '?' };

        private bool IsAbbreviation(string text, int index, IFixCallbacks callbacks)
        {
            if (text[index] != '.')
                return false;

            if (index - 3 > 0 && Utilities.AllLettersAndNumbers.Contains(text[index - 1]) && text[index - 2] == '.') // e.g: O.R.
                return true;

            var word = string.Empty;
            int i = index - 1;
            while (i >= 0 && Utilities.AllLetters.Contains(text[i]))
            {
                word = text[i] + word;
                i--;
            }

            return callbacks.GetAbbreviations().Contains(word + ".");
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.StartWithUppercaseLetterAfterPeriodInsideParagraph;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string oldText = p.Text;
                var st = new StripableText(p.Text);
                if (p.Text.Length > 3)
                {
                    string text = st.StrippedText.Replace("  ", " ");
                    int start = text.IndexOfAny(ExpectedChars);
                    while (start >= 0 && start < text.Length)
                    {
                        if (start > 0 && char.IsDigit(text[start - 1]))
                        {
                            // ignore periods after a number
                        }
                        else if (start + 4 < text.Length && text[start + 1] == ' ')
                        {
                            if (!IsAbbreviation(text, start, callbacks))
                            {
                                var subText = new StripableText(text.Substring(start + 2));
                                if (subText.StrippedText.Length > 0 && Helper.IsTurkishLittleI(subText.StrippedText[0], callbacks.Encoding, callbacks.Language))
                                {
                                    if (subText.StrippedText.Length > 1 && !(subText.Pre.Contains('\'') && subText.StrippedText.StartsWith('s')))
                                    {
                                        text = text.Substring(0, start + 2) + subText.Pre + Helper.GetTurkishUppercaseLetter(subText.StrippedText[0], callbacks.Encoding) + subText.StrippedText.Substring(1) + subText.Post;
                                        if (callbacks.AllowFix(p, fixAction))
                                        {
                                            p.Text = st.Pre + text + st.Post;
                                        }
                                    }
                                }
                                else if (subText.StrippedText.Length > 0 && Configuration.Settings.General.UppercaseLetters.Contains(char.ToUpper(subText.StrippedText[0])))
                                {
                                    if (subText.StrippedText.Length > 1 && !(subText.Pre.Contains('\'') && subText.StrippedText.StartsWith('s')))
                                    {
                                        text = text.Substring(0, start + 2) + subText.Pre + char.ToUpper(subText.StrippedText[0]) + subText.StrippedText.Substring(1) + subText.Post;
                                        if (callbacks.AllowFix(p, fixAction))
                                        {
                                            p.Text = st.Pre + text + st.Post;
                                        }
                                    }
                                }
                            }
                        }
                        start += 4;
                        if (start < text.Length)
                            start = text.IndexOfAny(ExpectedChars, start);
                    }
                }

                if (oldText != p.Text)
                {
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, language.StartWithUppercaseLetterAfterPeriodInsideParagraph, noOfFixes.ToString(CultureInfo.InvariantCulture));
        }

    }
}
