using System;

namespace Nikse.SubtitleEdit.Core.Casing
{
    public class LowerCaseConverter : CaseConverter
    {
        public override void Convert(Subtitle subtitle, CasingContext context)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                string text = p.Text;
                text = text.ToLower(context.Culture);
                if (!text.Equals(p.Text, StringComparison.Ordinal))
                {
                    p.Text = text;
                    Count++;
                }
            }
        }
    }
}
