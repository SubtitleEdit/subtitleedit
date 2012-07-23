using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public static class TextDraw
    {

        public static void DrawText(Font font, StringFormat sf, System.Drawing.Drawing2D.GraphicsPath path, StringBuilder sb, bool isItalic, bool isBold, bool isUnderline, float left, float top, ref bool newLine, float leftMargin, ref int pathPointsStart)
        {
            var next = new PointF(left, top);

            
            if (path.PointCount > 0)
            {                
                int k = 0;
                for (int i = path.PathPoints.Length - 1; i >= 0; i--)
                {
                    if (path.PathPoints[i].X > next.X)
                        next.X = path.PathPoints[i].X;
                    k++;
                    if (k > 50)
                        break;
                    if (i > pathPointsStart && pathPointsStart != -1)
                        break;
                }
            }

            if (newLine)
            {
                next.X = leftMargin;
                newLine = false;
                pathPointsStart = path.PointCount;
            }

            var fontStyle = FontStyle.Regular;
            if (isItalic && isBold && isUnderline)
                fontStyle = FontStyle.Italic | FontStyle.Bold | FontStyle.Underline;
            else if (isItalic && isBold)
                fontStyle = FontStyle.Italic | FontStyle.Bold;
            else if (isItalic && isUnderline)
                fontStyle = FontStyle.Italic | FontStyle.Underline;
            else if (isUnderline && isBold)
                fontStyle = FontStyle.Underline | FontStyle.Bold;
            else if (isItalic)
                    fontStyle = FontStyle.Italic;
            else if (isBold)
                fontStyle = FontStyle.Bold;
            else if (isUnderline)
                fontStyle = FontStyle.Underline;
            path.AddString(sb.ToString(), font.FontFamily, (int)fontStyle, font.Size, next, sf);

            sb.Length = 0;
        }

        public static float MeasureTextWidth(Font font, string text, bool bold)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
            var path = new GraphicsPath();

            var sb = new StringBuilder(text);
            bool isItalic = false;
            bool newLine = false;
            int leftMargin = 0;
            int pathPointsStart = -1;
            TextDraw.DrawText(font, sf, path, sb, isItalic, bold, false, 0, 0, ref newLine, leftMargin, ref pathPointsStart);

            float width = 0;
            int index = path.PathPoints.Length - 40;
            if (index < 0)
                index = 0;
            for (int i = index; i < path.PathPoints.Length; i++)
            {
                if (path.PathPoints[i].X > width)
                    width = path.PathPoints[i].X;
            }
            int max = 40;
            if (max > path.PathPoints.Length)
                max = path.PathPoints.Length;
            for (int i = 0; i < max; i++)
            {
                if (path.PathPoints[i].X > width)
                    width = path.PathPoints[i].X;
            }

            return width;
        }

        public static float MeasureTextHeight(Font font, string text, bool bold)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
            var path = new GraphicsPath();

            var sb = new StringBuilder(text);
            bool isItalic = false;
            bool newLine = false;
            int leftMargin = 0;
            int pathPointsStart = -1;
            TextDraw.DrawText(font, sf, path, sb, isItalic, bold, false, 0, 0, ref newLine, leftMargin, ref pathPointsStart);

            float height = 0;
            int index = path.PathPoints.Length - 80;
            if (index < 0)
                index = 0;
            for (int i = index; i < path.PathPoints.Length; i++)
            {
                if (path.PathPoints[i].Y > height)
                    height = path.PathPoints[i].Y;
            }
            int max = 80;
            if (max > path.PathPoints.Length)
                max = path.PathPoints.Length;
            for (int i = 0; i < max; i++)
            {
                if (path.PathPoints[i].Y > height)
                    height = path.PathPoints[i].Y;
            }

            return height;
        }


    }
}
