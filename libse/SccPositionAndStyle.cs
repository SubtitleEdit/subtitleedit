using System.Drawing;

namespace Nikse.SubtitleEdit.Core
{
    public class SccPositionAndStyle
    {
        public Color ForeColor { get; set; }
        public FontStyle Style { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Code { get; set; }

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