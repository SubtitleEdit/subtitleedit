using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Represents a single shape in an ASSA drawing consisting of multiple coordinates.
/// </summary>
public class DrawShape
{
    public List<DrawCoordinate> Points { get; set; } = [];
    public Color ForeColor { get; set; } = Colors.White;
    public Color OutlineColor { get; set; } = Colors.Black;
    public int OutlineWidth { get; set; }
    public int Layer { get; set; }
    public bool IsEraser { get; set; }
    public bool Hidden { get; set; }
    public bool Expanded { get; set; } = true;

    public DrawShape()
    {
    }

    public DrawShape(DrawShape other)
    {
        ForeColor = other.ForeColor;
        OutlineColor = other.OutlineColor;
        OutlineWidth = other.OutlineWidth;
        Layer = other.Layer;
        IsEraser = other.IsEraser;
        Hidden = other.Hidden;
        Expanded = other.Expanded;

        foreach (var point in other.Points)
        {
            var newPoint = point.Clone();
            newPoint.DrawShape = this;
            Points.Add(newPoint);
        }
    }

    public void AddPoint(DrawCoordinateType drawType, float x, float y, Color pointColor)
    {
        Points.Add(new DrawCoordinate(this, drawType, x, y, pointColor));
    }

    /// <summary>
    /// Converts the shape to ASSA drawing commands.
    /// </summary>
    public string ToAssa()
    {
        if (Points.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var first = Points[0];

        // Start with move command using the first point
        sb.Append(CultureInfo.InvariantCulture, $"m {first.X:0.##} {first.Y:0.##} ");

        // Determine if this is a bezier shape by checking if first point or any following points are bezier type
        var isBezierShape = Points.Any(p => p.DrawType == DrawCoordinateType.BezierCurve || 
                                             p.DrawType == DrawCoordinateType.BezierCurveSupport1 || 
                                             p.DrawType == DrawCoordinateType.BezierCurveSupport2);

        if (isBezierShape)
        {
            // Bezier shape: output as "b x1 y1 x2 y2 x3 y3 ..."
            // The coordinates after index 0 should be in triplets: (Support1, Support2, BezierCurve)
            sb.Append("b ");
            
            for (var i = 1; i < Points.Count; i++)
            {
                sb.Append(CultureInfo.InvariantCulture, $"{Points[i].X:0.##} {Points[i].Y:0.##} ");
            }
        }
        else
        {
            // Line shape: output each point with "l" command
            for (var i = 1; i < Points.Count; i++)
            {
                var point = Points[i];
                sb.Append(CultureInfo.InvariantCulture, $"l {point.X:0.##} {point.Y:0.##} ");
            }
        }

        return sb.ToString().Trim();
    }

    public DrawShape Clone()
    {
        return new DrawShape(this);
    }
}
