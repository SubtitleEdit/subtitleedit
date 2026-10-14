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

    /// <summary>
    /// Where text goes (a speech bubble's body), in the shape's own coordinates; null = no text area.
    /// </summary>
    public (float Left, float Top, float Right, float Bottom)? TextArea { get; }

    /// <summary>
    /// The parts keep their own colors (saved shapes, clip art) instead of taking the current color.
    /// </summary>
    public bool HasOwnColors { get; }

    public ShapeLibraryItem(string name, string category, Func<List<DrawShape>> build, SeAssaDrawShape? userShape = null,
        (float Left, float Top, float Right, float Bottom)? textArea = null, bool ownColors = false)
    {
        Name = name;
        Category = category;
        _build = build;
        UserShape = userShape;
        TextArea = textArea;
        HasOwnColors = ownColors || userShape != null;
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
            shape.ForeColor = HasOwnColors ? template.ForeColor : color;
            shape.Hidden = false;
            foreach (var point in shape.Points)
            {
                point.X = MathF.Round(x + (point.X - left) * scaleX, 2);
                point.Y = MathF.Round(y + (point.Y - top) * scaleY, 2);
            }

            result.Add(shape);
        }

        // The text area belongs to the topmost part under its center (a burst's yellow middle,
        // not the red edge below it - text goes on its shape's layer), else the biggest part
        if (TextArea is { } area && result.Count > 0)
        {
            var textLeft = x + (area.Left - left) * scaleX;
            var textTop = y + (area.Top - top) * scaleY;
            var textRight = x + (area.Right - left) * scaleX;
            var textBottom = y + (area.Bottom - top) * scaleY;
            var centerX = (textLeft + textRight) / 2;
            var centerY = (textTop + textBottom) / 2;
            var host = result
                .Where(s => !s.IsEraser && s.HitTest(centerX, centerY, 0))
                .OrderByDescending(s => s.Layer)
                .ThenByDescending(Area)
                .FirstOrDefault() ?? result.OrderByDescending(Area).First();
            host.SetTextBounds(textLeft, textTop, textRight, textBottom);
        }

        return result;
    }

    private static float Area(DrawShape shape)
    {
        var (left, top, right, bottom) = shape.GetBounds();
        return (right - left) * (bottom - top);
    }

    /// <summary>
    /// Thumbnail geometry per layer with its color, bottom layer first, moved so the shape starts at 0,0.
    /// </summary>
    public List<(Color Color, Geometry Geometry)> ToColoredGeometries()
    {
        var (left, top, _, _) = Bounds;
        var result = new List<(Color, Geometry)>();
        foreach (var layer in Template.Where(s => s.Points.Count > 2 && !s.IsEraser).GroupBy(s => s.Layer).OrderBy(g => g.Key))
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.SetFillRule(FillRule.NonZero);
                foreach (var shape in layer)
                {
                    AddFigure(ctx, shape, left, top);
                }
            }

            result.Add((layer.First().ForeColor, geometry));
        }

        return result;
    }

    private static void AddFigure(StreamGeometryContext ctx, DrawShape shape, float left, float top)
    {
        ctx.BeginFigure(new Avalonia.Point(shape.Points[0].X - left, shape.Points[0].Y - top), true);
        var i = 1;
        while (i < shape.Points.Count)
        {
            var point = shape.Points[i];
            if (point.DrawType == DrawCoordinateType.BezierCurveSupport1 && i + 2 < shape.Points.Count)
            {
                ctx.CubicBezierTo(
                    new Avalonia.Point(point.X - left, point.Y - top),
                    new Avalonia.Point(shape.Points[i + 1].X - left, shape.Points[i + 1].Y - top),
                    new Avalonia.Point(shape.Points[i + 2].X - left, shape.Points[i + 2].Y - top));
                i += 3;
            }
            else
            {
                ctx.LineTo(new Avalonia.Point(point.X - left, point.Y - top));
                i++;
            }
        }

        ctx.EndFigure(true);
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
        Se.Language.Assa.DrawCategoryClipArt,
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

    /// <summary>
    /// A shape with room for text, the text area given in the same 100-unit coordinates as the path data.
    /// </summary>
    private static ShapeLibraryItem TextItem(string name, string category, (float Left, float Top, float Right, float Bottom) textArea, string[] union)
    {
        // Path data is scaled by 10 when built (see SvgImporter.FromPathData)
        var (l, t, r, b) = textArea;
        return new ShapeLibraryItem(name, category, () => SvgImporter.FromPathData(union), textArea: (l * 10, t * 10, r * 10, b * 10));
    }

    /// <summary>
    /// Colored clip art: an SVG body for a 100 x 100 view box, imported like an SVG file
    /// (one layer per color run), with an optional text area in the same coordinates.
    /// </summary>
    private static ShapeLibraryItem Clip(string name, (float Left, float Top, float Right, float Bottom)? textArea, string svgBody)
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\">" + svgBody + "</svg>";

        // Imported at 1000 x 1000, so the view box is scaled by 10 like the path data shapes
        (float, float, float, float)? area = textArea is { } a ? (a.Left * 10, a.Top * 10, a.Right * 10, a.Bottom * 10) : null;
        return new ShapeLibraryItem(name, Se.Language.Assa.DrawCategoryClipArt, () => SvgImporter.Import(svg, 1000, 1000, 0).Shapes,
            textArea: area, ownColors: true);
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
            TextItem(l.DrawShapeSpeechBubble, bubbles, (8, 6, 92, 64), ["M12 0 H88 A12 12 0 0 1 100 12 V58 A12 12 0 0 1 88 70 H38 L14 94 L22 70 H12 A12 12 0 0 1 0 58 V12 A12 12 0 0 1 12 0 Z"]),
            TextItem(l.DrawShapeSpeechBubbleRight, bubbles, (8, 6, 92, 64), ["M12 0 H88 A12 12 0 0 1 100 12 V58 A12 12 0 0 1 88 70 H78 L86 94 L62 70 H12 A12 12 0 0 1 0 58 V12 A12 12 0 0 1 12 0 Z"]),
            TextItem(l.DrawShapeSpeechBubbleUp, bubbles, (8, 30, 92, 88), ["M12 24 H22 L30 0 L46 24 H88 A12 12 0 0 1 100 36 V82 A12 12 0 0 1 88 94 H12 A12 12 0 0 1 0 82 V36 A12 12 0 0 1 12 24 Z"]),
            TextItem(l.DrawShapeOvalBubble, bubbles, (14, 11, 86, 65), ["M0 38 A50 38 0 1 1 100 38 A50 38 0 1 1 0 38 Z", "M22 62 L8 96 L44 72 Z"]),
            TextItem(l.DrawShapeBoxBubble, bubbles, (6, 6, 94, 64), ["M0 0 H100 V70 H60 L50 94 L40 70 H0 Z"]),
            TextItem(l.DrawShapeThoughtBubble, bubbles, (16, 16, 84, 64),
            [
                Circle(26, 34, 20), Circle(46, 22, 22), Circle(68, 24, 20), Circle(84, 40, 16),
                Circle(70, 56, 18), Circle(46, 58, 20), Circle(22, 54, 17), Circle(50, 40, 26),
                Circle(18, 82, 7), Circle(8, 95, 4),
            ]),
            TextItem(l.DrawShapeShoutBubble, bubbles, (27, 30, 73, 70), [Polygon(50, 50, 24, -90, 50, 36, 46, 34, 50, 38, 44, 35)]),
            TextItem(l.DrawShapeSpeechBubbleSide, bubbles, (28, 6, 92, 64),
                ["M32 0 H88 A12 12 0 0 1 100 12 V58 A12 12 0 0 1 88 70 H32 A12 12 0 0 1 20 58 V50 L0 66 L20 34 V12 A12 12 0 0 1 32 0 Z"]),
            TextItem(l.DrawShapeRoundBubble, bubbles, (21, 13, 79, 71), [Circle(50, 42, 42), "M30 74 L12 100 L52 82 Z"]),
            TextItem(l.DrawShapePillBubble, bubbles, (12, 6, 88, 44), ["M25 0 H75 A25 25 0 0 1 75 50 H42 L20 76 L28 50 H25 A25 25 0 0 1 25 0 Z"]),
            TextItem(l.DrawShapeScreamBubble, bubbles, (27, 24, 73, 68), [Polygon(50, 46, 22, -90, 46, 34), "M32 70 L12 100 L50 76 Z"]),
            TextItem(l.DrawShapeCaptionBox, bubbles, (4, 3, 96, 27), ["M6 0 H94 A6 6 0 0 1 100 6 V24 A6 6 0 0 1 94 30 H6 A6 6 0 0 1 0 24 V6 A6 6 0 0 1 6 0 Z"]),

            // Arrows
            Item(l.DrawShapeArrow, arrows, ["M0 35 H60 V15 L100 50 L60 85 V65 H0 Z"]),
            Item(l.DrawShapeDoubleArrow, arrows, ["M0 50 L30 20 V38 H70 V20 L100 50 L70 80 V62 H30 V80 Z"]),
            Item(l.DrawShapeChevron, arrows, ["M0 0 H40 L80 50 L40 100 H0 L40 50 Z"]),
            Item(l.DrawShapeCurvedArrow, arrows, ["M10 90 A60 60 0 0 1 70 30 V10 L100 40 L70 70 V50 A40 40 0 0 0 30 90 Z"]),
            // 300° ring (clockwise) ending in an arrowhead at the top
            Item(l.DrawShapeRepeatArrow, arrows,
                ["M78.28 16.29 A44 44 0 1 1 34.95 8.65 L32.22 1.14 L56.15 8.39 L42.48 29.33 L39.74 21.81 A30 30 0 1 0 69.28 27.02 Z"]),
            Item(l.DrawShapeUTurnArrow, arrows, ["M8 100 V40 A34 34 0 0 1 76 40 V70 H86 L68 100 L50 70 H60 V40 A18 18 0 0 0 24 40 V100 Z"]),

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
            Item(l.DrawShapeHalfCircle, basic, ["M0 50 A50 50 0 0 1 100 50 Z"]),
            Item(l.DrawShapeTeardrop, basic, ["M50 0 C50 0 90 45 90 70 A40 40 0 0 1 10 70 C10 45 50 0 50 0 Z"]),

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
            Item(l.DrawShapeSpeaker, symbols,
            [
                "M0 35 H22 L50 10 V90 L22 65 H0 Z",
                "M65.56 34.44 A22 22 0 0 1 65.56 65.56 L59.9 59.9 A14 14 0 0 0 59.9 40.1 Z",
                "M78.28 21.72 A40 40 0 0 1 78.28 78.28 L72.63 72.63 A32 32 0 0 0 72.63 27.37 Z",
            ]),
            Item(l.DrawShapeCross, symbols, ["M15 0 L50 35 L85 0 L100 15 L65 50 L100 85 L85 100 L50 65 L15 100 L0 85 L35 50 L0 15 Z"]),

            // Faces: eyes and mouth are cut out, so the background shows through
            Item(l.DrawShapeSmiley, symbols, [Circle(50, 50, 50)],
                [Circle(34, 36, 8), Circle(66, 36, 8), "M26 58 A24 24 0 0 0 74 58 L64 58 A14 14 0 0 1 36 58 Z"]),
            Item(l.DrawShapeSadSmiley, symbols, [Circle(50, 50, 50)],
                [Circle(34, 36, 8), Circle(66, 36, 8), "M26 82 A24 24 0 0 1 74 82 L64 82 A14 14 0 0 0 36 82 Z"]),

            // Clip art (own colors; each color is one ASSA line, so a few colors each)
            Clip(l.DrawClipArtRedHeart, (22, 18, 78, 56),
                """<path fill="#E53935" d="M50 92 C20 70 0 52 0 30 C0 12 13 0 28 0 C38 0 46 6 50 14 C54 6 62 0 72 0 C87 0 100 12 100 30 C100 52 80 70 50 92 Z"/><ellipse fill="#FF8A80" cx="24" cy="22" rx="10" ry="6" transform="rotate(-35 24 22)"/>"""),
            Clip(l.DrawClipArtAngerMark, null,
                """<path fill="#E53935" transform="rotate(0 50 50)" d="M6 40 Q30 38 38 8 L48 8 Q46 46 6 50 Z"/><path fill="#E53935" transform="rotate(90 50 50)" d="M6 40 Q30 38 38 8 L48 8 Q46 46 6 50 Z"/><path fill="#E53935" transform="rotate(180 50 50)" d="M6 40 Q30 38 38 8 L48 8 Q46 46 6 50 Z"/><path fill="#E53935" transform="rotate(270 50 50)" d="M6 40 Q30 38 38 8 L48 8 Q46 46 6 50 Z"/>"""),
            Clip(l.DrawClipArtSweatDrop, null,
                """<path fill="#4FC3F7" d="M50 0 C50 0 88 46 88 66 A38 34 0 0 1 12 66 C12 46 50 0 50 0 Z"/><path fill="#E1F5FE" d="M30 64 Q30 50 42 38 Q36 54 38 70 Q34 74 30 64 Z"/>"""),
            Clip(l.DrawClipArtSparkles, null,
                """<path fill="#FFC107" d="M40 6 Q46 44 80 50 Q46 56 40 94 Q34 56 0 50 Q34 44 40 6 Z"/><path fill="#FFE082" d="M80 2 Q83 18 98 21 Q83 24 80 40 Q77 24 62 21 Q77 18 80 2 Z"/><path fill="#FFE082" d="M84 66 Q86 76 96 78 Q86 80 84 92 Q82 80 72 78 Q82 76 84 66 Z"/>"""),
            Clip(l.DrawClipArtBlush, null,
                """<ellipse fill="#F48FB1" fill-opacity="0.85" cx="50" cy="50" rx="48" ry="26"/><g stroke="#D81B60" stroke-width="5" stroke-linecap="round" fill="none"><path d="M28 62 L38 38"/><path d="M46 62 L56 38"/><path d="M64 62 L74 38"/></g>"""),
            Clip(l.DrawClipArtSurpriseLines, null,
                """<g fill="#212121"><path d="M14 30 L24 26 L38 70 L33 72 Z"/><path d="M45 4 L55 4 L52 62 L48 62 Z"/><path d="M86 30 L76 26 L62 70 L67 72 Z"/></g>"""),
            Clip(l.DrawClipArtBamBurst, (28, 32, 72, 68),
                """<path fill="#E53935" d="M50.0 0.0 L59.3 15.2 L75.0 6.7 L75.5 24.5 L93.3 25.0 L84.8 40.7 L100.0 50.0 L84.8 59.3 L93.3 75.0 L75.5 75.5 L75.0 93.3 L59.3 84.8 L50.0 100.0 L40.7 84.8 L25.0 93.3 L24.5 75.5 L6.7 75.0 L15.2 59.3 L0.0 50.0 L15.2 40.7 L6.7 25.0 L24.5 24.5 L25.0 6.7 L40.7 15.2 Z"/><path fill="#FFEB3B" d="M56.3 10.5 L63.6 23.3 L78.3 21.7 L76.7 36.4 L89.5 43.7 L79.6 54.7 L85.6 68.2 L71.2 71.2 L68.2 85.6 L54.7 79.6 L43.7 89.5 L36.4 76.7 L21.7 78.3 L23.3 63.6 L10.5 56.3 L20.4 45.3 L14.4 31.8 L28.8 28.8 L31.8 14.4 L45.3 20.4 Z"/>"""),
            Clip(l.DrawClipArtZzz, null,
                """<g fill="#5C6BC0"><path d="M4 56 H40 V64 L16 90 H40 V98 H4 V90 L28 64 H4 Z"/><path d="M48 30 H76 V36 L57 56 H76 V62 H48 V56 L67 36 H48 Z"/><path d="M80 6 H98 V10 L86 24 H98 V28 H80 V24 L92 10 H80 Z"/></g>"""),
            Clip(l.DrawClipArtSun, (30, 30, 70, 70),
                """<g fill="#FF9800"><path transform="rotate(0 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(30 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(60 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(90 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(120 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(150 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(180 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(210 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(240 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(270 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(300 50 50)" d="M44 14 L50 0 L56 14 Z"/><path transform="rotate(330 50 50)" d="M44 14 L50 0 L56 14 Z"/></g><circle fill="#FFC107" cx="50" cy="50" r="30"/><circle fill="#FFE082" cx="40" cy="40" r="8"/>"""),
            Clip(l.DrawClipArtFlame, null,
                """<path fill="#FF5722" d="M50 0 C58 22 84 36 84 64 A34 34 0 0 1 16 64 C16 46 28 38 32 24 C38 34 42 38 44 44 C48 30 50 16 50 0 Z"/><path fill="#FFC107" d="M50 46 C56 58 68 64 68 78 A18 18 0 0 1 32 78 C32 66 44 60 50 46 Z"/>"""),
            Clip(l.DrawClipArtLightBulb, null,
                """<path fill="#FFEB3B" d="M50 4 A30 30 0 0 1 68 58 Q63 63 63 72 H37 Q37 63 32 58 A30 30 0 0 1 50 4 Z"/><path fill="#FFF9C4" d="M36 32 A16 16 0 0 1 48 16 A22 22 0 0 0 40 40 Z"/><rect fill="#9E9E9E" x="36" y="74" width="28" height="9" rx="2"/><rect fill="#616161" x="39" y="85" width="22" height="9" rx="3"/>"""),
            Clip(l.DrawClipArtLightningBolt, null,
                """<path fill="#FFD600" stroke="#F57F17" stroke-width="3" stroke-linejoin="round" d="M60 2 L14 58 H44 L36 98 L86 38 H56 Z"/>"""),
            Clip(l.DrawClipArtColoredMusicNotes, null,
                """<g fill="#7E57C2"><ellipse cx="20" cy="82" rx="14" ry="10" transform="rotate(-20 20 82)"/><path d="M29 20 H35 V80 H29 Z"/><path d="M35 20 C40 32 56 36 56 54 C52 44 44 42 35 40 Z"/></g><g fill="#26A69A"><ellipse cx="70" cy="72" rx="12" ry="9" transform="rotate(-20 70 72)"/><ellipse cx="88" cy="62" rx="12" ry="9" transform="rotate(-20 88 62)"/><path d="M77 22 H82 V70 H77 Z"/><path d="M95 12 H100 V60 H95 Z"/><path d="M77 22 L100 12 V22 L77 32 Z"/></g>"""),
            Clip(l.DrawClipArtPhone, (31, 14, 69, 80),
                """<rect fill="#37474F" x="24" y="0" width="52" height="100" rx="8"/><rect fill="#81D4FA" x="29" y="10" width="42" height="74" rx="2"/><circle fill="#90A4AE" cx="50" cy="92" r="4"/>"""),
            Clip(l.DrawClipArtQuestionMark, null,
                """<circle fill="#1E88E5" cx="50" cy="50" r="50"/><path fill="none" stroke="#FFFFFF" stroke-width="10" stroke-linecap="round" stroke-linejoin="round" d="M35 36 Q35 20 50 20 Q65 20 65 34 Q65 44 50 50 V58"/><circle fill="#FFFFFF" cx="50" cy="76" r="7"/>"""),
            Clip(l.DrawClipArtExclamationMark, null,
                """<circle fill="#E53935" cx="50" cy="50" r="50"/><path fill="#FFFFFF" d="M43 18 H57 L54 62 H46 Z"/><circle fill="#FFFFFF" cx="50" cy="77" r="7"/>"""),
            Clip(l.DrawClipArtGreenCheck, null,
                """<circle fill="#43A047" cx="50" cy="50" r="50"/><path fill="none" stroke="#FFFFFF" stroke-width="11" stroke-linecap="round" stroke-linejoin="round" d="M27 52 L43 68 L74 34"/>"""),
            Clip(l.DrawClipArtRedCross, null,
                """<circle fill="#E53935" cx="50" cy="50" r="50"/><path fill="none" stroke="#FFFFFF" stroke-width="11" stroke-linecap="round" d="M32 32 L68 68 M68 32 L32 68"/>"""),
            Clip(l.DrawClipArtThumbsUp, null,
                """<rect fill="#1E88E5" x="4" y="44" width="22" height="52" rx="3"/><path fill="#FFCA28" d="M30 46 L46 18 Q50 8 58 12 Q66 16 62 30 L58 42 H84 Q96 42 94 54 L88 86 Q86 96 74 96 H30 Z"/><path fill="none" stroke="#F9A825" stroke-width="3" stroke-linecap="round" d="M62 60 H90 M60 74 H87"/>"""),
            Clip(l.DrawClipArtGoldStar, null,
                """<path fill="#FFC107" d="M50.0 5.0 L61.2 37.6 L95.7 38.2 L68.1 58.9 L78.2 91.8 L50.0 72.0 L21.8 91.8 L31.9 58.9 L4.3 38.2 L38.8 37.6 Z"/><path fill="#FFE082" d="M50.0 23.0 L57.1 43.3 L78.5 43.7 L61.4 56.7 L67.6 77.3 L50.0 65.0 L32.4 77.3 L38.6 56.7 L21.5 43.7 L42.9 43.3 Z"/>"""),
            Clip(l.DrawClipArtBrokenHeart, null,
                """<path fill="#E53935" d="M45 90 C18 70 0 52 0 30 C0 12 13 2 28 2 C37 2 42 7 45 12 L38 32 L48 46 L38 62 Z"/><path fill="#C62828" d="M55 12 C58 7 63 2 72 2 C87 2 100 12 100 30 C100 52 82 70 55 90 L47 62 L57 46 L47 32 Z"/>"""),
            Clip(l.DrawClipArtGhost, null,
                """<path fill="#ECEFF1" d="M12 46 A38 38 0 0 1 88 46 V94 L76 84 L63 94 L50 84 L37 94 L24 84 L12 94 Z"/><g fill="#37474F"><ellipse cx="37" cy="44" rx="7" ry="10"/><ellipse cx="63" cy="44" rx="7" ry="10"/><ellipse cx="50" cy="66" rx="8" ry="6"/></g>"""),
            Clip(l.DrawClipArtSkull, null,
                """<path fill="#ECEFF1" d="M50 4 C76 4 92 22 92 46 C92 60 86 68 78 72 V86 Q78 94 70 94 H30 Q22 94 22 86 V72 C14 68 8 60 8 46 C8 22 24 4 50 4 Z"/><g fill="#263238"><circle cx="33" cy="48" r="11"/><circle cx="67" cy="48" r="11"/><path d="M50 60 L57 72 H43 Z"/><rect x="36" y="80" width="5" height="14"/><rect x="48" y="80" width="5" height="14"/><rect x="60" y="80" width="5" height="14"/></g>"""),
            Clip(l.DrawClipArtSnowflake, null,
                """<g stroke="#4FC3F7" stroke-width="7" stroke-linecap="round" fill="none"><g transform="rotate(0 50 50)"><path d="M50 50 V6"/><path d="M50 22 L38 12 M50 22 L62 12"/></g><g transform="rotate(60 50 50)"><path d="M50 50 V6"/><path d="M50 22 L38 12 M50 22 L62 12"/></g><g transform="rotate(120 50 50)"><path d="M50 50 V6"/><path d="M50 22 L38 12 M50 22 L62 12"/></g><g transform="rotate(180 50 50)"><path d="M50 50 V6"/><path d="M50 22 L38 12 M50 22 L62 12"/></g><g transform="rotate(240 50 50)"><path d="M50 50 V6"/><path d="M50 22 L38 12 M50 22 L62 12"/></g><g transform="rotate(300 50 50)"><path d="M50 50 V6"/><path d="M50 22 L38 12 M50 22 L62 12"/></g></g>"""),
            Clip(l.DrawClipArtMoonAndStars, null,
                """<path fill="#FFD54F" d="M58 6 A44 44 0 1 0 94 70 A36 36 0 1 1 58 6 Z"/><path fill="#FFF59D" d="M78.0 9.0 L80.9 18.0 L90.4 18.0 L82.8 23.5 L85.6 32.5 L78.0 27.0 L70.4 32.5 L73.2 23.5 L65.6 18.0 L75.1 18.0 Z"/><path fill="#FFF59D" d="M90.0 39.0 L91.8 43.6 L96.7 43.8 L92.9 46.9 L94.1 51.7 L90.0 49.0 L85.9 51.7 L87.1 46.9 L83.3 43.8 L88.2 43.6 Z"/>"""),
            Clip(l.DrawClipArtRainCloud, (24, 24, 80, 58),
                """<path fill="#B0BEC5" d="M24 62 A18 18 0 0 1 22 26 A24 24 0 0 1 66 16 A20 20 0 0 1 84 62 Z"/><g fill="#29B6F6"><path d="M30 70 C30 70 22 82 22 87 A8 8 0 0 0 38 87 C38 82 30 70 30 70 Z"/><path d="M54 74 C54 74 46 86 46 91 A8 8 0 0 0 62 91 C62 86 54 74 54 74 Z"/><path d="M76 68 C76 68 68 80 68 85 A8 8 0 0 0 84 85 C84 80 76 68 76 68 Z"/></g>"""),
            Clip(l.DrawClipArtBomb, null,
                """<path fill="none" stroke="#8D6E63" stroke-width="5" stroke-linecap="round" d="M62 30 Q70 12 82 16"/><rect fill="#263238" x="50" y="26" width="18" height="12" rx="2" transform="rotate(30 59 32)"/><circle fill="#263238" cx="42" cy="60" r="36"/><circle fill="#546E7A" cx="30" cy="48" r="8"/><path fill="#FF9800" d="M86.0 0.0 L88.5 7.7 L96.4 6.0 L91.0 12.0 L96.4 18.0 L88.5 16.3 L86.0 24.0 L83.5 16.3 L75.6 18.0 L81.0 12.0 L75.6 6.0 L83.5 7.7 Z"/>"""),
            Clip(l.DrawClipArtGift, null,
                """<rect fill="#E53935" x="8" y="40" width="84" height="56" rx="3"/><rect fill="#C62828" x="4" y="28" width="92" height="16" rx="3"/><g fill="#FFC107"><rect x="44" y="28" width="12" height="68"/><path d="M50 28 C40 6 18 8 22 20 C26 30 44 28 50 28 Z"/><path d="M50 28 C60 6 82 8 78 20 C74 30 56 28 50 28 Z"/></g>"""),
            Clip(l.DrawClipArtCrown, null,
                """<path fill="#FFC107" d="M6 30 L28 54 L50 18 L72 54 L94 30 L86 82 H14 Z"/><rect fill="#FFA000" x="14" y="82" width="72" height="12" rx="2"/><g fill="#E53935"><circle cx="50" cy="62" r="7"/></g><g fill="#42A5F5"><circle cx="28" cy="68" r="5"/><circle cx="72" cy="68" r="5"/></g>"""),
            Clip(l.DrawClipArtClock, null,
                """<circle fill="#455A64" cx="50" cy="50" r="48"/><circle fill="#ECEFF1" cx="50" cy="50" r="40"/><g fill="#263238"><rect x="47" y="20" width="6" height="32" rx="3"/><rect x="47" y="47" width="26" height="6" rx="3" transform="rotate(30 50 50)"/><circle cx="50" cy="50" r="5"/></g>"""),
            Clip(l.DrawClipArtClapperboard, (10, 54, 90, 92),
                """<rect fill="#263238" x="6" y="40" width="88" height="56" rx="4"/><path fill="#263238" d="M4 26 L88 6 L92 22 L8 42 Z"/><g fill="#ECEFF1"><path d="M18 23 L30 20 L22 38 L10 41 Z"/><path d="M42 18 L54 15 L46 33 L34 36 Z"/><path d="M66 12 L78 9 L70 27 L58 30 Z"/><rect x="6" y="40" width="88" height="10"/></g>"""),
            Clip(l.DrawClipArtMicrophone, null,
                """<g fill="#455A64"><path d="M22 46 A28 28 0 0 0 78 46 H71 A21 21 0 0 1 29 46 Z"/><rect x="46" y="72" width="8" height="16"/><rect x="30" y="86" width="40" height="8" rx="3"/></g><rect fill="#90A4AE" x="34" y="4" width="32" height="58" rx="16"/><g fill="#607D8B"><rect x="38" y="18" width="24" height="3" rx="1.5"/><rect x="38" y="26" width="24" height="3" rx="1.5"/><rect x="38" y="34" width="24" height="3" rx="1.5"/></g>"""),
            Clip(l.DrawClipArtWarningSign, null,
                """<path fill="#FFC107" stroke="#FFC107" stroke-width="8" stroke-linejoin="round" d="M50 8 L94 88 H6 Z"/><g fill="#212121"><path d="M45 34 H55 L53 66 H47 Z"/><circle cx="50" cy="77" r="5"/></g>"""),
            Clip(l.DrawClipArtNoEntry, null,
                """<circle fill="#E53935" cx="50" cy="50" r="48"/><rect fill="#FFFFFF" x="16" y="40" width="68" height="20" rx="3"/>"""),
            Clip(l.DrawClipArtPopcorn, null,
                """<g fill="#FFF3C4"><circle cx="28" cy="30" r="13"/><circle cx="44" cy="20" r="14"/><circle cx="60" cy="22" r="13"/><circle cx="74" cy="32" r="12"/><circle cx="50" cy="34" r="13"/></g><path fill="#E53935" d="M16 38 H84 L74 98 H26 Z"/><g fill="#FFFFFF"><path d="M30 38 H40 L42 98 H34 Z"/><path d="M60 38 H70 L66 98 H58 Z"/></g>"""),
            Clip(l.DrawClipArtCoffeeCup, null,
                """<g fill="none" stroke="#B0BEC5" stroke-width="4" stroke-linecap="round"><path d="M34 30 Q28 23 34 16 Q40 9 34 3"/><path d="M50 30 Q44 23 50 16 Q56 9 50 3"/><path d="M66 30 Q60 23 66 16 Q72 9 66 3"/></g><path fill="#ECEFF1" d="M14 34 H78 V70 Q78 92 56 92 H36 Q14 92 14 70 Z M78 42 A16 16 0 0 1 78 74 V66 A8 8 0 0 0 78 50 Z"/><path fill="#6D4C41" d="M14 34 H78 V44 H14 Z"/>"""),
        ];
    }
}
