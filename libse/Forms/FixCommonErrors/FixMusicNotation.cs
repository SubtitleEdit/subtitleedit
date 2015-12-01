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
                    bool containsFontTag = oldText.Contains("<font ", StringComparison.OrdinalIgnoreCase);
                    foreach (string musicSymbol in musicSymbols)
                    {
                        if (containsFontTag && musicSymbol == "#")
                        {
                            var idx = newText.IndexOf('#');
                            while (idx >= 0)
                            {
                                // <font color="#808080">NATIVE HAWAIIAN CHANTING</font>
                                var isInsideFontTag = (idx < 13) ? false : (newText[idx - 1] == '"' && (newText.Length > idx + 2 && Uri.IsHexDigit(newText[idx + 1]) && Uri.IsHexDigit(newText[idx + 2])));
                                if (!isInsideFontTag)
                                {
                                    newText = newText.Remove(idx, 1);
                                    newText = newText.Insert(idx, musicSymbol);
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
    }
}
