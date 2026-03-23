using System;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Helper class to create circular shapes using Bezier curves.
/// </summary>
public static class CircleBezier
{
    // Magic number for approximating a circle with 4 Bezier curves
    // kappa = 4 * (sqrt(2) - 1) / 3 ? 0.5522847498
    private const float Kappa = 0.5522847498f;

    /// <summary>
    /// Creates a circle shape using Bezier curves.
    /// </summary>
    public static DrawShape MakeCircle(float centerX, float centerY, float radius, int layer, Color foreColor)
    {
        var shape = new DrawShape
        {
            ForeColor = foreColor,
            Layer = layer
        };

        // Control point offset
        var offset = radius * Kappa;

        // Start at top of circle
        var topX = centerX;
        var topY = centerY - radius;

        // Add starting point (top)
        shape.AddPoint(DrawCoordinateType.BezierCurve, topX, topY, DrawSettings.PointColor);

        // Top to right
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport1, centerX + offset, centerY - radius, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport2, centerX + radius, centerY - offset, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurve, centerX + radius, centerY, DrawSettings.PointColor);

        // Right to bottom
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport1, centerX + radius, centerY + offset, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport2, centerX + offset, centerY + radius, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurve, centerX, centerY + radius, DrawSettings.PointColor);

        // Bottom to left
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport1, centerX - offset, centerY + radius, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport2, centerX - radius, centerY + offset, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurve, centerX - radius, centerY, DrawSettings.PointColor);

        // Left to top
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport1, centerX - radius, centerY - offset, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurveSupport2, centerX - offset, centerY - radius, DrawSettings.PointHelperColor);
        shape.AddPoint(DrawCoordinateType.BezierCurve, topX, topY, DrawSettings.PointColor);

        return shape;
    }
}
