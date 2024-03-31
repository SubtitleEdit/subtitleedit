using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public class KaraokeCharTransform : TypeWriterEffect
    {
        public override string[] Transform(string text)
        {
            var result = new List<string>();
            var len = text.Length;
            for (var i = 0; i < len; i++)
            {
                var ch = text[i];
                // skip already existing tag
                if (HtmlUtil.IsStartTagSymbol(ch))
                {
                    var closingIdx = text.IndexOf(HtmlUtil.GetClosingPair(ch), i + 1);
                    // closing not present, treat it as normal char
                    if (closingIdx < 0)
                    {
                        result.Add(GetTextWithClosedFontTag(text, i));
                    }
                    else
                    {
                        i = closingIdx;
                    }
                }
                else if (IsVisibleChar(ch))
                {
                    result.Add(GetTextWithClosedFontTag(text, i));
                }
            }

            return result.ToArray();
        }
        
        private readonly string _fontClose = "</font>";
        private string GetTextWithClosedFontTag(in string text, int index) => text.Substring(0, index + 1) + _fontClose + text.Substring(index + 1);
    }
}