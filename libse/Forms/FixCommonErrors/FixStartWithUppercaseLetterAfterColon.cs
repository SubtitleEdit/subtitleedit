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
                        var s = text[j];
                        if (s == ':' || s == ';')
                        {
                            lastWasColon = true;
                        }
                        else if (lastWasColon)
                        {
                            // skip whitespace index
                            if (j + 2 < text.Length && text[j] == ' ')
                            {
                                s = text[++j];
                            }

                            var startFromJ = text.Substring(j);
                            if (startFromJ.Length > 3 && startFromJ[0] == '<' && startFromJ[2] == '>' && (startFromJ[1] == 'i' || startFromJ[1] == 'b' || startFromJ[1] == 'u'))
                            {
                                skipCount = 2;
                            }
                            else if (startFromJ.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) && text.Substring(j).Contains('>'))
                            {
                                skipCount = (j + startFromJ.IndexOf('>', 6)) - j;
                            }
                            else if (Helper.IsTurkishLittleI(s, callbacks.Encoding, callbacks.Language))
                            {
                                text = text.Remove(j, 1).Insert(j, Helper.GetTurkishUppercaseLetter(s, callbacks.Encoding).ToString(CultureInfo.InvariantCulture));
                                lastWasColon = false;
                            }
                            else if (char.IsLower(s))
                            {
                                // iPhone
                                bool change = true;
                                if (s == 'i' && text.Length > j + 1)
                                {
                                    if (text[j + 1] == char.ToUpper(text[j + 1]))
                                    {
                                        change = false;
                                    }
                                }
                                if (change)
                                {
                                    string textFromIdx = text.Substring(j).CapitalizeFirstLetter();
                                    text = text.Remove(j);
                                    text = text.Insert(j, textFromIdx);
                                }

                                lastWasColon = false;
                            }
                            else if (!(" " + Environment.NewLine).Contains(s))
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
