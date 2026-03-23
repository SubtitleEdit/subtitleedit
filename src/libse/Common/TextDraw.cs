using SkiaSharp;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class TextDraw
    {
        public static double GetFontSize(double fontSize)
        {
            return fontSize * 0.895d; // font rendered in video players like vlc/mpv are a little smaller than .net, so we adjust font size a bit down
        }

        public static void DrawText(SKTypeface typeface, SKPath path, StringBuilder sb, bool isItalic, bool isBold, bool isUnderline, float left, float top, ref bool newLine, float leftMargin, ref int pathPointsStart)
        {
            var next = new SKPoint(left, top);

            if (path.PointCount > 0)
            {
                var k = 0;
                var points = path.Points;
                for (var i = points.Length - 1; i >= 0; i--)
                {
                    if (points[i].X > next.X)
                    {
                        next.X = points[i].X;
                    }

                    k++;
                    if (k > 60 || (i <= pathPointsStart && pathPointsStart != -1))
                    {
                        break;
                    }
                }
            }

            if (newLine)
            {
                next.X = leftMargin;
                newLine = false;
                pathPointsStart = path.PointCount;
            }

            var fontStyle = new SKFontStyle(
                isBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                isItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright
            );

            var actualTypeface = SKTypeface.FromFamilyName(typeface.FamilyName, fontStyle);
            var font = new SKFont(actualTypeface, (float)GetFontSize(20));

            var text = sb.ToString();
            var glyphs = font.GetGlyphs(text);
            var widths = font.GetGlyphWidths(glyphs);

            float x = next.X;

            for (int i = 0; i < glyphs.Length; i++)
            {
                var glyphPath = font.GetGlyphPath(glyphs[i]);
                if (glyphPath != null)
                {
                    var translatedPath = new SKPath();
                    translatedPath.AddPath(glyphPath, x, next.Y);
                    path.AddPath(translatedPath);
                }

                x += widths[i];
            }

            if (isUnderline && glyphs.Length > 0)
            {
                float totalWidth = 0;
                foreach (var w in widths)
                {
                    totalWidth += w;
                }

                var underlineY = (float)(next.Y + font.Metrics.UnderlinePosition);
                path.MoveTo(next.X, underlineY);
                path.LineTo(next.X + totalWidth, underlineY);
            }

            sb.Clear();
        }


        public static float MeasureTextWidth(SKTypeface typeface, float fontSize, string text, bool bold)
        {
            var fontStyle = new SKFontStyle(
                bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright
            );

            var styledTypeface = SKTypeface.FromFamilyName(typeface.FamilyName, fontStyle);
            var font = new SKFont(styledTypeface, fontSize);

            return font.MeasureText(text);
        }

        public static float MeasureTextHeight(SKTypeface typeface, float fontSize, string text, bool bold)
        {
            var fontStyle = new SKFontStyle(
                bold? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright
            );

            var styledTypeface = SKTypeface.FromFamilyName(typeface.FamilyName, fontStyle);
            var font = new SKFont(styledTypeface, fontSize);

            var metrics = font.Metrics;
            return metrics.Descent - metrics.Ascent;
        }
    }
}