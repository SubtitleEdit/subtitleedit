using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// ASSA drawing commands (m/l/b, with or without {\p1} tags) as an Avalonia geometry - for the
/// style gallery thumbnails that are drawn by the same code that writes the ASSA.
/// </summary>
public static class AssaDrawingGeometry
{
    public static Geometry FromDrawing(string drawing)
    {
        var shapes = DrawShape.ParseAssa(drawing, 0, Colors.White, false);
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.SetFillRule(FillRule.NonZero);
        foreach (var shape in shapes.Where(s => s.Points.Count > 2))
        {
            ctx.BeginFigure(new Point(shape.Points[0].X, shape.Points[0].Y), true);
            var i = 1;
            while (i < shape.Points.Count)
            {
                var point = shape.Points[i];
                if (point.DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < shape.Points.Count)
                {
                    ctx.CubicBezierTo(new Point(point.X, point.Y),
                        new Point(shape.Points[i + 1].X, shape.Points[i + 1].Y),
                        new Point(shape.Points[i + 2].X, shape.Points[i + 2].Y));
                    i += 3;
                }
                else
                {
                    ctx.LineTo(new Point(point.X, point.Y));
                    i++;
                }
            }

            ctx.EndFigure(true);
        }

        return geometry;
    }
}
