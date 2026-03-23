using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public class KaraokeWordTransform : TypeWriterEffect
    {
        public override string[] Transform(string text)
        {
            // <font>foobar foo bar <i>zz</i>
            var result = new List<string>();
            var len = text.Length;

            var lastFontCloseIdx = len;
            const string fontClose = "</font>";
            for (var i = 0; i < len; i++)
            {
                var ch = text[i];
                if (HtmlUtil.IsStartTagSymbol(ch))
                {
                    // skip entire tag or just the char where we are current
                    i = Math.Max(i, text.IndexOf(HtmlUtil.GetClosingPair(ch), i + 1));
                }
                else if (char.IsWhiteSpace(ch))
                {
                    var karaoke = text.Substring(0, i) + fontClose + text.Substring(i);
                    lastFontCloseIdx = i;
                    result.Add(karaoke);
                    // skip only whitespace
                    while (i < len && char.IsWhiteSpace(text[i]))
                    {
                        i++;
                    }
                }
            }

            // insert last sentence
            var lastInsertIndex = CalculateLastFontInsertIndex(text, lastFontCloseIdx);
            result.Add(text.Substring(0, lastInsertIndex) + fontClose + text.Substring(lastInsertIndex));

            return result.ToArray();
        }

        private static int CalculateLastFontInsertIndex(in string text, int lastFontCloseIdx)
        {
            var nearestCloseIndex = text.IndexOf("</", lastFontCloseIdx, StringComparison.Ordinal);
            return nearestCloseIndex < 0 ? text.Length : nearestCloseIndex;
        }
    }
}