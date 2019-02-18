using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixStartWithUppercaseLetterAfterColon : IFixCommonError
    {
        private static readonly char[] ExpectedChars = { ':', ';' };
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.StartWithUppercaseLetterAfterColon;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (!callbacks.AllowFix(p, fixAction))
                {
                    continue;
                }

                Paragraph last = subtitle.GetParagraphOrDefault(i - 1);
                string text = subtitle.Paragraphs[i].Text;
                string oldText = text;
                int skipCount = 0;

                if (last != null)
                {
                    string lastText = HtmlUtil.RemoveHtmlTags(last.Text);
                    if (lastText.EndsWith(':') || lastText.EndsWith(';'))
                    {
                        var st = new StrippableText(text);
                        if (ShouldCapitalize(st.StrippedText))
                        {
                            text = st.CombineWithPrePost(st.StrippedText.CapitalizeFirstLetter());
                        }
                    }
                }

                if (oldText.Contains(ExpectedChars))
                {
                    bool lastWasColon = false;
                    for (int j = 0; j < text.Length; j++)
                    {
                        char ch = text[j];
                        if (ch == ':' || ch == ';')
                        {
                            lastWasColon = true;
                        }
                        else if (lastWasColon)
                        {
                            // skip whitespace index
                            if (j + 2 < text.Length && ch == ' ')
                            {
                                ch = text[++j];
                            }

                            var textFromJ = text.Substring(j);
                            if (textFromJ.Length > 3 && textFromJ[0] == '<' && textFromJ[2] == '>' && (textFromJ[1] == 'i' || textFromJ[1] == 'b' || textFromJ[1] == 'u'))
                            {
                                skipCount = 2;
                            }
                            else if (textFromJ.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) && text.Substring(j).Contains('>'))
                            {
                                skipCount = (j + textFromJ.IndexOf('>', 6)) - j;
                            }
                            else if (Helper.IsTurkishLittleI(ch, callbacks.Encoding, callbacks.Language))
                            {
                                text = text.Remove(j, 1).Insert(j, Helper.GetTurkishUppercaseLetter(ch, callbacks.Encoding).ToString(CultureInfo.InvariantCulture));
                                lastWasColon = false;
                            }
                            else if (char.IsLower(ch))
                            {
                                // iPhone
                                bool change = true;
                                if (ch == 'i' && text.Length > j + 1)
                                {
                                    if (text[j + 1] == char.ToUpper(text[j + 1]))
                                    {
                                        change = false;
                                    }
                                }
                                if (change)
                                {
                                    textFromJ = textFromJ.CapitalizeFirstLetter();
                                    text = text.Remove(j);
                                    text = text.Insert(j, textFromJ);
                                }

                                lastWasColon = false;
                            }
                            else if (!(" " + Environment.NewLine).Contains(ch))
                            {
                                lastWasColon = false;
                            }

                            // move the: 'j' pointer and reset skipCount to 0
                            if (skipCount > 0)
                            {
                                j += skipCount;
                                skipCount = 0;
                            }
                        }
                    }
                }

                if (!oldText.Equals(text, StringComparison.Ordinal))
                {
                    noOfFixes++;
                    subtitle.Paragraphs[i].Text = text;
                    callbacks.AddFixToListView(p, fixAction, oldText, text);
                }
            }

            callbacks.UpdateFixStatus(noOfFixes, language.StartWithUppercaseLetterAfterColon, noOfFixes.ToString(CultureInfo.InvariantCulture));
        }

        protected static bool ShouldCapitalize(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            char firstChar = input[0];
            // not a letter or already uppercase
            if (char.IsLetter(firstChar) == false || char.IsUpper(firstChar))
            {
                return false;
            }

            // ignore: iPhone, iPad...
            if (input.Length > 1 && input[0] == 'i' && char.IsUpper(input[1]))
            {
                return false;
            }

            return true;
        }
    }
}
