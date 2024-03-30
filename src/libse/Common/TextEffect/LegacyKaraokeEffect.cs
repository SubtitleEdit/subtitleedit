using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public class LegacyKaraokeEffect : ITextKaraokeEffect
    {
        public IEnumerable<Paragraph> Transform(Paragraph paragraph, Color color, double delay)
        {
            var result = new List<Paragraph>();
            var duration = paragraph.DurationTotalMilliseconds - delay;
            var partsBase = EffectAnimationPart.MakeBase(paragraph.Text);
            var stepsLength = duration / partsBase.Count + 1;
            for (var index = 0; index <= partsBase.Count; index++)
            {
                var list = EffectAnimationPart.MakeEffectKaraoke(partsBase, color, index);
                var text = EffectAnimationPart.ToString(list);
                var startMilliseconds = index * stepsLength;
                startMilliseconds += paragraph.StartTime.TotalMilliseconds;
                var endMilliseconds = ((index + 1) * stepsLength) - 1;
                endMilliseconds += paragraph.StartTime.TotalMilliseconds;
                var start = new TimeCode(startMilliseconds);
                var end = new TimeCode(endMilliseconds);
                result.Add(new Paragraph(start, end, text));
            }

            return result;
        }
    }
}