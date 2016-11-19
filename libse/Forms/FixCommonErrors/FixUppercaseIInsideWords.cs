using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixUppercaseIInsideWords : IFixCommonError
    {
        private static readonly Regex ReAfterLowercaseLetter = new Regex(@"[a-zæøåäöéùáàìéóúñüéíóúñü]I", RegexOptions.Compiled);
        private static readonly Regex ReBeforeLowercaseLetter = new Regex(@"I[a-zæøåäöéùáàìéóúñüéíóúñü]", RegexOptions.Compiled);

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixUppercaseIInsideLowercaseWord;
            int uppercaseIsInsideLowercaseWords = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string oldText = p.Text;

                Match match = ReAfterLowercaseLetter.Match(p.Text);
                while (match.Success)
                {
                    if (!(match.Index > 1 && p.Text.Substring(match.Index - 1, 2) == "Mc") // irish names, McDonalds etc.
                        && p.Text[match.Index + 1] == 'I'
                        && callbacks.AllowFix(p, fixAction))
                    {
                        p.Text = p.Text.Substring(0, match.Index + 1) + "l";
                        if (match.Index + 2 < oldText.Length)
                            p.Text += oldText.Substring(match.Index + 2);

                        uppercaseIsInsideLowercaseWords++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    match = match.NextMatch();
                }

                var st = new StrippableText(p.Text);
                match = ReBeforeLowercaseLetter.Match(st.StrippedText);
                while (match.Success)
                {
                    string word = GetWholeWord(st.StrippedText, match.Index);
                    if (!callbacks.IsName(word))
                    {
                        if (callbacks.AllowFix(p, fixAction))
                        {
                            if (word.Equals("internal", StringComparison.OrdinalIgnoreCase) ||
                                word.Equals("island", StringComparison.OrdinalIgnoreCase) ||
                                word.Equals("islands", StringComparison.OrdinalIgnoreCase))
                            {
                            }
                            else if (match.Index == 0)
                            {  // first letter in paragraph

                                //too risky! - perhaps if periods is fixed at the same time... or too complicated!?
                                //if (isLineContinuation)
                                //{
                                //    st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
                                //    p.Text = st.MergedString;
                                //    uppercaseIsInsideLowercaseWords++;
                                //    AddFixToListView(p, fixAction, oldText, p.Text);
                                //}
                            }
                            else
                            {
                                if (match.Index > 2 && st.StrippedText[match.Index - 1] == ' ')
                                {
                                    if ((Utilities.AllLettersAndNumbers + @",").Contains(st.StrippedText[match.Index - 2])
                                        && match.Length >= 2 && Utilities.LowercaseVowels.Contains(char.ToLower(match.Value[1])))
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                }
                                else if (match.Index > Environment.NewLine.Length + 1 && Environment.NewLine.Contains(st.StrippedText[match.Index - 1]))
                                {
                                    if ((Utilities.AllLettersAndNumbers + @",").Contains(st.StrippedText[match.Index - Environment.NewLine.Length + 1])
                                        && match.Length >= 2 && Utilities.LowercaseVowels.Contains(match.Value[1]))
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                }
                                else if (match.Index > 1 && ((st.StrippedText[match.Index - 1] == '\"') || (st.StrippedText[match.Index - 1] == '\'') ||
                                                             (st.StrippedText[match.Index - 1] == '>') || (st.StrippedText[match.Index - 1] == '-')))
                                {
                                }
                                else
                                {
                                    var before = '\0';
                                    var after = '\0';
                                    if (match.Index > 0)
                                        before = st.StrippedText[match.Index - 1];
                                    if (match.Index < st.StrippedText.Length - 2)
                                        after = st.StrippedText[match.Index + 1];
                                    if (before != '\0' && char.IsUpper(before) && after != '\0' && char.IsLower(after) &&
                                        !Utilities.LowercaseVowels.Contains(char.ToLower(before)) && !Utilities.LowercaseVowels.Contains(after))
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "i");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                    else if (@"‘’¡¿„“()[]♪'. @".Contains(before) && !Utilities.LowercaseVowels.Contains(char.ToLower(after)))
                                    {
                                    }
                                    else
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
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
            callbacks.UpdateFixStatus(uppercaseIsInsideLowercaseWords, language.FixUppercaseIInsindeLowercaseWords, language.XUppercaseIsFoundInsideLowercaseWords);
        }

        private static string GetWholeWord(string text, int index)
        {
            int start = index;
            while (start > 0 && !(Environment.NewLine + @" ,.!?""'=()/-").Contains(text[start - 1]))
                start--;

            int end = index;
            while (end + 1 < text.Length && !(Environment.NewLine + @" ,.!?""'=()/-").Contains(text[end + 1]))
                end++;

            return text.Substring(start, end - start + 1);
        }

    }
}
