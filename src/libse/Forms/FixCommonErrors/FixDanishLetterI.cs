using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixDanishLetterI : IFixCommonError
    {
        public static class Language
        {
            public static string FixDanishLetterI { get; set; } = "Fix Danish letter 'i'";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixCount = 0;

            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var text = p.Text;
                var oldText = text;

                // Make sure text contains lower: 'i'.
                if (callbacks.AllowFix(p, Language.FixDanishLetterI))
                {
                    if (RegexUtils.LittleIRegex.IsMatch(text))
                    {
                        foreach (var regex in RegexUtils.DanishLetterI.DanishCompiledRegexList)
                        {
                            var match = regex.Match(text);
                            while (match.Success)
                            {
                                // Get lower 'i' index from matched value.
                                var iIdx = RegexUtils.LittleIRegex.Match(match.Value).Index;
                                // Remove 'i' from given index and insert new uppwercase 'I'.
                                var temp = match.Value.Remove(iIdx, 1).Insert(iIdx, "I");

                                var index = match.Index;
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

                    text = RegexUtils.DanishLetterI.DanishLetterIRegex.Replace(text, "i $1");

                    if (text != oldText)
                    {
                        p.Text = text;
                        fixCount++;
                        callbacks.AddFixToListView(subtitle.Paragraphs[i], Language.FixDanishLetterI, oldText, text);
                    }
                }
            }

            callbacks.UpdateFixStatus(fixCount, Language.FixDanishLetterI);
        }
    }
}