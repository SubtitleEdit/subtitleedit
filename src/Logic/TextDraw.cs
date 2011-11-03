using System.Drawing;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public static class TextDraw
    {

        public static void DrawText(Font font, StringFormat sf, System.Drawing.Drawing2D.GraphicsPath path, StringBuilder sb, bool isItalic, float left, float top, ref bool newLine, float addX, float leftMargin)
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

            if (isItalic)
                path.AddString(sb.ToString(), font.FontFamily, (int)System.Drawing.FontStyle.Italic, font.Size, next, sf);
            else
                path.AddString(sb.ToString(), font.FontFamily, 0, font.Size, next, sf);

            sb.Length = 0;
        }

    }
}
