using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Represents a coordinate point in an ASSA drawing shape.
/// </summary>
public class DrawCoordinate
{
    public DrawShape? DrawShape { get; set; }
    public DrawCoordinateType DrawType { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public Color PointColor { get; set; }

    public bool IsBeizer => DrawType == DrawCoordinateType.BezierCurve ||
                            DrawType == DrawCoordinateType.BezierCurveSupport1 ||
                            DrawType == DrawCoordinateType.BezierCurveSupport2;

    public DrawCoordinate(DrawShape? drawShape, DrawCoordinateType drawType, float x, float y, Color pointColor)
    {
        DrawShape = drawShape;
        DrawType = drawType;
        X = x;
        Y = y;
        PointColor = pointColor;
    }

    public string GetText(float x, float y)
    {
        return DrawType switch
        {
            DrawCoordinateType.Move => $"Move to {x:0.#} {y:0.#}",
            DrawCoordinateType.Line => $"Line to {x:0.#} {y:0.#}",
            DrawCoordinateType.BezierCurve => $"Bezier to {x:0.#} {y:0.#}",
            DrawCoordinateType.BezierCurveSupport1 => $"  Control 1: {x:0.#} {y:0.#}",
            DrawCoordinateType.BezierCurveSupport2 => $"  Control 2: {x:0.#} {y:0.#}",
            _ => $"{x:0.#} {y:0.#}"
        };
    }

    public DrawCoordinate Clone()
    {
        return new DrawCoordinate(DrawShape, DrawType, X, Y, PointColor);
    }
}
