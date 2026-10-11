using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// A shape that can be placed with the shape tool: a built-in (speech bubbles, arrows, ...) or
/// one the user saved. Built-ins have no color of their own and take the current layer color.
/// </summary>
public sealed class ShapeLibraryItem
{
    private List<DrawShape>? _template;
    private readonly Func<List<DrawShape>> _build;

    public string Name { get; }
    public string Category { get; }

    /// <summary>
    /// Saved by the user (can be removed); its parts keep their own colors.
    /// </summary>
    public SeAssaDrawShape? UserShape { get; }

    public ShapeLibraryItem(string name, string category, Func<List<DrawShape>> build, SeAssaDrawShape? userShape = null)
    {
        Name = name;
        Category = category;
        _build = build;
        UserShape = userShape;
    }

    /// <summary>
    /// The shapes in their own coordinates; Layer is the part index, ForeColor the part color.
    /// </summary>
    public List<DrawShape> Template => _template ??= _build();

    public (float Left, float Top, float Right, float Bottom) Bounds
    {
        get
        {
            var shapes = Template.Where(s => s.Points.Count > 0).ToList();
            if (shapes.Count == 0)
            {
                return (0, 0, 1, 1);
            }

            var bounds = shapes[0].GetBounds();
            foreach (var shape in shapes.Skip(1))
            {
                var b = shape.GetBounds();
                bounds = (Math.Min(bounds.Left, b.Left), Math.Min(bounds.Top, b.Top), Math.Max(bounds.Right, b.Right), Math.Max(bounds.Bottom, b.Bottom));
            }

            return bounds;
        }
    }

    /// <summary>
    /// Height / width of the shape, for placing it with its own proportions.
    /// </summary>
    public float AspectRatio
    {
        get
        {
            var (left, top, right, bottom) = Bounds;
            return right - left > 0.01f ? (bottom - top) / (right - left) : 1f;
        }
    }

    /// <summary>
    /// Copies of the shape scaled into the rectangle. Built-ins get <paramref name="color"/> on
    /// <paramref name="firstLayer"/>; a saved shape gets one layer per part, with its own colors.
    /// </summary>
    public List<DrawShape> CreateShapes(float x, float y, float width, float height, Color color, int firstLayer)
    {
        var (left, top, right, bottom) = Bounds;
        var scaleX = right - left > 0.01f ? width / (right - left) : 1f;
        var scaleY = bottom - top > 0.01f ? height / (bottom - top) : 1f;
        var result = new List<DrawShape>();
        foreach (var template in Template)
        {
            var shape = template.Clone();
            shape.Layer = firstLayer + template.Layer;
            shape.ForeColor = UserShape != null ? template.ForeColor : color;
            shape.Hidden = false;
            foreach (var point in shape.Points)
            {
                point.X = MathF.Round(x + (point.X - left) * scaleX, 2);
                point.Y = MathF.Round(y + (point.Y - top) * scaleY, 2);
            }

            result.Add(shape);
        }

        return result;
    }

    /// <summary>
    /// Thumbnail geometry in the shape's own coordinates (non-zero fill keeps holes open).
    /// </summary>
    public Geometry ToGeometry()
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.SetFillRule(FillRule.NonZero);
        foreach (var shape in Template.Where(s => s.Points.Count > 2 && !s.IsEraser))
        {
            ctx.BeginFigure(new Avalonia.Point(shape.Points[0].X, shape.Points[0].Y), true);
            var i = 1;
            while (i < shape.Points.Count)
            {
                var point = shape.Points[i];
                if (point.DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < shape.Points.Count)
                {
                    ctx.CubicBezierTo(
                        new Avalonia.Point(point.X, point.Y),
                        new Avalonia.Point(shape.Points[i + 1].X, shape.Points[i + 1].Y),
                        new Avalonia.Point(shape.Points[i + 2].X, shape.Points[i + 2].Y));
                    i += 3;
                }
                else
                {
                    ctx.LineTo(new Avalonia.Point(point.X, point.Y));
                    i++;
                }
            }

            ctx.EndFigure(true);
        }

        return geometry;
    }
}

/// <summary>
/// The built-in shapes (written as SVG path data in a 100 x 100 box) and the user's saved shapes.
/// </summary>
public static class ShapeLibrary
{
    private static List<ShapeLibraryItem>? _builtIn;

    public static IReadOnlyList<ShapeLibraryItem> BuiltIn => _builtIn ??= MakeBuiltIn();

    public static IReadOnlyList<string> BuiltInCategories =>
    [
        Se.Language.Assa.DrawCategorySpeechBubbles,
        Se.Language.Assa.DrawCategoryArrows,
        Se.Language.Assa.DrawCategoryBasicShapes,
        Se.Language.Assa.DrawCategorySymbols,
    ];

    public static List<ShapeLibraryItem> GetUserShapes()
    {
        return Se.Settings.Assa.DrawShapeLibrary
            .Select(saved => new ShapeLibraryItem(saved.Name, Se.Language.Assa.DrawCategoryMyShapes, () => ParseSaved(saved), saved))
            .ToList();
    }

    /// <summary>
    /// Saves shapes as a library item: one part per layer (the layer's color), erasers kept as erasers.
    /// </summary>
    public static SeAssaDrawShape Save(string name, IEnumerable<DrawShape> shapes)
    {
        var saved = new SeAssaDrawShape { Name = name };
        foreach (var group in shapes.Where(s => s.Points.Count > 0).GroupBy(s => (s.Layer, s.IsEraser)).OrderBy(g => g.Key.Layer).ThenBy(g => g.Key.IsEraser))
        {
            saved.Parts.Add(new SeAssaDrawShapePart
            {
                Drawing = string.Join(" ", group.Select(s => s.ToAssa())),
                Color = group.First().ForeColor.ToString(),
                IsEraser = group.Key.IsEraser,
            });
        }

        Se.Settings.Assa.DrawShapeLibrary.Add(saved);
        return saved;
    }

    public static void Remove(SeAssaDrawShape saved)
    {
        Se.Settings.Assa.DrawShapeLibrary.Remove(saved);
    }

    private static List<DrawShape> ParseSaved(SeAssaDrawShape saved)
    {
        var result = new List<DrawShape>();
        var layer = 0;
        int? previousLayerKey = null;
        foreach (var part in saved.Parts)
        {
            var color = Color.TryParse(part.Color, out var parsed) ? parsed : Colors.White;

            // An eraser part belongs to the layer of the draw part before it
            if (!part.IsEraser && previousLayerKey != null)
            {
                layer++;
            }

            previousLayerKey = layer;
            result.AddRange(DrawShape.ParseAssa(part.Drawing, layer, color, part.IsEraser));
        }

        return result;
    }

    private static ShapeLibraryItem Item(string name, string category, string[] union, string[]? subtract = null)
    {
        return new ShapeLibraryItem(name, category, () => SvgImporter.FromPathData(union, subtract));
    }

    private static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Circle(double cx, double cy, double r) =>
        $"M{F(cx - r)} {F(cy)} A{F(r)} {F(r)} 0 1 0 {F(cx + r)} {F(cy)} A{F(r)} {F(r)} 0 1 0 {F(cx - r)} {F(cy)} Z";

    /// <summary>
    /// Closed polygon around (cx, cy) with the radii repeated around the circle (two radii = a star).
    /// </summary>
    private static string Polygon(double cx, double cy, int corners, double startDegrees, params double[] radii)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < corners; i++)
        {
            var angle = (startDegrees + 360.0 * i / corners) * Math.PI / 180;
            var r = radii[i % radii.Length];
            sb.Append(i == 0 ? "M" : " L").Append(F(cx + r * Math.Cos(angle))).Append(' ').Append(F(cy + r * Math.Sin(angle)));
        }

        return sb.Append(" Z").ToString();
    }

    private static List<ShapeLibraryItem> MakeBuiltIn()
    {
        var l = Se.Language.Assa;
        var bubbles = l.DrawCategorySpeechBubbles;
        var arrows = l.DrawCategoryArrows;
        var basic = l.DrawCategoryBasicShapes;
        var symbols = l.DrawCategorySymbols;

        return
        [
            // Speech bubbles
            Item(l.DrawShapeSpeechBubble, bubbles, ["M12 0 H88 A12 12 0 0 1 100 12 V58 A12 12 0 0 1 88 70 H38 L14 94 L22 70 H12 A12 12 0 0 1 0 58 V12 A12 12 0 0 1 12 0 Z"]),
            Item(l.DrawShapeSpeechBubbleRight, bubbles, ["M12 0 H88 A12 12 0 0 1 100 12 V58 A12 12 0 0 1 88 70 H78 L86 94 L62 70 H12 A12 12 0 0 1 0 58 V12 A12 12 0 0 1 12 0 Z"]),
            Item(l.DrawShapeSpeechBubbleUp, bubbles, ["M12 24 H22 L30 0 L46 24 H88 A12 12 0 0 1 100 36 V82 A12 12 0 0 1 88 94 H12 A12 12 0 0 1 0 82 V36 A12 12 0 0 1 12 24 Z"]),
            Item(l.DrawShapeOvalBubble, bubbles, ["M0 38 A50 38 0 1 1 100 38 A50 38 0 1 1 0 38 Z", "M22 62 L8 96 L44 72 Z"]),
            Item(l.DrawShapeBoxBubble, bubbles, ["M0 0 H100 V70 H60 L50 94 L40 70 H0 Z"]),
            Item(l.DrawShapeThoughtBubble, bubbles,
            [
                Circle(26, 34, 20), Circle(46, 22, 22), Circle(68, 24, 20), Circle(84, 40, 16),
                Circle(70, 56, 18), Circle(46, 58, 20), Circle(22, 54, 17), Circle(50, 40, 26),
                Circle(18, 82, 7), Circle(8, 95, 4),
            ]),
            Item(l.DrawShapeShoutBubble, bubbles, [Polygon(50, 50, 24, -90, 50, 36, 46, 34, 50, 38, 44, 35)]),
            Item(l.DrawShapeCaptionBox, bubbles, ["M6 0 H94 A6 6 0 0 1 100 6 V24 A6 6 0 0 1 94 30 H6 A6 6 0 0 1 0 24 V6 A6 6 0 0 1 6 0 Z"]),

            // Arrows
            Item(l.DrawShapeArrow, arrows, ["M0 35 H60 V15 L100 50 L60 85 V65 H0 Z"]),
            Item(l.DrawShapeDoubleArrow, arrows, ["M0 50 L30 20 V38 H70 V20 L100 50 L70 80 V62 H30 V80 Z"]),
            Item(l.DrawShapeChevron, arrows, ["M0 0 H40 L80 50 L40 100 H0 L40 50 Z"]),
            Item(l.DrawShapeCurvedArrow, arrows, ["M10 90 A60 60 0 0 1 70 30 V10 L100 40 L70 70 V50 A40 40 0 0 0 30 90 Z"]),

            // Basic shapes
            Item(l.DrawShapeRoundedRectangle, basic, ["M15 0 H85 A15 15 0 0 1 100 15 V85 A15 15 0 0 1 85 100 H15 A15 15 0 0 1 0 85 V15 A15 15 0 0 1 15 0 Z"]),
            Item(l.DrawShapeTriangle, basic, ["M50 0 L100 88 H0 Z"]),
            Item(l.DrawShapeDiamond, basic, ["M50 0 L100 50 L50 100 L0 50 Z"]),
            Item(l.DrawShapePentagon, basic, [Polygon(50, 50, 5, -90, 50)]),
            Item(l.DrawShapeHexagon, basic, [Polygon(50, 50, 6, 0, 50)]),
            Item(l.DrawShapeOctagon, basic, [Polygon(50, 50, 8, 22.5, 50)]),
            Item(l.DrawShapeStar, basic, [Polygon(50, 50, 10, -90, 50, 20)]),
            Item(l.DrawShapeRing, basic, [Circle(50, 50, 50)], [Circle(50, 50, 32)]),
            Item(l.DrawShapePlus, basic, ["M35 0 H65 V35 H100 V65 H65 V100 H35 V65 H0 V35 H35 Z"]),
            Item(l.DrawShapeBanner, basic, ["M0 20 H100 L88 45 L100 70 H0 L12 45 Z"]),

            // Symbols
            Item(l.DrawShapeHeart, symbols, ["M50 92 C20 70 0 52 0 30 C0 12 13 0 28 0 C38 0 46 6 50 14 C54 6 62 0 72 0 C87 0 100 12 100 30 C100 52 80 70 50 92 Z"]),
            Item(l.DrawShapeCheck, symbols, ["M0 55 L15 40 L38 62 L85 10 L100 25 L38 92 Z"]),
            Item(l.DrawShapeMusicNote, symbols,
            [
                "M14.96 85.47 A16 11 -20 1 0 45.04 74.53 A16 11 -20 1 0 14.96 85.47 Z",
                "M40 8 H46 V76 H40 Z",
                "M46 8 C52 22 74 28 74 50 C70 38 58 33 46 32 Z",
            ]),
            Item(l.DrawShapeMusicNotes, symbols,
            [
                "M9.04 87.47 A16 11 -20 1 0 39.04 76.53 A16 11 -20 1 0 9.04 87.47 Z",
                "M59 77.47 A16 11 -20 1 0 89.04 66.53 A16 11 -20 1 0 59 77.47 Z",
                "M34 18 H40 V80 H34 Z",
                "M84 8 H90 V70 H84 Z",
                "M34 18 L90 6 V18 L34 30 Z",
            ]),
            Item(l.DrawShapeLightning, symbols, ["M58 0 L10 58 H44 L36 100 L90 38 H56 Z"]),
            Item(l.DrawShapeCloud, symbols, [Circle(30, 58, 22), Circle(52, 42, 28), Circle(76, 58, 22), "M30 58 H76 V80 H30 Z"]),
            Item(l.DrawShapeMoon, symbols, [Circle(50, 50, 50)], [Circle(70, 36, 42)]),
            Item(l.DrawShapeBadge, symbols, [Polygon(50, 50, 32, -90, 50, 43)]),
        ];
    }
}
