using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Svg.Skia;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class ThemeIconTests : IDisposable
{
    private const string Svg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"32\" height=\"32\" viewBox=\"0 0 32 32\">" +
        "<rect x=\"8\" y=\"8\" width=\"16\" height=\"16\" fill=\"#FF0000\"/></svg>";

    private readonly string _folder = Path.Combine(Path.GetTempPath(), "ThemeIconTests_" + Guid.NewGuid());

    public ThemeIconTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, true);

    [AvaloniaFact]
    public void Exists_FindsSvgOnly()
    {
        File.WriteAllText(Path.Combine(_folder, "Open.svg"), Svg);

        Assert.True(ThemeIcon.Exists(_folder, "Open"));
        Assert.False(ThemeIcon.Exists(_folder, "Save"));
    }

    [AvaloniaFact]
    public void Load_Svg_KeepsLayoutSize()
    {
        File.WriteAllText(Path.Combine(_folder, "Open.svg"), Svg);

        var image = ThemeIcon.Load(_folder, "Open");

        Assert.IsType<SvgImage>(image);
        Assert.Equal(new Size(ThemeIcon.LayoutSize, ThemeIcon.LayoutSize), image.Size);
    }

    [AvaloniaFact]
    public void LoadPixels_Svg_RendersTheShape()
    {
        File.WriteAllText(Path.Combine(_folder, "Open.svg"), Svg);

        using var pixels = ThemeIcon.LoadPixels(_folder, "Open");

        Assert.Equal(255, pixels.GetPixel(pixels.Width / 2, pixels.Height / 2).Alpha);
        Assert.Equal(0, pixels.GetPixel(1, 1).Alpha);
    }
}
