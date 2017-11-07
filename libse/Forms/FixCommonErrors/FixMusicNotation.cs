using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixMusicNotation : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixMusicNotation;
            int fixCount = 0;
            string[] musicSymbols = Configuration.Settings.Tools.MusicSymbolToReplace.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    var newText = oldText;
                    foreach (string musicSymbol in musicSymbols)
                    {
                        if (musicSymbol == "#")
                        {
                            int idx = newText.IndexOf('#');
                            while (idx >= 0)
                            {
                                if (IsQualified(newText, idx))
                                {
                                    newText = newText.Remove(idx, 1);
                                    newText = newText.Insert(idx, Configuration.Settings.Tools.MusicSymbol);
                                }
                                idx = newText.IndexOf('#', idx + 1);
                            }
                        }
                        else
                        {
                            newText = newText.Replace(musicSymbol, Configuration.Settings.Tools.MusicSymbol);
                            newText = newText.Replace(musicSymbol.ToUpper(), Configuration.Settings.Tools.MusicSymbol);
                        }
                    }
                    var noTagsText = HtmlUtil.RemoveHtmlTags(newText);
                    if (newText != oldText && noTagsText != HtmlUtil.RemoveHtmlTags(oldText))
                    {
                        p.Text = newText;
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.FixMusicNotation, language.XFixMusicNotation);
        }

        public static bool IsQualified(string text, int idx)
        {
            if (idx == 0 || idx + 1 == text.Length)
            {
                return true;
            }
            // Do not replace number-sign 'MAN #1' with music symbols in text like:
            //- <font color="#804040">MAN #1</font>: Oh, yeah.
            if (text[idx - 1] == ' ' && char.IsDigit(text[idx + 1]))
            {
                return false;
            }
            if (text.Contains("<font ", StringComparison.OrdinalIgnoreCase) && (text[idx - 1] == '"') && (text.Length > idx + 2 && Uri.IsHexDigit(text[idx + 1]) && Uri.IsHexDigit(text[idx + 2])))
            {
                return false;
            }
            return true;
        }

    }
}
