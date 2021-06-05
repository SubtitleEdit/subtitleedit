using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixUppercaseIInsideWords : IFixCommonError
    {
        public static class Language
        {
            public static string FixUppercaseIInsideLowercaseWord { get; set; } = "Fix uppercase 'i' inside lowercase word";
            public static string FixUppercaseIInsideLowercaseWords { get; set; } = "Fix uppercase 'i' inside lowercase words (OCR error)";
        }

        private static readonly Regex ReAfterLowercaseLetter = new Regex(@"\p{Ll}I", RegexOptions.Compiled);
        private static readonly Regex ReBeforeLowercaseLetter = new Regex(@"\p{L}I\p{Ll}", RegexOptions.Compiled);

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.FixUppercaseIInsideLowercaseWord;
            var language = callbacks.Language;
            int uppercaseIsInsideLowercaseWords = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string oldText = p.Text;
                var st = new StrippableText(p.Text);

                Match match = ReAfterLowercaseLetter.Match(st.StrippedText);
                while (match.Success)
                {
                    if (!(match.Index > 1 && st.StrippedText.Substring(match.Index - 1, 2) == "Mc") // irish names, McDonalds etc.
                        && st.StrippedText[match.Index + 1] == 'I'
                        && callbacks.AllowFix(p, fixAction))
                    {
                        string word = GetWholeWord(st.StrippedText, match.Index);
                        if (!callbacks.IsName(word))
                        {
                            var old = st.StrippedText;
                            st.StrippedText = st.StrippedText.Substring(0, match.Index + 1) + "l";
                            if (match.Index + 2 < old.Length)
                            {
                                st.StrippedText += old.Substring(match.Index + 2);
                            }

                            p.Text = st.MergedString;

                            st = new StrippableText(p.Text);
                            uppercaseIsInsideLowercaseWords++;
                            callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                            match = ReAfterLowercaseLetter.Match(st.StrippedText, match.Index);
                        }
                        else
                        {
                            match = match.NextMatch();
                        }
                    }
                    else
                    {
                        match = match.NextMatch();
                    }
                }

                match = ReBeforeLowercaseLetter.Match(st.StrippedText);
                while (match.Success)
                {
                    string word = GetWholeWord(st.StrippedText, match.Index);
                    if (!callbacks.IsName(word))
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            if (language == "en" && word.EndsWith('s') &&
                                     p.Text.Length > match.Index + 1 &&
                                     word.TrimEnd('s') == word.TrimEnd('s').ToUpperInvariant())
                            {
                                // skip words like "MRIs" where the last 's' indicates plural
                            }
                            else
                            {
                                var before = st.StrippedText[match.Index];
                                var after = '\0';
                                if (match.Index < st.StrippedText.Length - 3)
                                {
                                    after = st.StrippedText[match.Index + 2];
                                }

                                if (before != '\0' && char.IsUpper(before) && after != '\0' && char.IsLower(after) &&
                                    !Utilities.LowercaseVowels.Contains(char.ToLower(before)) && !Utilities.LowercaseVowels.Contains(after))
                                {
                                    st.StrippedText = st.StrippedText.Remove(match.Index + 1, 1).Insert(match.Index + 1, "i");
                                    p.Text = st.MergedString;
                                    uppercaseIsInsideLowercaseWords++;
                                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                                }
                                else if (@"‘’¡¿„“()[]♪'. @".Contains(before) && !Utilities.LowercaseVowels.Contains(char.ToLower(after)))
                                {
                                }
                                else
                                {
                                    bool ok = !(match.Index >= 1 && st.StrippedText.Substring(match.Index - 1, 2) == "Mc");

                                    if (ok)
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index + 1, 1).Insert(match.Index + 1, "l");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                }
                            }
                        }
                    }
                    match = match.NextMatch();
                }
            }
            callbacks.UpdateFixStatus(uppercaseIsInsideLowercaseWords, Language.FixUppercaseIInsideLowercaseWords);
        }

        private static string GetWholeWord(string text, int index)
        {
            int start = index;
            while (start > 0 && !(Environment.NewLine + @" ,.!?""'=()[]/-¿¡«»“”>—").Contains(text[start - 1]) ||
                   start > 1 && text[start - 1] == '\'' && char.IsLetter(text[start - 2]))
            {
                start--;
            }

            int end = index;
            while (end + 1 < text.Length && !(Environment.NewLine + @" ,.!?:;""'=()[]/-«»“”<—").Contains(text[end + 1]) ||
                   end + 2 < text.Length && text[end + 1] == '\'' && char.IsLetter(text[end + 2]))
            {
                end++;
            }

            return text.Substring(start, end - start + 1);
        }
    }
}
