using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public static class TextDraw
    {
        public static void DrawText(Font font, StringFormat sf, GraphicsPath path, StringBuilder sb, bool isItalic, bool isBold, bool isUnderline, float left, float top, ref bool newLine, float leftMargin, ref int pathPointsStart)
        {
            var next = new PointF(left, top);

            if (path.PointCount > 0)
            {
                int k = 0;
                var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                for (int i = list.Length - 1; i >= 0; i--)
                {
                    if (list[i].X > next.X)
                        next.X = list[i].X;
                    k++;
                    if ((k > 60) || (i <= pathPointsStart && pathPointsStart != -1))
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
            if (isItalic)
                fontStyle |= FontStyle.Italic;
            if (isBold)
                fontStyle |= FontStyle.Bold;
            if (isUnderline)
                fontStyle |= FontStyle.Underline;

            try
            {
                path.AddString(sb.ToString(), font.FontFamily, (int)fontStyle, font.Size, next, sf);
            }
            catch
            {
                fontStyle = FontStyle.Regular;
                try
                {
                    path.AddString(sb.ToString(), font.FontFamily, (int)fontStyle, font.Size, next, sf);
                }
                catch
                {
                    path.AddString(sb.ToString(), new FontFamily("Arial"), (int)fontStyle, font.Size, next, sf);
                }
            }
            sb.Length = 0;
        }

        public static float MeasureTextWidth(Font font, string text, bool bold)
        {
            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near })
            {
                using (var path = new GraphicsPath())
                {
                    var sb = new StringBuilder(text);
                    bool newLine = false;
                    const int leftMargin = 0;
                    int pathPointsStart = -1;
                    DrawText(font, sf, path, sb, false, bold, false, 0, 0, ref newLine, leftMargin, ref pathPointsStart);
                    if (path.PathData.Points.Length == 0)
                    {
                        return 0;
                    }

                    float width = 0;
                    var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                    int index = System.Math.Max(list.Length - 42, 0);
                    for (int i = index; i < list.Length; i += 2)
                    {
                        if (list[i].X > width)
                            width = list[i].X;
                    }
                    int max = System.Math.Min(52, list.Length);
                    for (int i = 0; i < max; i += 2)
                    {
                        if (list[i].X > width)
                            width = list[i].X;
                    }

                    return width;
                }
            }
        }

        public static float MeasureTextHeight(Font font, string text, bool bold)
        {
            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near })
            {
                using (var path = new GraphicsPath())
                {
                    var sb = new StringBuilder(text);
                    bool newLine = false;
                    const int leftMargin = 0;
                    int pathPointsStart = -1;
                    DrawText(font, sf, path, sb, false, bold, false, 0, 0, ref newLine, leftMargin, ref pathPointsStart);

                    float height = 0;
                    var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                    int index = System.Math.Max(list.Length - 80, 0);
                    for (int i = index; i < list.Length; i += 2)
                    {
                        if (list[i].Y > height)
                            height = list[i].Y;
                    }

                    for (int i = 0; i < list.Length; i += 2)
                    {
                        if (list[i].Y > height)
                            height = list[i].Y;
                    }

                    return height;
                }
            }
        }

    }
}