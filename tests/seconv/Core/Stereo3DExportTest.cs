using Nikse.SubtitleEdit.UiLogic.Export;
using SeConv.Core;
using System.Text.RegularExpressions;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// <c>--mode-3d</c> / <c>--depth-3d</c>: SE4's 3D image export in seconv, for text → image and
/// for turning a 2D image track into a 3D one. BDN-XML records each image's size and position,
/// so the flat export is the reference for where the two eyes' copies must land.
/// </summary>
public class Stereo3DExportTest : IDisposable
{
    private readonly string _tempRoot;

    public Stereo3DExportTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Stereo3D_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello, World!

        """;

    private sealed record Graphic(int Width, int Height, int X, int Y);

    private async Task<ConversionResult> Convert(string input, string format, string outFolderName, ImageExportStyle style)
    {
        var outFolder = Path.Combine(_tempRoot, outFolderName);
        Directory.CreateDirectory(outFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
            Resolution = (1920, 1080),
            ImageStyle = style,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        return result;
    }

    private async Task<string> WriteSrt()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);
        return input;
    }

    private Graphic ReadBdnGraphic(string outFolderName)
    {
        var xmlFile = Assert.Single(Directory.GetFiles(Path.Combine(_tempRoot, outFolderName), "*.xml", SearchOption.AllDirectories));
        var element = Regex.Match(File.ReadAllText(xmlFile), "<Graphic[^>]*>").Value;
        Assert.False(string.IsNullOrEmpty(element), "no Graphic element in the BDN-XML");

        int Attribute(string name) => int.Parse(Regex.Match(element, "\\s" + name + "=\"(\\d+)\"").Groups[1].Value);
        return new Graphic(Attribute("Width"), Attribute("Height"), Attribute("X"), Attribute("Y"));
    }

    private static ImageExportStyle Style(Export3DMode mode = Export3DMode.None, int depth = 0) => new()
    {
        BottomTopMargin = 20,
        LeftRightMargin = 20,
        Mode3D = mode,
        Depth3D = depth,
    };

    [Fact]
    public async Task TextToImage_HalfSideBySide_PutsAHalfWidthCopyInEachHalf()
    {
        var input = await WriteSrt();
        await Convert(input, "bdnxml", "flat", Style());
        await Convert(input, "bdnxml", "sbs", Style(Export3DMode.HalfSideBySide));

        var flat = ReadBdnGraphic("flat");
        var sbs = ReadBdnGraphic("sbs");

        Assert.Equal(flat.X / 2, sbs.X);
        Assert.Equal(flat.Y, sbs.Y);
        Assert.Equal(flat.Height, sbs.Height);
        Assert.Equal(960 + (flat.Width + 1) / 2, sbs.Width);
    }

    [Fact]
    public async Task TextToImage_HalfTopBottom_PutsAHalfHeightCopyInEachHalf()
    {
        var input = await WriteSrt();
        await Convert(input, "bdnxml", "flat", Style());
        await Convert(input, "bdnxml", "tab", Style(Export3DMode.HalfTopBottom, depth: 6));

        var flat = ReadBdnGraphic("flat");
        var tab = ReadBdnGraphic("tab");

        Assert.Equal(flat.X - 6, tab.X);
        Assert.Equal(flat.Width + 12, tab.Width);
        Assert.Equal(flat.Y / 2, tab.Y);
        Assert.Equal(540 + (flat.Height + 1) / 2, tab.Height);
    }

    [Fact]
    public async Task ImageToImage_TurnsA2DSupInto3D_FromTheSourcePosition()
    {
        var input = await WriteSrt();
        await Convert(input, "bluraysup", "sup2d", Style());
        var sup = Assert.Single(Directory.GetFiles(Path.Combine(_tempRoot, "sup2d"), "*.sup"));

        await Convert(sup, "bdnxml", "flatFromSup", Style());
        await Convert(sup, "bdnxml", "sbsFromSup", Style(Export3DMode.HalfSideBySide, depth: 3));

        var flat = ReadBdnGraphic("flatFromSup");
        var sbs = ReadBdnGraphic("sbsFromSup");

        Assert.Equal(flat.X / 2 + 3, sbs.X);
        Assert.Equal(flat.Y, sbs.Y);
        Assert.Equal(960 + (flat.Width + 1) / 2 - 6, sbs.Width);
    }

    [Fact]
    public async Task DCinema_WarnsThatThe3DModeIsIgnored()
    {
        var input = await WriteSrt();
        var result = await Convert(input, "dcinemainterop", "dcinema", Style(Export3DMode.HalfSideBySide));

        Assert.Contains(result.Warnings, w => w.Contains("3D mode is not supported"));
    }

    [Fact]
    public async Task DepthWithoutMode_Warns()
    {
        var input = await WriteSrt();
        var result = await Convert(input, "bluraysup", "depthOnly", Style(depth: 5));

        Assert.Contains(result.Warnings, w => w.Contains("3D depth has no effect"));
    }

    [Theory]
    [InlineData("none", Export3DMode.None)]
    [InlineData("half-side-by-side", Export3DMode.HalfSideBySide)]
    [InlineData("HalfSideBySide", Export3DMode.HalfSideBySide)]
    [InlineData("sbs", Export3DMode.HalfSideBySide)]
    [InlineData("half-sbs", Export3DMode.HalfSideBySide)]
    [InlineData("half-top-bottom", Export3DMode.HalfTopBottom)]
    [InlineData("half-top/bottom", Export3DMode.HalfTopBottom)]
    [InlineData("tab", Export3DMode.HalfTopBottom)]
    [InlineData("half_ou", Export3DMode.HalfTopBottom)]
    public void TryParseMode3D_ValidInputs(string input, Export3DMode expected)
    {
        Assert.True(ImageExportStyle.TryParseMode3D(input, out var mode));
        Assert.Equal(expected, mode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("anaglyph")]
    [InlineData("full-sbs")]
    public void TryParseMode3D_InvalidInputs(string input)
    {
        Assert.False(ImageExportStyle.TryParseMode3D(input, out _));
    }
}
