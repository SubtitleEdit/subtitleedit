using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixDanishLetterI : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            const string fixAction = "Fix Danish letter 'i'";
            int fixCount = 0;

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = p.Text;
                string oldText = text;

                // Make sure text contains lower: 'i'.
                if (RegexUtils.LittleIRegex.IsMatch(text))
                {
                    foreach (var regex in RegexUtils.DanishLetterI.DanishCompiledRegexList)
                    {
                        Match match = regex.Match(text);
                        while (match.Success)
                        {
                            // Get lower 'i' index from matched value.
                            int iIdx = RegexUtils.LittleIRegex.Match(match.Value).Index;
                            // Remove 'i' from given index and insert new uppwercase 'I'.
                            string temp = match.Value.Remove(iIdx, 1).Insert(iIdx, "I");

                            int index = match.Index;
                            if (index + match.Value.Length >= text.Length)
                            {
                                text = text.Substring(0, index) + temp;
                            }
                            else
                            {
                                text = text.Substring(0, index) + temp + text.Substring(index + match.Value.Length);
                            }

                            match = match.NextMatch();
                        }
                    }
                }

                if (RegexUtils.DanishLetterI.RegExIDag.IsMatch(text))
                {
                    text = RegexUtils.DanishLetterI.RegExIDag.Replace(text, "i dag");
                }

                if (RegexUtils.DanishLetterI.RegExIGaar.IsMatch(text))
                {
                    text = RegexUtils.DanishLetterI.RegExIGaar.Replace(text, "i går");
                }

                if (RegexUtils.DanishLetterI.RegExIMorgen.IsMatch(text))
                {
                    text = RegexUtils.DanishLetterI.RegExIMorgen.Replace(text, "i morgen");
                }

                if (RegexUtils.DanishLetterI.RegExIAlt.IsMatch(text))
                {
                    text = RegexUtils.DanishLetterI.RegExIAlt.Replace(text, "i alt");
                }

                if (RegexUtils.DanishLetterI.RegExIGang.IsMatch(text))
                {
                    text = RegexUtils.DanishLetterI.RegExIGang.Replace(text, "i gang");
                }

                if (RegexUtils.DanishLetterI.RegExIStand.IsMatch(text))
                {
                    text = RegexUtils.DanishLetterI.RegExIStand.Replace(text, "i stand");
                }

                if (RegexUtils.DanishLetterI.RegExIOevrigt.IsMatch(text))
                {
                    text = RegexUtils.DanishLetterI.RegExIOevrigt.Replace(text, "i øvrigt");
                }

                if (text != oldText && callbacks.AllowFix(p, fixAction))
                {
                    p.Text = text;
                    fixCount++;
                    callbacks.AddFixToListView(subtitle.Paragraphs[i], fixAction, oldText, text);
                }
            }

            callbacks.UpdateFixStatus(fixCount, language.FixDanishLetterI, string.Format(language.XIsChangedToUppercase, fixCount));
        }

    }
}
