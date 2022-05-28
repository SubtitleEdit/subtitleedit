using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixMusicNotation : IFixCommonError
    {
        public static class Language
        {
            public static string FixMusicNotation { get; set; } = "Replace music symbols (e.g. âTª) with preferred symbol";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.FixMusicNotation;
            var fixCount = 0;
            var musicSymbols = Configuration.Settings.Tools.MusicSymbolReplace.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    var newText = oldText;
                    var containsFontTag = oldText.Contains("<font ", StringComparison.OrdinalIgnoreCase);
                    foreach (var musicSymbol in musicSymbols)
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
                                var count = Utilities.CountTagInText(newText, '#');
                                if (count == 1)
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
                                    var hashTagCount = 0;
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
                                newText = newText.Replace(ms.ToUpperInvariant(), Configuration.Settings.Tools.MusicSymbol);
                            }
                        }
                    }

                    // convert line start & ends with question marks into music symbol
                    if (newText.Contains('?'))
                    {
                        newText = HandleQuestionMarks(newText);
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
            callbacks.UpdateFixStatus(fixCount, Language.FixMusicNotation);
        }

        private static string HandleQuestionMarks(string input)
        {
            var narrator = string.Empty;
            var text = input;

            // jump narrator
            var colonIdx = text.IndexOf(':');
            if (colonIdx > 0 && colonIdx + 1 < text.Length && (text[colonIdx + 1] == ' ' || text[colonIdx + 1] == '?'))
            {
                // jump white space
                narrator = text.Substring(0, colonIdx + 1);
                text = text.Substring(colonIdx + 1).TrimStart();
            }

            // be stub when looking for question marks
            var noTagsText = HtmlUtil.RemoveHtmlTags(text, true);

            // check if text starts and ends with?
            if (noTagsText.StartsWith('?') && noTagsText.EndsWith('?') && noTagsText.Length > 2)
            {
                // find correct start & end question mark position taking tags in consideration
                var startIdx = text.IndexOf('?');
                var endIdx = text.LastIndexOf('?');

                // get pre and post text excluding question marks
                var preText = text.Substring(0, startIdx);
                var postText = text.Substring(endIdx + 1);

                text = text.Substring(startIdx + 1, endIdx - (startIdx + 1));

                // construct music text and restore preText/postText (mostly tags)
                text = preText + WrapInMusic(text) + postText;
            }

            // restore narrator
            return (narrator + " " + text).TrimStart();
        }

        private static string WrapInMusic(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Configuration.Settings.Tools.MusicSymbol + Configuration.Settings.Tools.MusicSymbol;
            }
            return $"{Configuration.Settings.Tools.MusicSymbol} {input.Trim()} {Configuration.Settings.Tools.MusicSymbol}";
        }
    }

}
