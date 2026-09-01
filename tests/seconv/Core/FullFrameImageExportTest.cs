using Nikse.SubtitleEdit.Core.BluRaySup;
using SeConv.Core;
using SkiaSharp;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// "Full frame image" in seconv (issue #14376): the subtitle is drawn onto a canvas the size of
/// the video frame instead of a bitmap cropped to the text, so every image can be placed at 0,0
/// in an editing timeline. Only the FCP and Blu-Ray sup handlers act on it; the rest warn.
/// </summary>
public class FullFrameImageExportTest : IDisposable
{
    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;

    private readonly string _tempRoot;

    public FullFrameImageExportTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "FullFrame_" + Guid.NewGuid().ToString("N"));
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

    private async Task<ConversionResult> Convert(string format, string outFolderName, ImageExportStyle style)
    {
        var input = Path.Combine(_tempRoot, outFolderName + ".srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);
        var outFolder = Path.Combine(_tempRoot, outFolderName);
        Directory.CreateDirectory(outFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
            Resolution = (ScreenWidth, ScreenHeight),
            ImageStyle = style,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        return result;
    }

    private string[] PngsIn(string outFolderName) =>
        Directory.GetFiles(Path.Combine(_tempRoot, outFolderName), "*.png", SearchOption.AllDirectories);

    [Fact]
    public async Task FcpImage_FullFrame_WritesFrameSizedPngs()
    {
        await Convert("fcpimage", "fcpfull", new ImageExportStyle { IsFullFrame = true });

        var png = Assert.Single(PngsIn("fcpfull"));
        using var bitmap = SKBitmap.Decode(png);
        Assert.Equal(ScreenWidth, bitmap.Width);
        Assert.Equal(ScreenHeight, bitmap.Height);
    }

    [Fact]
    public async Task FcpImage_WithoutFullFrame_WritesCroppedPngs()
    {
        // The other half of the pair: without the option the png stays cropped to the text,
        // which is what the xmeml's frame-sized <width>/<height> is measured against.
        await Convert("fcpimage", "fcpcrop", new ImageExportStyle());

        var png = Assert.Single(PngsIn("fcpcrop"));
        using var bitmap = SKBitmap.Decode(png);
        Assert.True(bitmap.Width < ScreenWidth, $"expected a cropped bitmap, got {bitmap.Width}x{bitmap.Height}");
        Assert.True(bitmap.Height < ScreenHeight, $"expected a cropped bitmap, got {bitmap.Width}x{bitmap.Height}");
    }

    [Fact]
    public async Task FcpImage_FullFrame_DefaultBackgroundIsTransparent()
    {
        await Convert("fcpimage", "fcpalpha", new ImageExportStyle { IsFullFrame = true });

        var png = Assert.Single(PngsIn("fcpalpha"));
        using var bitmap = SKBitmap.Decode(png);

        // SKColors.Transparent is #00FFFFFF, but a cleared premultiplied bitmap reads
        // #00000000 - so assert on the alpha channel, not on colour equality.
        Assert.Equal(0, bitmap.GetPixel(0, 0).Alpha);
        Assert.Equal(0, bitmap.GetPixel(ScreenWidth - 1, ScreenHeight - 1).Alpha);
    }

    [Fact]
    public async Task FcpImage_FullFrameBackgroundColor_PaintsTheWholeFrame()
    {
        Assert.True(ImageExportStyle.TryParseColor("#FF0000FF", out var opaqueBlue));
        await Convert("fcpimage", "fcpbg", new ImageExportStyle
        {
            IsFullFrame = true,
            FullFrameBackgroundColor = opaqueBlue,
        });

        var png = Assert.Single(PngsIn("fcpbg"));
        using var bitmap = SKBitmap.Decode(png);

        var corner = bitmap.GetPixel(0, 0);
        Assert.Equal(255, corner.Alpha);
        Assert.Equal(0, corner.Red);
        Assert.Equal(0, corner.Green);
        Assert.Equal(255, corner.Blue);
    }

    [Fact]
    public async Task FcpImage_FullFrameBackgroundColor_DoesNotFollowTheTextBoxColour()
    {
        // The full frame background is its own field: reusing --background-color (the box behind
        // the text) would paint the whole frame instead of the box.
        Assert.True(ImageExportStyle.TryParseColor("#FF000000", out var black));
        await Convert("fcpimage", "fcpboxonly", new ImageExportStyle
        {
            IsFullFrame = true,
            BackgroundColor = black,
        });

        var png = Assert.Single(PngsIn("fcpboxonly"));
        using var bitmap = SKBitmap.Decode(png);
        Assert.Equal(0, bitmap.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public async Task BluRaySup_FullFrame_SubtitleIsInAFrameSizedImageAtOrigin()
    {
        await Convert("bluraysup", "supfull", new ImageExportStyle { IsFullFrame = true });

        var supFile = Assert.Single(Directory.GetFiles(Path.Combine(_tempRoot, "supfull"), "*.sup"));
        var subtitle = Assert.Single(BluRaySupParser.ParseBluRaySup(supFile, new StringBuilder()));

        var position = subtitle.GetPosition();
        Assert.Equal(0, position.Left);
        Assert.Equal(0, position.Top);

        using var bitmap = subtitle.GetBitmap();
        Assert.Equal(ScreenWidth, bitmap.Width);
        Assert.Equal(ScreenHeight, bitmap.Height);

        // The text is really inside the frame, near the bottom - not a blank full frame.
        Assert.True(HasVisiblePixels(bitmap, ScreenHeight / 2, ScreenHeight), "no drawn pixels in the lower half of the full frame image");
        Assert.False(HasVisiblePixels(bitmap, 0, ScreenHeight / 2), "bottom-center text should not reach the upper half");
    }

    [Fact]
    public async Task BluRaySup_WithoutFullFrame_KeepsTheCroppedPlacement()
    {
        await Convert("bluraysup", "supcrop", new ImageExportStyle());

        var supFile = Assert.Single(Directory.GetFiles(Path.Combine(_tempRoot, "supcrop"), "*.sup"));
        var subtitle = Assert.Single(BluRaySupParser.ParseBluRaySup(supFile, new StringBuilder()));

        using var bitmap = subtitle.GetBitmap();
        Assert.True(bitmap.Width < ScreenWidth, $"expected a cropped bitmap, got {bitmap.Width}x{bitmap.Height}");
        Assert.True(subtitle.GetPosition().Top > ScreenHeight / 2, "a cropped bottom-center line is placed low in the frame");
    }

    [Fact]
    public async Task BdnXml_FullFrame_WarnsAndWritesCroppedPngs()
    {
        // Only FCP and Blu-Ray sup implement it - the other image handlers would silently write
        // cropped images, which on a command line (often from a shared --settings profile) is
        // worth a word.
        var result = await Convert("bdnxml", "bdnfull", new ImageExportStyle { IsFullFrame = true });

        Assert.Contains(result.Warnings, w => w.Contains("Full frame image is not supported", StringComparison.Ordinal));

        var png = Assert.Single(PngsIn("bdnfull"));
        using var bitmap = SKBitmap.Decode(png);
        Assert.True(bitmap.Width < ScreenWidth, "bdnxml ignores full frame");
    }

    [Fact]
    public async Task BluRaySupAndFcp_FullFrame_DoNotWarn()
    {
        var sup = await Convert("bluraysup", "supquiet", new ImageExportStyle { IsFullFrame = true });
        Assert.DoesNotContain(sup.Warnings, w => w.Contains("Full frame", StringComparison.Ordinal));

        var fcp = await Convert("fcpimage", "fcpquiet", new ImageExportStyle { IsFullFrame = true });
        Assert.DoesNotContain(fcp.Warnings, w => w.Contains("Full frame", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TextTarget_FullFrame_DoesNotWarn()
    {
        // A text target ignores every image styling option, not just this one; warning about a
        // settings profile's isFullFrame on every srt run would be noise.
        var result = await Convert("subrip", "srtout", new ImageExportStyle { IsFullFrame = true });
        Assert.Empty(result.Warnings);
    }

    private static bool HasVisiblePixels(SKBitmap bitmap, int fromY, int toY)
    {
        for (var y = fromY; y < toY; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
