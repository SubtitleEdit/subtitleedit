using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common.TextEffect
{
    public interface ITextKaraokeEffect
    {
        IEnumerable<Paragraph> Transform(Paragraph paragraph, Color color, double delay);
    }
}