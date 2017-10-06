using System;

namespace Nikse.SubtitleEdit.Core.Casing
{
    public class UpperCaseConverter : CaseConverter
    {
        public override void Convert(Subtitle subtitle, CasingContext context)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                string text = p.Text;
                var st = new StrippableText(text);
                text = st.Pre + st.StrippedText.ToUpper(context.Culture) + st.Post;
                text = HtmlUtil.FixUpperTags(text);
                if (!p.Text.Equals(text, StringComparison.Ordinal))
                {
                    p.Text = text;
                    Count++;
                }
            }
        }
    }
}
