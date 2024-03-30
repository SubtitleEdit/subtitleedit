using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public class TypeWriterEffect : TextEffectBase
    {
        public override string[] Transform(string text)
        {
            // todo: fix this!
            var closingTags = new List<string>();
            var list = new List<string>();
            var sb = new StringBuilder();
            var sbClosing = new StringBuilder();
            
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '<' || ch == '{')
                {
                    var closingIdx = text.IndexOf(HtmlUtil.GetClosingPair(ch), i + 1);
                    // invalid tag (no closing)
                    if (closingIdx < 0)
                    {
                        sb.Append(ch); // < or {
                    }
                    else
                    {
                        var tag = text.Substring(i, closingIdx - i + 1);
                        // ass tag are always included, this logic is for html formatting tags
                        if (ch == '<')
                        {
                            if (HtmlUtil.IsOpenTag(tag))
                            {
                                closingTags.Add(HtmlUtil.GetClosingPair(tag));
                            }
                            else
                            {
                                closingTags.Remove(tag);
                            }
                        }

                        sb.Append(tag);
                        i = closingIdx;
                    }
                }
                else // anything else
                {
                    sb.Append(ch);

                    // always have char like white space to be followed by a visible character
                    if (IsVisibleChar(ch))
                    {
                        sbClosing.Clear();
                        // write closing for all open tag
                        for (var j = closingTags.Count - 1; j >= 0; j--)
                        {
                            sbClosing.Append(closingTags[j]);
                        }

                        list.Add(sb + sbClosing.ToString());
                    }
                }
            }

            return list.ToArray();
        }

        private readonly string _fontClose = "</font>";
        protected string GetTextWithClosedFontTag(in string text, int index) => text.Substring(0, index + 1) + _fontClose + text.Substring(index + 1);
    }
}