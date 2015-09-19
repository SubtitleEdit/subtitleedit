using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixStartWithUppercaseLetterAfterColon : IFixCommonError
    {

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.StartWithUppercaseLetterAfterColon;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = new Paragraph(subtitle.Paragraphs[i]);
                Paragraph last = subtitle.GetParagraphOrDefault(i - 1);
                string oldText = p.Text;
                int skipCount = 0;

                if (last != null)
                {
                    string lastText = HtmlUtil.RemoveHtmlTags(last.Text);
                    if (lastText.EndsWith(':') || lastText.EndsWith(';'))
                    {
                        var st = new StripableText(p.Text);
                        if (st.StrippedText.Length > 0 && st.StrippedText[0] != char.ToUpper(st.StrippedText[0]))
                            p.Text = st.Pre + char.ToUpper(st.StrippedText[0]) + st.StrippedText.Substring(1) + st.Post;
                    }
                }

                if (oldText.Contains(new[] { ':', ';' }))
                {
                    bool lastWasColon = false;
                    for (int j = 0; j < p.Text.Length; j++)
                    {
                        var s = p.Text[j];
                        if (s == ':' || s == ';')
                        {
                            lastWasColon = true;
                        }
                        else if (lastWasColon)
                        {
                            var startFromJ = p.Text.Substring(j);
                            if (skipCount > 0)
                                skipCount--;
                            else if (startFromJ.StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                                skipCount = 2;
                            else if (startFromJ.StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                                skipCount = 2;
                            else if (startFromJ.StartsWith("<u>", StringComparison.OrdinalIgnoreCase))
                                skipCount = 2;
                            else if (startFromJ.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) && p.Text.Substring(j).Contains('>'))
                                skipCount = startFromJ.IndexOf('>') - startFromJ.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
                            else if (Helper.IsTurkishLittleI(s, callbacks.Encoding, callbacks.Language))
                            {
                                p.Text = p.Text.Remove(j, 1).Insert(j, Helper.GetTurkishUppercaseLetter(s, callbacks.Encoding).ToString(CultureInfo.InvariantCulture));
                                lastWasColon = false;
                            }
                            else if (char.IsLower(s))
                            {
                                // iPhone
                                bool change = true;
                                if (s == 'i' && p.Text.Length > j + 1)
                                {
                                    if (p.Text[j + 1] == char.ToUpper(p.Text[j + 1]))
                                        change = false;
                                }
                                if (change)
                                    p.Text = p.Text.Remove(j, 1).Insert(j, char.ToUpper(s).ToString(CultureInfo.InvariantCulture));
                                lastWasColon = false;
                            }
                            else if (!(" " + Environment.NewLine).Contains(s))
                                lastWasColon = false;
                        }
                    }
                }

                if (oldText != p.Text && callbacks.AllowFix(p, fixAction))
                {
                    noOfFixes++;
                    subtitle.Paragraphs[i].Text = p.Text;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, language.StartWithUppercaseLetterAfterColon, noOfFixes.ToString(CultureInfo.InvariantCulture));
        }

    }
}
