using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixStartWithUppercaseLetterAfterPeriodInsideParagraph : IFixCommonError
    {
        public static class Language
        {
            public static string StartWithUppercaseLetterAfterPeriodInsideParagraph { get; set; } = "Start with uppercase letter after period inside paragraph";
        }

        private static readonly char[] ExpectedChars = { '.', '!', '?' };

        private bool IsAbbreviation(string text, int index, IFixCallbacks callbacks)
        {
            if (text[index] != '.')
            {
                return false;
            }

            if (index - 3 > 0 && char.IsLetterOrDigit(text[index - 1]) && text[index - 2] == '.') // e.g: O.R.
            {
                return true;
            }

            var word = string.Empty;
            int i = index - 1;
            while (i >= 0 && char.IsLetter(text[i]))
            {
                word = text[i--] + word;
            }

            return callbacks.GetAbbreviations().Contains(word + ".");
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.StartWithUppercaseLetterAfterPeriodInsideParagraph;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string oldText = p.Text;
                if (p.Text.Length > 3 && callbacks.AllowFix(p, fixAction))
                {
                    var st = new StrippableText(p.Text);
                    string text = st.StrippedText;
                    int start = text.IndexOfAny(ExpectedChars);
                    while (start > 0 && start < text.Length)
                    {
                        char charAtPosition = text[start];
                        // Allow fixing lowercase letter after recursive ??? or !!!.
                        if (charAtPosition != '.') // Dot is not include 'cause I don't capitalize word after the ellipses (...), right?
                        {
                            while (start + 1 < text.Length && text[start + 1] == charAtPosition)
                            {
                                start++;
                            }
                        }

                        // Try to reach the last dot if char at *start is '.'.
                        if (charAtPosition == '.')
                        {
                            while (start + 1 < text.Length && text[start + 1] == '.')
                            {
                                start++;
                            }
                        }

                        if (start + 3 < text.Length && (text[start + 1] == ' ') && !IsAbbreviation(text, start, callbacks))
                        {
                            var textBefore = text.Substring(0, start + 1);
                            var subText = new StrippableText(text.Substring(start + 2));
                            text = text.Substring(0, start + 2) + subText.CombineWithPrePost(ToUpperFirstLetter(textBefore, subText.StrippedText, callbacks));
                        }

                        start += 3;
                        if (start < text.Length)
                        {
                            start = text.IndexOfAny(ExpectedChars, start);
                        }
                    }
                    text = st.CombineWithPrePost(text);
                    if (oldText != text)
                    {
                        p.Text = text;
                        noOfFixes++;
                        var isChecked = true;
                        if (callbacks.Language == "bg" && text.Contains("г. "))
                        {
                            // Bulgarian have "г." after years but the sentence continues with lowercase
                            var regex = new Regex(@"\d г\. \p{L}");
                            var t1 = regex.Replace(oldText, string.Empty);
                            var t2 = regex.Replace(text, string.Empty);
                            isChecked = t1 != t2;
                        }
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text, isChecked);
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, Language.StartWithUppercaseLetterAfterPeriodInsideParagraph);
        }

        private static string ToUpperFirstLetter(string textBefore, string text, IFixCallbacks callbacks)
        {
            if (string.IsNullOrEmpty(text) || !char.IsLetter(text[0]) || char.IsUpper(text[0]))
            {
                return text;
            }

            if (textBefore != null && textBefore.EndsWith("...", System.StringComparison.Ordinal))
            {
                if (callbacks.Language == "en" && text.StartsWith("i "))
                {
                }
                else
                {
                    return text; // too hard to say if uppercase after "..."
                }
            }

            if (textBefore != null && textBefore.EndsWith(" - ", System.StringComparison.Ordinal) && !textBefore.EndsWith(". - ", System.StringComparison.Ordinal))
            {
                return text;
            }

            // Skip words like iPhone, iPad...
            if (text[0] == 'i' && text.Length > 1 && char.IsUpper(text[1]))
            {
                return text;
            }

            if (Helper.IsTurkishLittleI(text[0], callbacks.Encoding, callbacks.Language))
            {
                return Helper.GetTurkishUppercaseLetter(text[0], callbacks.Encoding) + text.Substring(1);
            }

            text = char.ToUpper(text[0]) + text.Substring(1);
            return text;
        }

    }
}
