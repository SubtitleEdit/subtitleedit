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
            string[] musicSymbols = Configuration.Settings.Tools.MusicSymbolReplace.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                        var ms = musicSymbol.Trim();
                        if (containsFontTag && ms == "#")
                        {
                            var idx = newText.IndexOf('#');
                            while (idx >= 0)
                            {
                                // <font color="#808080">NATIVE HAWAIIAN CHANTING</font>
                                var isInsideFontTag = idx >= 13 && (newText[idx - 1] == '"' && newText.Length > idx + 2 && Uri.IsHexDigit(newText[idx + 1]) && Uri.IsHexDigit(newText[idx + 2]));
                                if (!isInsideFontTag)
                                {
                                    newText = newText.Remove(idx, 1);
                                    newText = newText.Insert(idx, Configuration.Settings.Tools.MusicSymbol);
                                }

                                idx = newText.IndexOf('#', idx + 1);
                            }
                        }                        
                        else
                        {
                            var fix = true;
                            if (ms == "#" && newText.Contains("#") && !newText.Contains("# "))
                            {
                                int count = Utilities.CountTagInText(newText, '#');
                                if (count == 1 )
                                {
                                    var idx = newText.IndexOf('#');
                                    if (idx < newText.Length - 2)
                                    {
                                        if (char.IsLetterOrDigit(newText[idx + 1]))
                                        {
                                            fix = false;
                                        }
                                    }
                                }
                                else if (!newText.EndsWith('#'))
                                {
                                    var idx = newText.IndexOf('#');
                                    int hashTagCount = 0;
                                    while (idx >= 0)
                                    {
                                        if (char.IsLetterOrDigit(newText[idx + 1]))
                                        {
                                            hashTagCount++;
                                        }
                                        idx = newText.IndexOf('#', idx + 1);
                                    }
                                    fix = hashTagCount == 0;
                                }
                            }

                            if (fix)
                            {
                                newText = newText.Replace(ms, Configuration.Settings.Tools.MusicSymbol);
                                newText = newText.Replace(ms.ToUpper(), Configuration.Settings.Tools.MusicSymbol);
                            }
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
