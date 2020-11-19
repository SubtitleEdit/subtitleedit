using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class SccPositionAndStyle
    {
        public Color ForeColor { get; }
        public FontStyle Style { get; }
        public int X { get; }
        public int Y { get; }
        public string Code { get; }

        public SccPositionAndStyle(Color color, FontStyle style, int y, int x, string code)
        {
            ForeColor = color;
            Style = style;
            X = x;
            Y = y;
            Code = code;
        }
    }
}