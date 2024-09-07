using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public class KaraokeEffect
    {
        private readonly TextEffectBase _splitStrategy;

        public KaraokeEffect(TextEffectBase splitStrategy) => _splitStrategy = splitStrategy;

        public IEnumerable<Paragraph> Transform(Paragraph paragraph, Color color, double delay)
        {
            // remove any coloring tag already in text
            var text = HtmlUtil.RemoveColorTags(paragraph.Text);
            // calculate index where the first coloring will be inserted
            var fontInsertIndex = CalculateColorInsertIndex(text);
            text = text.Insert(fontInsertIndex, GetColor(color));
            var result = _splitStrategy.Transform(text);
            var duration = paragraph.DurationTotalMilliseconds - delay;
            var durationPerSentence = duration / result.Length;
            var baseStartTime = paragraph.StartTime.TotalMilliseconds;

            // the gaps must be 0 to avoid flickering
            const double gapBetweenSentences = 0;

            var animations = new List<Paragraph>();
            for (var i = 0; i < result.Length; i++)
            {
                var startTime = new TimeCode(baseStartTime + i * durationPerSentence);
                var endTime = new TimeCode(baseStartTime + (i + 1) * durationPerSentence - gapBetweenSentences);
                animations.Add(new Paragraph(startTime, endTime, result[i]));
            }

            // fixes precision/fraction lost from division
            if (animations.Count > 0)
            {
                animations[animations.Count - 1].EndTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds;
            }

            return animations;
        }

        private int CalculateColorInsertIndex(in string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return -1;
            }

            var len = text.Length;
            for (var i = 0; i < len; i++)
            {
                var ch = text[i];
                if (HtmlUtil.IsStartTagSymbol(ch))
                {
                    var closeIdx = text.IndexOf(HtmlUtil.GetClosingPair(ch), i + 1);
                    if (closeIdx < 0)
                    {
                        return i;
                    }

                    i = closeIdx;
                }
                else
                {
                    return i;
                }
            }

            return 0;
        }

        private string GetColor(Color color) => $"<font color=\"{Utilities.ColorToHex(color)}\">";
    }
}