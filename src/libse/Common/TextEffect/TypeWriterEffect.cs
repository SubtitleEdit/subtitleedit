using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public class TypeWriterEffect : TextEffectBase
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly StringBuilder _sbClosing = new StringBuilder();

        public override string[] Transform(string text)
        {
            var closingTags = new List<string>(text.Length / 3 + 1);
            var list = new List<string>();

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (HtmlUtil.IsStartTagSymbol(ch))
                {
                    var closingIdx = text.IndexOf(HtmlUtil.GetClosingPair(ch), i + 1);
                    // invalid tag (no closing)
                    if (closingIdx < 0)
                    {
                        _sb.Append(ch); // < or {
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

                        _sb.Append(tag);
                        i = closingIdx;
                    }
                }
                else // anything else
                {
                    _sb.Append(ch);

                    // always have char like white space to be followed by a visible character
                    if (IsVisibleChar(ch))
                    {
                        _sbClosing.Clear();
                        // write closing for all open tag
                        for (var j = closingTags.Count - 1; j >= 0; j--)
                        {
                            _sbClosing.Append(closingTags[j]);
                        }

                        list.Add(_sb + _sbClosing.ToString());
                    }
                }
            }

            return list.ToArray();
        }
    }
}