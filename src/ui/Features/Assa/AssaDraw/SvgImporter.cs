using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Media;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Assa.AssaDraw;

/// <summary>
/// Converts an SVG image into ASSA drawing shapes. ASSA drawings only know move/line/cubic bezier
/// and a single fill color per line, so: paths are parsed by Skia (arcs, quads and conics become
/// cubics), transforms are baked into the points, strokes are turned into filled outlines, and
/// every change of color starts a new layer (document order = stacking order).
/// Not supported: gradients (first stop color is used), patterns, masks/clip paths, text, filters.
/// </summary>
public static class SvgImporter
{
    private static readonly Regex NumberRegex = new(@"[-+]?(?:\d*\.\d+|\d+\.?)(?:[eE][-+]?\d+)?", RegexOptions.Compiled);
    private static readonly Regex TransformRegex = new(@"(matrix|translate|scale|rotate|skewX|skewY)\s*\(([^)]*)\)", RegexOptions.Compiled);
    private static readonly Regex CssRuleRegex = new(@"([^{}]+)\{([^}]*)\}", RegexOptions.Compiled);
    private static readonly Regex CssCommentRegex = new(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly HashSet<string> SkippedElements = new(StringComparer.Ordinal)
    {
        "defs", "clipPath", "mask", "symbol", "marker", "pattern", "linearGradient", "radialGradient",
        "filter", "title", "desc", "metadata", "style", "script", "text", "image", "foreignObject",
    };

    private static readonly string[] InheritedProperties =
    {
        "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width", "stroke-opacity",
        "stroke-linecap", "stroke-linejoin", "stroke-miterlimit", "color", "visibility",
    };

    private const float FullFrameAspectTolerance = 0.01f;
    private const float FitFraction = 0.5f;

    public sealed class Result
    {
        public List<DrawShape> Shapes { get; } = [];
    }

    private sealed class Context
    {
        public SKMatrix Matrix = SKMatrix.Identity;
        public Dictionary<string, string> Properties = new(StringComparer.Ordinal);
        public float Opacity = 1;
    }

    private sealed class Painted
    {
        public SKPath Path = null!;
        public Color Color;
    }

    private sealed class Segment
    {
        public bool IsCubic;
        public SKPoint Control1;
        public SKPoint Control2;
        public SKPoint End;
    }

    private sealed class Contour
    {
        public SKPoint Start;
        public List<Segment> Segments = [];
    }

    /// <summary>
    /// Imports the SVG and places it on a canvas of the given size: an SVG with the canvas' aspect
    /// ratio (a full-frame overlay) fills the frame, anything else is fitted to half the frame and centered.
    /// </summary>
    public static Result Import(string svgText, int canvasWidth, int canvasHeight, int firstLayer)
    {
        // Illustrator writes a DOCTYPE with entities used in attributes, so the DTD is parsed -
        // with no resolver (no external files) and an entity size cap.
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null,
            MaxCharactersFromEntities = 1_000_000,
        };
        using var stringReader = new System.IO.StringReader(svgText);
        using var reader = XmlReader.Create(stringReader, settings);
        var document = XDocument.Load(reader, LoadOptions.None);
        var root = document.Root ?? throw new XmlException("No root element");
        if (root.Name.LocalName != "svg")
        {
            throw new XmlException("Not an SVG file");
        }

        var ids = new Dictionary<string, XElement>(StringComparer.Ordinal);
        foreach (var element in root.DescendantsAndSelf())
        {
            var id = (string?)element.Attribute("id");
            if (!string.IsNullOrEmpty(id))
            {
                ids.TryAdd(id, element);
            }
        }

        var classRules = ParseStyleSheets(root);
        var painted = new List<Painted>();
        Walk(root, new Context(), painted, ids, classRules, 0);

        var result = new Result();
        if (painted.Count == 0)
        {
            return result;
        }

        var fit = GetFitMatrix(root, painted, canvasWidth, canvasHeight);
        var layer = firstLayer - 1;
        Color? layerColor = null;
        foreach (var item in painted)
        {
            item.Path.Transform(fit);
            var simplified = item.Path.Simplify() ?? item.Path;
            var contours = ReadContours(simplified);
            NormalizeDirections(contours);

            if (contours.Count == 0)
            {
                continue;
            }

            if (layerColor != item.Color)
            {
                layer++;
                layerColor = item.Color;
            }

            foreach (var contour in contours)
            {
                result.Shapes.Add(ToDrawShape(contour, layer, item.Color));
            }
        }

        return result;
    }

    /// <summary>
    /// Builds shapes from SVG path data: the <paramref name="union"/> paths are merged, the
    /// <paramref name="subtract"/> paths cut out of them, and the result scaled by <paramref name="scale"/>.
    /// Used for the built-in shape library, which is written as SVG path data.
    /// </summary>
    public static List<DrawShape> FromPathData(IEnumerable<string> union, IEnumerable<string>? subtract = null, float scale = 10)
    {
        SKPath? result = null;
        foreach (var data in union)
        {
            var path = SKPath.ParseSvgPathData(data);
            if (path == null)
            {
                continue;
            }

            path.FillType = SKPathFillType.Winding;
            result = result == null ? path.Simplify() ?? path : result.Op(path, SKPathOp.Union) ?? result;
        }

        if (result == null)
        {
            return [];
        }

        foreach (var data in subtract ?? [])
        {
            var path = SKPath.ParseSvgPathData(data);
            if (path != null)
            {
                result = result.Op(path, SKPathOp.Difference) ?? result;
            }
        }

        result.Transform(SKMatrix.CreateScale(scale, scale));
        var contours = ReadContours(result.Simplify() ?? result);
        NormalizeDirections(contours);
        return contours.Select(c => ToDrawShape(c, 0, Colors.White)).ToList();
    }

    private static void Walk(XElement element, Context parent, List<Painted> painted, Dictionary<string, XElement> ids,
        Dictionary<string, Dictionary<string, string>> classRules, int depth)
    {
        var name = element.Name.LocalName;
        if (depth > 32 || SkippedElements.Contains(name))
        {
            return;
        }

        var properties = GetProperties(element, classRules);
        if (properties.TryGetValue("display", out var display) && display == "none")
        {
            return;
        }

        var context = new Context
        {
            Matrix = parent.Matrix,
            Properties = new Dictionary<string, string>(parent.Properties, StringComparer.Ordinal),
            Opacity = parent.Opacity,
        };

        foreach (var key in InheritedProperties)
        {
            if (properties.TryGetValue(key, out var value) && value != "inherit")
            {
                context.Properties[key] = value;
            }
        }

        if (properties.TryGetValue("opacity", out var opacity))
        {
            context.Opacity *= ParseOpacity(opacity);
        }

        var transform = (string?)element.Attribute("transform");
        if (!string.IsNullOrEmpty(transform))
        {
            context.Matrix = context.Matrix.PreConcat(ParseTransform(transform));
        }

        switch (name)
        {
            case "g":
            case "a":
            case "switch":
                foreach (var child in element.Elements())
                {
                    Walk(child, context, painted, ids, classRules, depth + 1);
                }

                return;

            case "svg":
                // Nested viewport: only its position is used
                context.Matrix = context.Matrix.PreConcat(SKMatrix.CreateTranslation(GetLength(element, "x"), GetLength(element, "y")));
                foreach (var child in element.Elements())
                {
                    Walk(child, context, painted, ids, classRules, depth + 1);
                }

                return;

            case "use":
                var href = ((string?)element.Attribute("href") ?? (string?)element.Attribute(XName.Get("href", "http://www.w3.org/1999/xlink")))?.TrimStart('#');
                if (href != null && ids.TryGetValue(href, out var target) && !target.AncestorsAndSelf().Contains(element))
                {
                    context.Matrix = context.Matrix.PreConcat(SKMatrix.CreateTranslation(GetLength(element, "x"), GetLength(element, "y")));
                    if (target.Name.LocalName == "symbol")
                    {
                        foreach (var child in target.Elements())
                        {
                            Walk(child, context, painted, ids, classRules, depth + 1);
                        }
                    }
                    else
                    {
                        Walk(target, context, painted, ids, classRules, depth + 1);
                    }
                }

                return;
        }

        var path = MakePath(element);
        if (path == null)
        {
            return;
        }

        if (context.Properties.TryGetValue("visibility", out var visibility) && visibility is "hidden" or "collapse")
        {
            return;
        }

        AddFill(path, context, painted, ids, name);
        AddStroke(path, context, painted, ids);
    }

    private static void AddFill(SKPath source, Context context, List<Painted> painted, Dictionary<string, XElement> ids, string elementName)
    {
        var fill = context.Properties.GetValueOrDefault("fill", "black");
        if (elementName is "line" || !TryGetPaintColor(fill, context, ids, out var color))
        {
            return;
        }

        var alpha = context.Opacity * ParseOpacity(context.Properties.GetValueOrDefault("fill-opacity", "1"));
        var path = new SKPath(source)
        {
            FillType = context.Properties.GetValueOrDefault("fill-rule") == "evenodd" ? SKPathFillType.EvenOdd : SKPathFillType.Winding,
        };
        path.Transform(context.Matrix);
        painted.Add(new Painted { Path = path, Color = WithAlpha(color, alpha) });
    }

    private static void AddStroke(SKPath source, Context context, List<Painted> painted, Dictionary<string, XElement> ids)
    {
        var stroke = context.Properties.GetValueOrDefault("stroke", "none");
        var width = ParseFloat(context.Properties.GetValueOrDefault("stroke-width", "1"), 1);
        if (width <= 0 || !TryGetPaintColor(stroke, context, ids, out var color))
        {
            return;
        }

        // ASSA has no strokes the drawing could use per shape, so the stroke becomes a filled outline
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = width,
            StrokeMiter = ParseFloat(context.Properties.GetValueOrDefault("stroke-miterlimit", "4"), 4),
            StrokeCap = context.Properties.GetValueOrDefault("stroke-linecap") switch
            {
                "round" => SKStrokeCap.Round,
                "square" => SKStrokeCap.Square,
                _ => SKStrokeCap.Butt,
            },
            StrokeJoin = context.Properties.GetValueOrDefault("stroke-linejoin") switch
            {
                "round" => SKStrokeJoin.Round,
                "bevel" => SKStrokeJoin.Bevel,
                _ => SKStrokeJoin.Miter,
            },
        };

        var outline = new SKPath();
        if (!paint.GetFillPath(source, outline))
        {
            outline.Dispose();
            return;
        }

        outline.FillType = SKPathFillType.Winding;
        outline.Transform(context.Matrix);
        var alpha = context.Opacity * ParseOpacity(context.Properties.GetValueOrDefault("stroke-opacity", "1"));
        painted.Add(new Painted { Path = outline, Color = WithAlpha(color, alpha) });
    }

    private static SKPath? MakePath(XElement element)
    {
        var path = new SKPath();
        switch (element.Name.LocalName)
        {
            case "path":
                var data = (string?)element.Attribute("d");
                if (string.IsNullOrWhiteSpace(data))
                {
                    return null;
                }

                return SKPath.ParseSvgPathData(data);

            case "rect":
                var width = GetLength(element, "width");
                var height = GetLength(element, "height");
                if (width <= 0 || height <= 0)
                {
                    return null;
                }

                var rect = SKRect.Create(GetLength(element, "x"), GetLength(element, "y"), width, height);
                var rx = element.Attribute("rx") != null ? GetLength(element, "rx") : -1;
                var ry = element.Attribute("ry") != null ? GetLength(element, "ry") : -1;
                if (rx < 0)
                {
                    rx = ry;
                }

                if (ry < 0)
                {
                    ry = rx;
                }

                if (rx > 0 && ry > 0)
                {
                    path.AddRoundRect(rect, Math.Min(rx, width / 2), Math.Min(ry, height / 2));
                }
                else
                {
                    path.AddRect(rect);
                }

                return path;

            case "circle":
                var r = GetLength(element, "r");
                if (r <= 0)
                {
                    return null;
                }

                path.AddCircle(GetLength(element, "cx"), GetLength(element, "cy"), r);
                return path;

            case "ellipse":
                var erx = GetLength(element, "rx");
                var ery = GetLength(element, "ry");
                if (erx <= 0 || ery <= 0)
                {
                    return null;
                }

                var cx = GetLength(element, "cx");
                var cy = GetLength(element, "cy");
                path.AddOval(new SKRect(cx - erx, cy - ery, cx + erx, cy + ery));
                return path;

            case "line":
                path.MoveTo(GetLength(element, "x1"), GetLength(element, "y1"));
                path.LineTo(GetLength(element, "x2"), GetLength(element, "y2"));
                return path;

            case "polygon":
            case "polyline":
                var numbers = ParseNumbers((string?)element.Attribute("points") ?? string.Empty);
                if (numbers.Count < 4)
                {
                    return null;
                }

                var points = new SKPoint[numbers.Count / 2];
                for (var i = 0; i < points.Length; i++)
                {
                    points[i] = new SKPoint(numbers[i * 2], numbers[i * 2 + 1]);
                }

                path.AddPoly(points, element.Name.LocalName == "polygon");
                return path;
        }

        path.Dispose();
        return null;
    }

    private static SKMatrix GetFitMatrix(XElement root, List<Painted> painted, int canvasWidth, int canvasHeight)
    {
        SKRect source;
        var viewBox = ParseNumbers((string?)root.Attribute("viewBox") ?? string.Empty);
        if (viewBox.Count == 4 && viewBox[2] > 0 && viewBox[3] > 0)
        {
            source = SKRect.Create(viewBox[0], viewBox[1], viewBox[2], viewBox[3]);
        }
        else if (GetLength(root, "width") > 0 && GetLength(root, "height") > 0 &&
                 !((string?)root.Attribute("width") ?? string.Empty).Contains('%'))
        {
            source = SKRect.Create(0, 0, GetLength(root, "width"), GetLength(root, "height"));
        }
        else
        {
            source = painted[0].Path.Bounds;
            foreach (var item in painted.Skip(1))
            {
                source.Union(item.Path.Bounds);
            }
        }

        if (source.Width <= 0 || source.Height <= 0)
        {
            return SKMatrix.CreateTranslation(-source.Left, -source.Top);
        }

        var sourceAspect = source.Width / source.Height;
        var canvasAspect = canvasWidth / (float)canvasHeight;
        var fraction = Math.Abs(sourceAspect - canvasAspect) / canvasAspect <= FullFrameAspectTolerance ? 1f : FitFraction;
        var scale = Math.Min(canvasWidth * fraction / source.Width, canvasHeight * fraction / source.Height);
        var offsetX = (canvasWidth - source.Width * scale) / 2f - source.Left * scale;
        var offsetY = (canvasHeight - source.Height * scale) / 2f - source.Top * scale;
        return new SKMatrix(scale, 0, offsetX, 0, scale, offsetY, 0, 0, 1);
    }

    private static List<Contour> ReadContours(SKPath path)
    {
        var contours = new List<Contour>();
        Contour? current = null;
        using var iterator = path.CreateRawIterator();
        var points = new SKPoint[4];
        SKPathVerb verb;
        while ((verb = iterator.Next(points)) != SKPathVerb.Done)
        {
            switch (verb)
            {
                case SKPathVerb.Move:
                    current = new Contour { Start = points[0] };
                    contours.Add(current);
                    break;

                case SKPathVerb.Line when current != null:
                    current.Segments.Add(new Segment { End = points[1] });
                    break;

                case SKPathVerb.Quad when current != null:
                    current.Segments.Add(QuadToCubic(points[0], points[1], points[2]));
                    break;

                case SKPathVerb.Conic when current != null:
                    current.Segments.Add(ConicToCubic(points[0], points[1], points[2], iterator.ConicWeight()));
                    break;

                case SKPathVerb.Cubic when current != null:
                    current.Segments.Add(new Segment { IsCubic = true, Control1 = points[1], Control2 = points[2], End = points[3] });
                    break;
            }
        }

        foreach (var contour in contours)
        {
            // ASSA closes every shape itself, so a final line back to the start is redundant
            if (contour.Segments.Count > 0 && !contour.Segments[^1].IsCubic && IsSamePoint(contour.Segments[^1].End, contour.Start))
            {
                contour.Segments.RemoveAt(contour.Segments.Count - 1);
            }
        }

        contours.RemoveAll(c => c.Segments.Count == 0 || (c.Segments.Count == 1 && !c.Segments[0].IsCubic) || Math.Abs(SignedArea(Flatten(c))) < 0.01f);
        return contours;
    }

    /// <summary>
    /// Outer contours one way, holes the other way, so the result renders the same with
    /// even-odd and non-zero filling (Skia's simplify output doesn't promise directions).
    /// </summary>
    private static void NormalizeDirections(List<Contour> contours)
    {
        var polygons = contours.Select(Flatten).ToList();
        for (var i = 0; i < contours.Count; i++)
        {
            var probe = polygons[i][0];
            var depth = 0;
            for (var j = 0; j < contours.Count; j++)
            {
                if (i != j && IsInside(polygons[j], probe))
                {
                    depth++;
                }
            }

            var clockwise = SignedArea(polygons[i]) > 0;
            var wantClockwise = depth % 2 == 0;
            if (clockwise != wantClockwise)
            {
                Reverse(contours[i]);
            }
        }
    }

    private static void Reverse(Contour contour)
    {
        var starts = new List<SKPoint> { contour.Start };
        foreach (var segment in contour.Segments)
        {
            starts.Add(segment.End);
        }

        var reversed = new List<Segment>();
        for (var i = contour.Segments.Count - 1; i >= 0; i--)
        {
            var segment = contour.Segments[i];
            reversed.Add(new Segment
            {
                IsCubic = segment.IsCubic,
                Control1 = segment.Control2,
                Control2 = segment.Control1,
                End = starts[i],
            });
        }

        contour.Start = starts[^1];
        contour.Segments = reversed;
    }

    private static List<SKPoint> Flatten(Contour contour)
    {
        var result = new List<SKPoint> { contour.Start };
        var previous = contour.Start;
        foreach (var segment in contour.Segments)
        {
            if (segment.IsCubic)
            {
                for (var step = 1; step <= 8; step++)
                {
                    var t = step / 8f;
                    var u = 1 - t;
                    result.Add(new SKPoint(
                        u * u * u * previous.X + 3 * u * u * t * segment.Control1.X + 3 * u * t * t * segment.Control2.X + t * t * t * segment.End.X,
                        u * u * u * previous.Y + 3 * u * u * t * segment.Control1.Y + 3 * u * t * t * segment.Control2.Y + t * t * t * segment.End.Y));
                }
            }
            else
            {
                result.Add(segment.End);
            }

            previous = segment.End;
        }

        return result;
    }

    private static float SignedArea(List<SKPoint> polygon)
    {
        var area = 0f;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            area += (polygon[j].X * polygon[i].Y) - (polygon[i].X * polygon[j].Y);
        }

        return area / 2f;
    }

    private static bool IsInside(List<SKPoint> polygon, SKPoint point)
    {
        var inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            var a = polygon[i];
            var b = polygon[j];
            if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static DrawShape ToDrawShape(Contour contour, int layer, Color color)
    {
        var shape = new DrawShape { Layer = layer, ForeColor = color };
        var firstType = contour.Segments[0].IsCubic ? DrawCoordinateType.BezierCurve : DrawCoordinateType.Line;
        shape.AddPoint(firstType, Round(contour.Start.X), Round(contour.Start.Y), DrawSettings.PointColor);
        foreach (var segment in contour.Segments)
        {
            if (segment.IsCubic)
            {
                shape.AddPoint(DrawCoordinateType.BezierCurveSupport1, Round(segment.Control1.X), Round(segment.Control1.Y), DrawSettings.PointHelperColor);
                shape.AddPoint(DrawCoordinateType.BezierCurveSupport2, Round(segment.Control2.X), Round(segment.Control2.Y), DrawSettings.PointHelperColor);
                shape.AddPoint(DrawCoordinateType.BezierCurve, Round(segment.End.X), Round(segment.End.Y), DrawSettings.PointColor);
            }
            else
            {
                shape.AddPoint(DrawCoordinateType.Line, Round(segment.End.X), Round(segment.End.Y), DrawSettings.PointColor);
            }
        }

        return shape;
    }

    private static float Round(float value) => MathF.Round(value, 1);

    private static bool IsSamePoint(SKPoint a, SKPoint b) => Math.Abs(a.X - b.X) < 0.01f && Math.Abs(a.Y - b.Y) < 0.01f;

    /// <summary>
    /// One cubic for a conic: control points at 4w/(3(1+w)) along the conic's tangents. Skia makes
    /// arcs of at most 90 degrees, where this is the usual circle approximation (k = 0.5523) - one
    /// bezier per quarter circle instead of the four the quad conversion produced.
    /// </summary>
    private static Segment ConicToCubic(SKPoint p0, SKPoint p1, SKPoint p2, float weight)
    {
        var k = 4f * weight / (3f * (1f + weight));
        return new Segment
        {
            IsCubic = true,
            Control1 = new SKPoint(p0.X + (p1.X - p0.X) * k, p0.Y + (p1.Y - p0.Y) * k),
            Control2 = new SKPoint(p2.X + (p1.X - p2.X) * k, p2.Y + (p1.Y - p2.Y) * k),
            End = p2,
        };
    }

    private static Segment QuadToCubic(SKPoint p0, SKPoint q, SKPoint p2)
    {
        return new Segment
        {
            IsCubic = true,
            Control1 = new SKPoint(p0.X + 2f / 3f * (q.X - p0.X), p0.Y + 2f / 3f * (q.Y - p0.Y)),
            Control2 = new SKPoint(p2.X + 2f / 3f * (q.X - p2.X), p2.Y + 2f / 3f * (q.Y - p2.Y)),
            End = p2,
        };
    }

    private static Dictionary<string, Dictionary<string, string>> ParseStyleSheets(XElement root)
    {
        var rules = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach (var style in root.Descendants().Where(e => e.Name.LocalName == "style"))
        {
            var css = CssCommentRegex.Replace(style.Value, string.Empty);
            foreach (Match match in CssRuleRegex.Matches(css))
            {
                var declarations = ParseDeclarations(match.Groups[2].Value);
                foreach (var selector in match.Groups[1].Value.Split(','))
                {
                    var trimmed = selector.Trim();
                    if (trimmed.Length < 2 || trimmed[0] != '.' || trimmed.IndexOfAny([' ', '>', ':', '[', '.'], 1) >= 0)
                    {
                        continue; // only simple ".class" selectors (what Illustrator/Inkscape write)
                    }

                    if (!rules.TryGetValue(trimmed[1..], out var existing))
                    {
                        existing = new Dictionary<string, string>(StringComparer.Ordinal);
                        rules[trimmed[1..]] = existing;
                    }

                    foreach (var pair in declarations)
                    {
                        existing[pair.Key] = pair.Value;
                    }
                }
            }
        }

        return rules;
    }

    /// <summary>
    /// Presentation attributes, overridden by class rules, overridden by the style attribute.
    /// </summary>
    private static Dictionary<string, string> GetProperties(XElement element, Dictionary<string, Dictionary<string, string>> classRules)
    {
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var attribute in element.Attributes())
        {
            if (attribute.Name.Namespace == XNamespace.None)
            {
                properties[attribute.Name.LocalName] = attribute.Value.Trim();
            }
        }

        var classes = (string?)element.Attribute("class");
        if (!string.IsNullOrWhiteSpace(classes))
        {
            foreach (var className in classes.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (classRules.TryGetValue(className, out var rule))
                {
                    foreach (var pair in rule)
                    {
                        properties[pair.Key] = pair.Value;
                    }
                }
            }
        }

        var style = (string?)element.Attribute("style");
        if (!string.IsNullOrWhiteSpace(style))
        {
            foreach (var pair in ParseDeclarations(style))
            {
                properties[pair.Key] = pair.Value;
            }
        }

        return properties;
    }

    private static Dictionary<string, string> ParseDeclarations(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var declaration in text.Split(';'))
        {
            var colon = declaration.IndexOf(':');
            if (colon <= 0)
            {
                continue;
            }

            var value = declaration[(colon + 1)..].Replace("!important", string.Empty).Trim();
            result[declaration[..colon].Trim()] = value;
        }

        return result;
    }

    private static bool TryGetPaintColor(string paint, Context context, Dictionary<string, XElement> ids, out Color color)
    {
        color = Colors.Black;
        paint = paint.Trim();
        if (paint.Length == 0 || paint == "none" || paint == "transparent")
        {
            return false;
        }

        if (paint.StartsWith("url(", StringComparison.Ordinal))
        {
            // Gradients/patterns: use the first stop color, or the fallback after the url
            var close = paint.IndexOf(')');
            var id = close > 4 ? paint[4..close].Trim().Trim('\'', '"').TrimStart('#') : string.Empty;
            var fallback = close > 0 ? paint[(close + 1)..].Trim() : string.Empty;
            return TryGetGradientColor(id, ids, out color) || (fallback.Length > 0 && TryParseColor(fallback, context, out color));
        }

        return TryParseColor(paint, context, out color);
    }

    private static bool TryGetGradientColor(string id, Dictionary<string, XElement> ids, out Color color)
    {
        color = Colors.Black;
        for (var hops = 0; hops < 4 && ids.TryGetValue(id, out var gradient); hops++)
        {
            var stop = gradient.Elements().FirstOrDefault(e => e.Name.LocalName == "stop");
            if (stop != null)
            {
                var properties = GetProperties(stop, new Dictionary<string, Dictionary<string, string>>());
                if (!TryParseColor(properties.GetValueOrDefault("stop-color", "black"), new Context(), out color))
                {
                    return false;
                }

                var opacity = ParseOpacity(properties.GetValueOrDefault("stop-opacity", "1"));
                color = WithAlpha(color, opacity);
                return true;
            }

            id = ((string?)gradient.Attribute("href") ?? (string?)gradient.Attribute(XName.Get("href", "http://www.w3.org/1999/xlink")) ?? string.Empty).TrimStart('#');
        }

        return false;
    }

    private static bool TryParseColor(string text, Context context, out Color color)
    {
        color = Colors.Black;
        text = text.Trim();
        if (text == "currentColor")
        {
            text = context.Properties.GetValueOrDefault("color", "black");
        }

        if (text.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            var open = text.IndexOf('(');
            var close = text.IndexOf(')');
            if (open < 0 || close < open)
            {
                return false;
            }

            var parts = text[(open + 1)..close].Split([',', ' ', '/'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return false;
            }

            static byte Channel(string part) => part.EndsWith('%')
                ? (byte)Math.Clamp(Math.Round(ParseFloat(part.TrimEnd('%'), 0) * 2.55), 0, 255)
                : (byte)Math.Clamp(Math.Round(ParseFloat(part, 0)), 0, 255);

            var alpha = parts.Length > 3
                ? parts[3].EndsWith('%') ? ParseFloat(parts[3].TrimEnd('%'), 100) / 100f : ParseFloat(parts[3], 1)
                : 1f;
            color = Color.FromArgb((byte)Math.Clamp(Math.Round(alpha * 255), 0, 255), Channel(parts[0]), Channel(parts[1]), Channel(parts[2]));
            return true;
        }

        if (text.StartsWith('#'))
        {
            if (text.Length == 4)
            {
                // #rgb shorthand
                text = $"#{text[1]}{text[1]}{text[2]}{text[2]}{text[3]}{text[3]}";
            }
            else if (text.Length == 5)
            {
                // #rgba shorthand -> #aarrggbb (Avalonia reads 8 hex digits as ARGB)
                text = $"#{text[4]}{text[4]}{text[1]}{text[1]}{text[2]}{text[2]}{text[3]}{text[3]}";
            }
            else if (text.Length == 9)
            {
                // CSS #rrggbbaa -> #aarrggbb
                text = $"#{text[7..9]}{text[1..7]}";
            }
        }
        else if (text.Contains("grey", StringComparison.OrdinalIgnoreCase))
        {
            // CSS also accepts the British spelling (grey, lightgrey, slategrey, ...)
            text = text.Replace("grey", "gray", StringComparison.OrdinalIgnoreCase);
        }

        return Color.TryParse(text, out color);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return Color.FromArgb((byte)Math.Clamp(Math.Round(color.A * alpha), 0, 255), color.R, color.G, color.B);
    }

    private static SKMatrix ParseTransform(string text)
    {
        var result = SKMatrix.Identity;
        foreach (Match match in TransformRegex.Matches(text))
        {
            var values = ParseNumbers(match.Groups[2].Value);
            SKMatrix next;
            switch (match.Groups[1].Value)
            {
                case "matrix" when values.Count == 6:
                    next = new SKMatrix(values[0], values[2], values[4], values[1], values[3], values[5], 0, 0, 1);
                    break;
                case "translate" when values.Count >= 1:
                    next = SKMatrix.CreateTranslation(values[0], values.Count > 1 ? values[1] : 0);
                    break;
                case "scale" when values.Count >= 1:
                    next = SKMatrix.CreateScale(values[0], values.Count > 1 ? values[1] : values[0]);
                    break;
                case "rotate" when values.Count >= 3:
                    next = SKMatrix.CreateRotationDegrees(values[0], values[1], values[2]);
                    break;
                case "rotate" when values.Count >= 1:
                    next = SKMatrix.CreateRotationDegrees(values[0]);
                    break;
                case "skewX" when values.Count >= 1:
                    next = SKMatrix.CreateSkew(MathF.Tan(values[0] * MathF.PI / 180f), 0);
                    break;
                case "skewY" when values.Count >= 1:
                    next = SKMatrix.CreateSkew(0, MathF.Tan(values[0] * MathF.PI / 180f));
                    break;
                default:
                    continue;
            }

            result = result.PreConcat(next);
        }

        return result;
    }

    private static float GetLength(XElement element, string attribute)
    {
        var text = (string?)element.Attribute(attribute);
        if (string.IsNullOrEmpty(text) || text.Contains('%'))
        {
            return 0;
        }

        var match = NumberRegex.Match(text);
        if (!match.Success)
        {
            return 0;
        }

        var value = float.Parse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        var unit = text[(match.Index + match.Length)..].Trim();
        return unit switch
        {
            "pt" => value * 4f / 3f,
            "pc" => value * 16f,
            "mm" => value * 96f / 25.4f,
            "cm" => value * 96f / 2.54f,
            "in" => value * 96f,
            _ => value,
        };
    }

    private static List<float> ParseNumbers(string text)
    {
        var result = new List<float>();
        foreach (Match match in NumberRegex.Matches(text))
        {
            if (float.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                result.Add(value);
            }
        }

        return result;
    }

    private static float ParseOpacity(string text)
    {
        var value = ParseFloat(text, 1);
        if (text != null && text.TrimEnd().EndsWith('%'))
        {
            value /= 100f;
        }

        return Math.Clamp(value, 0, 1);
    }

    private static float ParseFloat(string text, float defaultValue)
    {
        var match = NumberRegex.Match(text ?? string.Empty);
        return match.Success && float.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : defaultValue;
    }
}
