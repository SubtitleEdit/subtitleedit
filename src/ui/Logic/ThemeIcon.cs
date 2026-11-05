using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using SkiaSharp;
using Svg.Skia;
using System.IO;

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
