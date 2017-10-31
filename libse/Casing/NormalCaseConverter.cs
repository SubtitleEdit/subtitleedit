using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Casing
{
    public class NormalCaseConverter : NameCaseConverter
    {
        private const string Pre = " >¡¿♪♫([";
        private const string Post = " <!?.:;,♪♫)]";

        private static readonly char[] ExpectedCharsArray = { '.', '!', '?', ':', ';', ')', ']', '}', '(', '[', '{' };

        private readonly CasingOptions _options;

        public NormalCaseConverter(IList<string> nameList, CasingOptions options) : base(nameList)
        {
            _options = options;
        }

        public override void Convert(Subtitle subtitle, CasingContext context)
        {
            string preText = string.Empty;
            foreach (var p in subtitle.Paragraphs)
            {
                string text = p.Text;

                if (_options.OnlyWhereUppercase && !text.Equals(text.ToUpper(context.Culture), StringComparison.Ordinal))
                {
                    continue;
                }

                text = text.ToLower(context.Culture);

                // NOTE: only save this change if others changes were made!
                text = text.FixExtraSpaces();

                if (context.IsEnglish)
                {
                    text = FixEnglishAloneILowerToUpper(text);
                }

                // fix name casings
                if (_options.Names)
                {
                    text = DoNameCasing(text);
                }

                text = ProcessText(new StrippableText(text), preText);

                if (!text.Equals(p.Text, StringComparison.Ordinal))
                {
                    // if no listener is bound, update paragraph directly (e.g: in cmd mode)
                    if (!OnTextChanged(p, new CasingResult { Before = p.Text, After = text }))
                    {
                        p.Text = text;
                    }
                    Count++;
                }
                preText = p.Text;
            }
        }

        private static string FixEnglishAloneILowerToUpper(string text)
        {
            for (var indexOfI = text.IndexOf('i'); indexOfI >= 0; indexOfI = text.IndexOf('i', indexOfI + 1))
            {
                if (indexOfI == 0 || Pre.Contains(text[indexOfI - 1]))
                {
                    if (indexOfI + 1 == text.Length || Post.Contains(text[indexOfI + 1]))
                    {
                        text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                    }
                }
            }
            return text;
        }

        private string ProcessText(StrippableText st, string preText)
        {
            string strippedText = st.StrippedText;
            if (ShouldStartWithUppercase(st, preText))
            {
                strippedText = strippedText.CapitalizeFirstLetter();
            }
            // fix lowercase inside text
            if (strippedText.Contains(ExpectedCharsArray))
            {
                const string breakAfterChars = @".!?:;)]}([{";
                const string expectedChars = "\"`´'()<>!?.- \r\n";
                var sb = new StringBuilder();
                bool lastWasBreak = false;
                for (int i = 0; i < strippedText.Length; i++)
                {
                    char ch = strippedText[i];
                    if (lastWasBreak)
                    {
                        if (expectedChars.Contains(ch))
                        {
                            sb.Append(ch);
                        }
                        else if ((sb.EndsWith('<') || sb.ToString().EndsWith("</", StringComparison.Ordinal)) && i + 1 < strippedText.Length && strippedText[i + 1] == '>')
                        { // tags
                            sb.Append(ch); // i, b , u
                        }
                        else if (sb.EndsWith('<') && ch == '/' && i + 2 < strippedText.Length && strippedText[i + 2] == '>')
                        { // tags
                            sb.Append(ch); // reviewing: after adding / the condition above may run
                        }
                        else if (sb.ToString().EndsWith("... ", StringComparison.Ordinal))
                        {
                            sb.Append(ch);
                            lastWasBreak = false;
                        }
                        else
                        {
                            if (breakAfterChars.Contains(ch))
                            {
                                sb.Append(ch);
                            }
                            else
                            {
                                lastWasBreak = false;
                                sb.Append(char.ToUpper(ch));
                            }
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                        if (breakAfterChars.Contains(ch))
                        {
                            var idx = sb.ToString().IndexOf('[');
                            if (ch == ']' && idx > 1)
                            { // I [Motor roaring] love you!
                                string temp = sb.ToString(0, idx - 1).Trim();
                                if (temp.Length > 0 && !char.IsLower(temp[temp.Length - 1]))
                                    lastWasBreak = true;
                            }
                            else
                            {
                                idx = sb.ToString().LastIndexOf(' ');
                                if (idx >= 0 && idx < sb.Length - 2 && !IsInMiddleOfUrl(i - idx, strippedText.Substring(idx + 1)))
                                {
                                    lastWasBreak = true;
                                }
                            }
                        }
                    }
                }
                strippedText = sb.ToString();
            }

            return st.CombineWithPrePost(strippedText);
        }

        private static bool ShouldStartWithUppercase(StrippableText st, string preText)
        {
            string preTextNoTags = HtmlUtil.RemoveHtmlTags(preText).TrimEnd().TrimEnd('\"').TrimEnd();
            const string lineCloseChars = ".?!)]:";
            // check if pre-text if closed
            if (!string.IsNullOrEmpty(preTextNoTags))
            {
                char lastChar = preTextNoTags[preTextNoTags.Length - 1];
                // check if ellipses
                if (preText.EndsWith("...", StringComparison.Ordinal))
                {
                    return false;
                }
                // preText may ends with ♪/♫, do start with uppercase if *st.Pre (current working text/line)
                // doesn't start with music symbol.
                if (!lineCloseChars.Contains(preText[preText.Length - 1]))
                {
                    // p1 = ♪ Foobar ♪
                    // p2 = foobar
                    // p3 = ♪ foobar ♪
                    // Do fix p2 if p1->p2
                    // Do not fix p2 => p1->p3
                    if ((lastChar == '♪' || lastChar == '♫') && st.Pre.Contains(new[] { '♪', '♫' }))
                    {
                        return false;
                    }
                }
            }
            string strippedText = st.StrippedText;
            // check urls
            if (strippedText.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ||
                strippedText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            // ignore if current line start with ellipses
            if (st.Pre.Contains("..."))
            {
                return false;
            }
            // ignore words like: iPhone, iPad...
            if (strippedText.Length > 2 && strippedText[0] == 'i')
            {
                if (char.IsUpper(strippedText[1]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsInMiddleOfUrl(int idx, string s)
        {
            if (idx < s.Length - 1 && (char.IsWhiteSpace(s[idx]) || char.IsPunctuation(s[idx])))
                return false;
            return s.StartsWith("www.", StringComparison.OrdinalIgnoreCase) || s.StartsWith("http", StringComparison.OrdinalIgnoreCase);
        }

    }
}
