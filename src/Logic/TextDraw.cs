using System.Drawing;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public static class TextDraw
    {

        public static void DrawText(Font font, StringFormat sf, System.Drawing.Drawing2D.GraphicsPath path, StringBuilder sb, bool isItalic, bool isBold, float left, float top, ref bool newLine, float addX, float leftMargin)
        {
            PointF next = new PointF(left, top);

            if (path.PointCount > 0)
            {
                int k = 0;
                for (int i = path.PathPoints.Length - 1; i >= 0; i--)
                {
                    if (path.PathPoints[i].X > next.X)
                        next.X = path.PathPoints[i].X;
                    k++;
                    if (k > 10)
                        break;
                }
            }

            next.X += addX;
            if (newLine)
            {
                next.X = leftMargin;
                newLine = false;
            }

            var fontStyle = FontStyle.Regular;
            if (isItalic && isBold)
                fontStyle = FontStyle.Italic | FontStyle.Bold;
            else if (isItalic)
                fontStyle = FontStyle.Italic;
            else if (isBold)
                fontStyle = FontStyle.Bold;
            path.AddString(sb.ToString(), font.FontFamily, (int)fontStyle, font.Size, next, sf);

            sb.Length = 0;
        }

    }
}
