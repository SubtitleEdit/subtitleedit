
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class SccPositionAndStyle
    {
        public SKColor ForeColor { get; }
        public SccFontStyle Style { get; }
        public int X { get; }
        public int Y { get; }
        public string Code { get; }

        public SccPositionAndStyle(SKColor color, SccFontStyle style, int y, int x, string code)
        {
            ForeColor = color;
            Style = style;
            X = x;
            Y = y;
            Code = code;
        }
    }
}