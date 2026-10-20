using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    /// <summary>
    /// ASSA \bord. Drawing shapes share their layer's outline (one line per layer); a text box
    /// outlines its own text.
    /// </summary>
    public float OutlineWidth { get; set; }
    public int Layer { get; set; }
    public bool IsEraser { get; set; }
    public bool Hidden { get; set; }
    public bool Expanded { get; set; }

    /// <summary>
    /// Editor-only group (0 = none): a click selects every shape with the same id. Not part of
    /// the ASSA output; an .assadraw file keeps it in the Effect field.
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// The in/out effect of the shape's line (a line is a layer, so every shape of a layer has the
    /// same one); null = not animated.
    /// </summary>
    public DrawAnimation? Animation { get; set; }

    /// <summary>
    /// A text-only object: its rectangle is where the text goes, nothing is drawn for it.
    /// </summary>
    public bool IsTextBox { get; set; }

    /// <summary>
    /// Text written inside the shape (a speech bubble) or the text box; "\n" = line break.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    public string FontName { get; set; } = "Arial";

    /// <summary>
    /// ASSA \fs - with <see cref="ShrinkToFit"/> the largest size, made smaller until the text fits.
    /// </summary>
    public float FontSize { get; set; } = 48;
    public bool FontBold { get; set; }
    public Color TextColor { get; set; } = Colors.Black;
    public bool ShrinkToFit { get; set; } = true;

    /// <summary>
    /// Where the text goes (a bubble's body, not its tail), in canvas coordinates. It is moved,
    /// scaled, rotated and mirrored with the shape. Null = inside the shape's bounds.
    /// </summary>
    public DrawTextFrame? TextFrame { get; set; }

    public bool HasText => !IsEraser && !string.IsNullOrWhiteSpace(Text);

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
        GroupId = other.GroupId;
        Animation = other.Animation?.Clone();
        IsTextBox = other.IsTextBox;
        Text = other.Text;
        FontName = other.FontName;
        FontSize = other.FontSize;
        FontBold = other.FontBold;
        TextColor = other.TextColor;
        ShrinkToFit = other.ShrinkToFit;
        TextFrame = other.TextFrame;

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
    /// Converts the shape to ASSA drawing commands, optionally relative to an anchor (an animated
    /// line is positioned at its anchor with \pos or \move).
    /// </summary>
    public string ToAssa(float originX = 0, float originY = 0)
    {
        if (Points.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var first = Points[0];

        // Start with move command using the first point
        sb.Append(CultureInfo.InvariantCulture, $"m {first.X - originX:0.##} {first.Y - originY:0.##} ");

        // A shape can mix line and bezier segments (the importer appends "l" and "b" runs to
        // the same shape, and switching tools mid-shape does too). Emit each point under the
        // command its type calls for: a "b" run takes triplets (Support1, Support2, BezierCurve)
        // and an "l" run takes single points. Serializing a mixed shape as one "b" run turned
        // the straight points into bezier control points and shifted every later triplet.
        var currentCommand = ' ';
        for (var i = 1; i < Points.Count; i++)
        {
            var point = Points[i];
            var command = point.DrawType is DrawCoordinateType.BezierCurve or DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2
                ? 'b'
                : 'l';
            if (command != currentCommand)
            {
                sb.Append(command).Append(' ');
                currentCommand = command;
            }

            sb.Append(CultureInfo.InvariantCulture, $"{point.X - originX:0.##} {point.Y - originY:0.##} ");
        }

        return sb.ToString().Trim();
    }

    public DrawShape Clone()
    {
        return new DrawShape(this);
    }

    /// <summary>
    /// The outline as a polygon, with bezier segments sampled into short lines.
    /// </summary>
    public List<(float X, float Y)> Flatten()
    {
        var result = new List<(float X, float Y)>(Points.Count);
        if (Points.Count == 0)
        {
            return result;
        }

        result.Add((Points[0].X, Points[0].Y));
        var i = 1;
        while (i < Points.Count)
        {
            var point = Points[i];
            if (point.DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < Points.Count)
            {
                var p0 = Points[i - 1];
                var p1 = point;
                var p2 = Points[i + 1];
                var p3 = Points[i + 2];
                const int steps = 12;
                for (var step = 1; step <= steps; step++)
                {
                    var t = step / (float)steps;
                    var u = 1 - t;
                    var x = u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X;
                    var y = u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y;
                    result.Add((x, y));
                }

                i += 3;
            }
            else
            {
                result.Add((point.X, point.Y));
                i++;
            }
        }

        return result;
    }

    /// <summary>
    /// True when (x, y) is inside the closed shape or within <paramref name="tolerance"/> of its outline.
    /// </summary>
    public bool HitTest(float x, float y, float tolerance)
    {
        var polygon = Flatten();
        if (polygon.Count == 0)
        {
            return false;
        }

        if (polygon.Count == 1)
        {
            return Math.Abs(polygon[0].X - x) <= tolerance && Math.Abs(polygon[0].Y - y) <= tolerance;
        }

        var inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            var a = polygon[i];
            var b = polygon[j];
            if (DistanceToSegment(x, y, a.X, a.Y, b.X, b.Y) <= tolerance)
            {
                return true;
            }

            if ((a.Y > y) != (b.Y > y) && x < (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X)
            {
                inside = !inside;
            }
        }

        return polygon.Count > 2 && inside;
    }

    /// <summary>
    /// Bounding box of all points, bezier control points included.
    /// </summary>
    public (float Left, float Top, float Right, float Bottom) GetBounds()
    {
        if (Points.Count == 0)
        {
            return (0, 0, 0, 0);
        }

        var left = float.MaxValue;
        var top = float.MaxValue;
        var right = float.MinValue;
        var bottom = float.MinValue;
        foreach (var point in Points)
        {
            left = Math.Min(left, point.X);
            top = Math.Min(top, point.Y);
            right = Math.Max(right, point.X);
            bottom = Math.Max(bottom, point.Y);
        }

        return (left, top, right, bottom);
    }

    /// <summary>
    /// Bounding box around several shapes (shapes without points are skipped).
    /// </summary>
    public static (float Left, float Top, float Right, float Bottom) GetBounds(IEnumerable<DrawShape> shapes)
    {
        (float Left, float Top, float Right, float Bottom)? bounds = null;
        foreach (var shape in shapes)
        {
            if (shape.Points.Count == 0)
            {
                continue;
            }

            var b = shape.GetBounds();
            bounds = bounds is { } u
                ? (Math.Min(u.Left, b.Left), Math.Min(u.Top, b.Top), Math.Max(u.Right, b.Right), Math.Max(u.Bottom, b.Bottom))
                : b;
        }

        return bounds ?? (0, 0, 0, 0);
    }

    /// <summary>
    /// The text area: the one set, or else the middle 80% of the bounds.
    /// </summary>
    public DrawTextFrame GetTextFrame()
    {
        if (TextFrame is { } frame)
        {
            return frame;
        }

        var (left, top, right, bottom) = GetBounds();
        return new DrawTextFrame((left + right) / 2, (top + bottom) / 2, (right - left) * 0.8f, (bottom - top) * 0.8f, 0);
    }

    /// <summary>
    /// The text area before rotation (rotated by <see cref="DrawTextFrame.Angle"/> around its center).
    /// </summary>
    public (float Left, float Top, float Right, float Bottom) GetTextBounds()
    {
        var frame = GetTextFrame();
        return (frame.CenterX - frame.Width / 2, frame.CenterY - frame.Height / 2, frame.CenterX + frame.Width / 2, frame.CenterY + frame.Height / 2);
    }

    public void SetTextBounds(float left, float top, float right, float bottom, float angle = 0)
    {
        TextFrame = new DrawTextFrame((left + right) / 2, (top + bottom) / 2, Math.Abs(right - left), Math.Abs(bottom - top), angle);
    }

    /// <summary>
    /// Moves the text area along with a transform of the points. A mirror keeps the text readable:
    /// of the two possible directions the one closest to the old angle is used.
    /// </summary>
    public void TransformText(Func<float, float, (float X, float Y)> map, bool mirrored)
    {
        if (TextFrame is not { } frame)
        {
            return;
        }

        var radians = frame.Angle * MathF.PI / 180f;
        var ux = MathF.Cos(radians) * frame.Width / 2;
        var uy = MathF.Sin(radians) * frame.Width / 2;
        var vx = -MathF.Sin(radians) * frame.Height / 2;
        var vy = MathF.Cos(radians) * frame.Height / 2;
        var center = map(frame.CenterX, frame.CenterY);
        var u = map(frame.CenterX + ux, frame.CenterY + uy);
        var v = map(frame.CenterX + vx, frame.CenterY + vy);
        var newUx = u.X - center.X;
        var newUy = u.Y - center.Y;
        var newVx = v.X - center.X;
        var newVy = v.Y - center.Y;

        var angle = frame.Width > 0.001f ? MathF.Atan2(newUy, newUx) * 180f / MathF.PI : frame.Angle;
        if (mirrored && Math.Abs(NormalizeAngle(angle + 180 - frame.Angle)) < Math.Abs(NormalizeAngle(angle - frame.Angle)))
        {
            angle += 180;
        }

        TextFrame = new DrawTextFrame(center.X, center.Y,
            2 * MathF.Sqrt(newUx * newUx + newUy * newUy),
            2 * MathF.Sqrt(newVx * newVx + newVy * newVy),
            MathF.Round(NormalizeAngle(angle), 2));
    }

    private static float NormalizeAngle(float degrees)
    {
        degrees %= 360;
        if (degrees > 180)
        {
            degrees -= 360;
        }
        else if (degrees <= -180)
        {
            degrees += 360;
        }

        return degrees;
    }

    public void Offset(float x, float y)
    {
        foreach (var point in Points)
        {
            point.X += x;
            point.Y += y;
        }

        if (TextFrame is { } frame)
        {
            TextFrame = frame with { CenterX = frame.CenterX + x, CenterY = frame.CenterY + y };
        }
    }

    /// <summary>
    /// Rotates around the center of the bounding box; positive is clockwise on screen (y points down).
    /// </summary>
    public void Rotate(float degrees)
    {
        var (left, top, right, bottom) = GetBounds();
        Rotate(degrees, (left + right) / 2f, (top + bottom) / 2f);
    }

    /// <summary>
    /// Rotates around a given center, e.g. the middle of a multi-selection.
    /// </summary>
    public void Rotate(float degrees, float centerX, float centerY)
    {
        var radians = degrees * MathF.PI / 180f;
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        foreach (var point in Points)
        {
            var x = point.X - centerX;
            var y = point.Y - centerY;
            point.X = MathF.Round(centerX + x * cos - y * sin, 2);
            point.Y = MathF.Round(centerY + x * sin + y * cos, 2);
        }

        TransformText((x, y) => (centerX + (x - centerX) * cos - (y - centerY) * sin, centerY + (x - centerX) * sin + (y - centerY) * cos), mirrored: false);
    }

    /// <summary>
    /// Mirrors the shape and reverses its point order, so the winding (and with the non-zero
    /// fill rule, which contours are holes) stays the same.
    /// </summary>
    public void FlipHorizontal()
    {
        var (left, _, right, _) = GetBounds();
        FlipHorizontal(left, right);
    }

    /// <summary>
    /// Mirrors inside the given horizontal range, e.g. the bounds of a multi-selection.
    /// </summary>
    public void FlipHorizontal(float left, float right)
    {
        foreach (var point in Points)
        {
            point.X = left + right - point.X;
        }

        TransformText((x, y) => (left + right - x, y), mirrored: true);
        ReverseWinding();
    }

    public void FlipVertical()
    {
        var (_, top, _, bottom) = GetBounds();
        FlipVertical(top, bottom);
    }

    /// <summary>
    /// Mirrors inside the given vertical range, e.g. the bounds of a multi-selection.
    /// </summary>
    public void FlipVertical(float top, float bottom)
    {
        foreach (var point in Points)
        {
            point.Y = top + bottom - point.Y;
        }

        TransformText((x, y) => (x, top + bottom - y), mirrored: true);
        ReverseWinding();
    }

    /// <summary>
    /// Walks the outline the other way round: same segments, opposite direction. The coordinates
    /// simply reverse order; each curve's control points swap roles (Support1 ↔ Support2).
    /// </summary>
    public void ReverseWinding()
    {
        var count = Points.Count;
        if (count < 2)
        {
            return;
        }

        var newTypes = new DrawCoordinateType[count];
        newTypes[count - 1] = Points[0].DrawType;
        var i = 1;
        while (i < count)
        {
            if (Points[i].DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < count &&
                Points[i + 1].DrawType == DrawCoordinateType.BezierCurveSupport2)
            {
                newTypes[i + 1] = DrawCoordinateType.BezierCurveSupport1;
                newTypes[i] = DrawCoordinateType.BezierCurveSupport2;
                newTypes[i - 1] = DrawCoordinateType.BezierCurve;
                i += 3;
            }
            else
            {
                newTypes[i - 1] = DrawCoordinateType.Line;
                i++;
            }
        }

        for (var j = 0; j < count; j++)
        {
            Points[j].DrawType = newTypes[j];
        }

        Points.Reverse();
    }

    /// <summary>
    /// Removes a point. A bezier end point takes its two control points with it, and removing the
    /// first point of a bezier segment drops that segment's control points so the next end point
    /// becomes the start. Control points can't be removed on their own.
    /// </summary>
    public bool RemovePoint(DrawCoordinate point)
    {
        var index = Points.IndexOf(point);
        if (index < 0 || point.DrawType is DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2)
        {
            return false;
        }

        if (index == 0)
        {
            Points.RemoveAt(0);
            if (Points.Count >= 3 && Points[0].DrawType == DrawCoordinateType.BezierCurveSupport1)
            {
                Points.RemoveRange(0, 2);
            }

            return true;
        }

        if (point.DrawType == DrawCoordinateType.BezierCurve && index >= 2 &&
            Points[index - 1].DrawType == DrawCoordinateType.BezierCurveSupport2 &&
            Points[index - 2].DrawType == DrawCoordinateType.BezierCurveSupport1)
        {
            Points.RemoveRange(index - 2, 3);
            return true;
        }

        Points.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// The end point of the segment a point belongs to: control points map to their curve's end point.
    /// </summary>
    public DrawCoordinate? GetSegmentEnd(DrawCoordinate point)
    {
        var index = Points.IndexOf(point);
        if (index < 0)
        {
            return null;
        }

        return point.DrawType switch
        {
            DrawCoordinateType.BezierCurveSupport1 when index + 2 < Points.Count => Points[index + 2],
            DrawCoordinateType.BezierCurveSupport2 when index + 1 < Points.Count => Points[index + 1],
            DrawCoordinateType.BezierCurveSupport1 or DrawCoordinateType.BezierCurveSupport2 => null,
            _ => point,
        };
    }

    /// <summary>
    /// True when the segment ending at <paramref name="point"/> is a straight line that can become a curve.
    /// The first point has no segment of its own (the closing line is implicit in ASSA).
    /// </summary>
    public bool IsLineSegment(DrawCoordinate point)
    {
        var end = GetSegmentEnd(point);
        return end != null && Points.IndexOf(end) > 0 && end.DrawType == DrawCoordinateType.Line;
    }

    public bool IsCurveSegment(DrawCoordinate point)
    {
        var end = GetSegmentEnd(point);
        var index = end == null ? -1 : Points.IndexOf(end);
        return index >= 3 && end!.DrawType == DrawCoordinateType.BezierCurve &&
               Points[index - 1].DrawType == DrawCoordinateType.BezierCurveSupport2 &&
               Points[index - 2].DrawType == DrawCoordinateType.BezierCurveSupport1;
    }

    /// <summary>
    /// Turns the straight segment ending at the point into a bezier curve with control points at
    /// one and two thirds, so it looks the same until a control point is dragged.
    /// </summary>
    public bool ConvertSegmentToCurve(DrawCoordinate point)
    {
        if (!IsLineSegment(point))
        {
            return false;
        }

        var end = GetSegmentEnd(point)!;
        var index = Points.IndexOf(end);
        var start = Points[index - 1];
        var dx = (end.X - start.X) / 3f;
        var dy = (end.Y - start.Y) / 3f;
        end.DrawType = DrawCoordinateType.BezierCurve;
        Points.Insert(index, new DrawCoordinate(this, DrawCoordinateType.BezierCurveSupport2, start.X + dx * 2, start.Y + dy * 2, DrawSettings.PointHelperColor));
        Points.Insert(index, new DrawCoordinate(this, DrawCoordinateType.BezierCurveSupport1, start.X + dx, start.Y + dy, DrawSettings.PointHelperColor));
        return true;
    }

    /// <summary>
    /// Turns the curve the point belongs to into a straight line (its control points are removed).
    /// </summary>
    public bool ConvertSegmentToLine(DrawCoordinate point)
    {
        if (!IsCurveSegment(point))
        {
            return false;
        }

        var end = GetSegmentEnd(point)!;
        var index = Points.IndexOf(end);
        Points.RemoveRange(index - 2, 2);
        end.DrawType = DrawCoordinateType.Line;
        return true;
    }

    public int ConvertAllLinesToCurves()
    {
        var count = 0;
        foreach (var point in Points.Where(IsLineSegment).ToList())
        {
            if (ConvertSegmentToCurve(point))
            {
                count++;
            }
        }

        return count;
    }

    public int ConvertAllCurvesToLines()
    {
        var count = 0;
        foreach (var point in Points.Where(p => p.DrawType == DrawCoordinateType.BezierCurve && IsCurveSegment(p)).ToList())
        {
            if (ConvertSegmentToLine(point))
            {
                count++;
            }
        }

        return count;
    }

    private static readonly Regex RegexDrawStart = new(@"\{[^{]*\\p1[^}]*\}", RegexOptions.Compiled);
    private static readonly Regex RegexDrawEnd = new(@"\{[^{]*\\p0[^}]*\}", RegexOptions.Compiled);

    /// <summary>
    /// Parses ASSA drawing commands (m/l/b, several "m" = several shapes) into shapes.
    /// </summary>
    public static List<DrawShape> ParseAssa(string text, int layer, Color color, bool isEraser)
    {
        var shapes = new List<DrawShape>();
        text = RegexDrawStart.Replace(text, string.Empty);
        text = RegexDrawEnd.Replace(text, string.Empty);
        var arr = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        var i = 0;
        var bezierCount = 0;
        var state = DrawCoordinateType.None;
        DrawCoordinate? moveCoordinate = null;
        DrawShape? drawShape = null;

        while (i < arr.Length)
        {
            var v = arr[i];

            if (v == "m" && i < arr.Length - 2 &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var mX) &&
                float.TryParse(arr[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out var mY))
            {
                bezierCount = 0;
                moveCoordinate = new DrawCoordinate(null, DrawCoordinateType.Move, mX, mY, DrawSettings.PointColor);
                state = DrawCoordinateType.Move;
                i += 2;
            }
            else if (v == "l")
            {
                state = DrawCoordinateType.Line;
                bezierCount = 0;
                if (moveCoordinate != null)
                {
                    drawShape = new DrawShape { Layer = layer, ForeColor = color, IsEraser = isEraser };
                    drawShape.AddPoint(DrawCoordinateType.Line, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                    moveCoordinate = null;
                    shapes.Add(drawShape);
                }
            }
            else if (v == "b")
            {
                state = DrawCoordinateType.BezierCurve;
                if (moveCoordinate != null)
                {
                    drawShape = new DrawShape { Layer = layer, ForeColor = color, IsEraser = isEraser };
                    drawShape.AddPoint(DrawCoordinateType.BezierCurve, moveCoordinate.X, moveCoordinate.Y, DrawSettings.PointColor);
                    moveCoordinate = null;
                    shapes.Add(drawShape);
                }
                bezierCount = 1;
            }
            else if (state == DrawCoordinateType.Line && drawShape != null && i < arr.Length - 1 &&
                float.TryParse(arr[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var lX) &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lY))
            {
                drawShape.AddPoint(DrawCoordinateType.Line, lX, lY, DrawSettings.PointColor);
                i++;
            }
            else if (state == DrawCoordinateType.BezierCurve && drawShape != null && i < arr.Length - 1 &&
                float.TryParse(arr[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var bX) &&
                float.TryParse(arr[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var bY))
            {
                bezierCount++;
                if (bezierCount > 3)
                {
                    bezierCount = 1;
                }

                var pointType = bezierCount switch
                {
                    2 => DrawCoordinateType.BezierCurveSupport1,
                    3 => DrawCoordinateType.BezierCurveSupport2,
                    _ => DrawCoordinateType.BezierCurve
                };

                var pointColor = bezierCount is 2 or 3 ? DrawSettings.PointHelperColor : DrawSettings.PointColor;
                drawShape.AddPoint(pointType, bX, bY, pointColor);
                i++;
            }

            i++;
        }

        return shapes;
    }

    private static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var lengthSquared = dx * dx + dy * dy;
        var t = lengthSquared > 0 ? Math.Clamp(((px - ax) * dx + (py - ay) * dy) / lengthSquared, 0, 1) : 0;
        var cx = ax + t * dx - px;
        var cy = ay + t * dy - py;
        return MathF.Sqrt(cx * cx + cy * cy);
    }
}

/// <summary>
/// A text area: center and size in canvas coordinates, turned <see cref="Angle"/> degrees
/// clockwise (on screen) around the center.
/// </summary>
public readonly record struct DrawTextFrame(float CenterX, float CenterY, float Width, float Height, float Angle);
