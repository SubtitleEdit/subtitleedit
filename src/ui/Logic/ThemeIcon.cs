using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using SkiaSharp;
using Svg.Skia;
using Nikse.SubtitleEdit.Logic.Config;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Loads a named icon from a theme's image folder. An icon set is either SVG or PNG, so a
/// <c>{name}.svg</c> wins and <c>{name}.png</c> is the fallback - that also covers folders
/// unpacked before their theme had SVG icons.
/// </summary>
public static class ThemeIcon
{
    // Large enough to stay sharp when a recolored icon is shown at any toolbar size
    private const int RasterSize = 128;

    /// <summary>
    /// Layout size of an SVG icon. Pixels from <see cref="LoadPixels"/> are larger, so an image
    /// showing them needs this size set instead of taking it from the bitmap.
    /// </summary>
    public const int LayoutSize = 32;

    public static bool Exists(string folder, string name)
    {
        return File.Exists(Path.Combine(folder, name + ".svg")) || File.Exists(Path.Combine(folder, name + ".png"));
    }

    public static bool IsSvg(string folder, string name) => File.Exists(Path.Combine(folder, name + ".svg"));

    public static IImage Load(string folder, string name)
    {
        var svgPath = Path.Combine(folder, name + ".svg");
        if (!File.Exists(svgPath))
        {
            return new Bitmap(Path.Combine(folder, name + ".png"));
        }

        using var stream = File.OpenRead(svgPath);
        return new SvgImage { Source = SvgSource.LoadFromStream(stream) };
    }

    /// <summary>
    /// The icon as the user wants it shown: as drawn, or in the theme's text color when
    /// "Match icon color to theme foreground color" is on.
    /// </summary>
    public static IImage LoadForTheme(string folder, string name)
    {
        return Se.Settings.Appearance.MatchIconColorToTheme
            ? LoadRecolored(folder, name, UiTheme.GetThemeForegroundColor())
            : Load(folder, name);
    }

    /// <summary>
    /// The icon drawn in one color, for "Match icon color to theme foreground color". Works on
    /// every icon set: in an SVG the strokes take the color and a fill behind a stroke becomes a
    /// faint wash of it, so filled sets (Pastel, Nord, Classic) keep their shapes; a PNG becomes
    /// a silhouette in the color.
    /// </summary>
    public static IImage LoadRecolored(string folder, string name, Color color)
    {
        var svgPath = Path.Combine(folder, name + ".svg");
        if (!File.Exists(svgPath))
        {
            using var decoded = SKBitmap.Decode(Path.Combine(folder, name + ".png"));
            using var tinted = new SKBitmap(new SKImageInfo(decoded.Width, decoded.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
            using (var canvas = new SKCanvas(tinted))
            using (var paint = new SKPaint { ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(color.R, color.G, color.B), SKBlendMode.SrcIn) })
            {
                canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(decoded, 0, 0, paint);
            }

            return tinted.ToAvaloniaBitmap();
        }

        var svg = RecolorSvg(File.ReadAllText(svgPath), color);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        return new SvgImage { Source = SvgSource.LoadFromStream(stream) };
    }

    // How strongly a fill under a stroke shows through after recoloring
    private const string WashOpacity = "0.25";

    private static readonly Regex ElementRegex = new(@"<[a-zA-Z][^>]*>", RegexOptions.Compiled);
    private static readonly Regex StrokeRegex = new(@"\bstroke=""(?!none"")[^""]*""", RegexOptions.Compiled);
    private static readonly Regex StopColorRegex = new(@"\bstop-color=""[^""]*""", RegexOptions.Compiled);
    private static readonly Regex FillColorRegex = new(@"\bfill=""#[0-9A-Fa-f]+""", RegexOptions.Compiled);
    private static readonly Regex FillRegex = new(@"\bfill=""(?!none"")[^""]*""", RegexOptions.Compiled);

    internal static string RecolorSvg(string svg, Color color)
    {
        var hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        return ElementRegex.Replace(svg, m =>
        {
            var element = m.Value;
            var hasStroke = StrokeRegex.IsMatch(element);
            element = StrokeRegex.Replace(element, $"stroke=\"{hex}\"");
            element = StopColorRegex.Replace(element, $"stop-color=\"{hex}\"");
            element = FillColorRegex.Replace(element, $"fill=\"{hex}\"");

            // A fill under a stroke (a folder body, a page) in the stroke's color would merge the
            // shape into a blob, so it becomes a wash. A gradient fill (Classic) gets the color
            // through its stops.
            if (hasStroke && FillRegex.IsMatch(element) && !element.Contains("fill-opacity="))
            {
                var end = element.EndsWith("/>") ? element.Length - 2 : element.Length - 1;
                element = element.Insert(end, $" fill-opacity=\"{WashOpacity}\"");
            }

            return element;
        });
    }

    /// <summary>
    /// The icon as pixels, for callers that recolor it. An SVG is rendered at a fixed size.
    /// </summary>
    public static SKBitmap LoadPixels(string folder, string name)
    {
        var svgPath = Path.Combine(folder, name + ".svg");
        if (!File.Exists(svgPath))
        {
            return SKBitmap.Decode(Path.Combine(folder, name + ".png"));
        }

        using var svg = new SKSvg();
        svg.Load(svgPath);
        var picture = svg.Picture ?? throw new InvalidDataException($"Could not read \"{svgPath}\"");

        var bitmap = new SKBitmap(new SKImageInfo(RasterSize, RasterSize, SKColorType.Bgra8888, SKAlphaType.Premul));
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(RasterSize / picture.CullRect.Width, RasterSize / picture.CullRect.Height);
        canvas.DrawPicture(picture);
        canvas.Flush();
        return bitmap;
    }
}
