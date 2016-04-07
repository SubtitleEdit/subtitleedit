using System.Collections.Generic;
using System.Text.RegularExpressions;

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
                if (SubtitleEditRegex.LittleIRegex.IsMatch(text))
                {
                    foreach (var regex in SubtitleEditRegex.DanishLetterI.DanishCompiledRegexList)
                    {
                        Match match = regex.Match(text);
                        while (match.Success)
                        {
                            // Get lower 'i' index from matched value.
                            int iIdx = SubtitleEditRegex.LittleIRegex.Match(match.Value).Index;
                            // Remove 'i' from given index and insert new uppwercase 'I'.
                            string temp = match.Value.Remove(iIdx, 1).Insert(iIdx, "I");

                            int index = match.Index;
                            if (index + match.Value.Length >= text.Length)
                                text = text.Substring(0, index) + temp;
                            else
                                text = text.Substring(0, index) + temp + text.Substring(index + match.Value.Length);
                            match = match.NextMatch();
                        }
                    }
                }

                if (SubtitleEditRegex.DanishLetterI.RegExIDag.IsMatch(text))
                    text = SubtitleEditRegex.DanishLetterI.RegExIDag.Replace(text, "i dag");

                if (SubtitleEditRegex.DanishLetterI.RegExIGaar.IsMatch(text))
                    text = SubtitleEditRegex.DanishLetterI.RegExIGaar.Replace(text, "i går");

                if (SubtitleEditRegex.DanishLetterI.RegExIMorgen.IsMatch(text))
                    text = SubtitleEditRegex.DanishLetterI.RegExIMorgen.Replace(text, "i morgen");

                if (SubtitleEditRegex.DanishLetterI.RegExIAlt.IsMatch(text))
                    text = SubtitleEditRegex.DanishLetterI.RegExIAlt.Replace(text, "i alt");

                if (SubtitleEditRegex.DanishLetterI.RegExIGang.IsMatch(text))
                    text = SubtitleEditRegex.DanishLetterI.RegExIGang.Replace(text, "i gang");

                if (SubtitleEditRegex.DanishLetterI.RegExIStand.IsMatch(text))
                    text = SubtitleEditRegex.DanishLetterI.RegExIStand.Replace(text, "i stand");

                if (SubtitleEditRegex.DanishLetterI.RegExIOevrigt.IsMatch(text))
                    text = SubtitleEditRegex.DanishLetterI.RegExIOevrigt.Replace(text, "i øvrigt");

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
