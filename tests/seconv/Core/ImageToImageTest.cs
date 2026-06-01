using SeConv.Core;
using SkiaSharp;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Verifies the image-to-image conversion path: source bitmaps from a Blu-Ray .sup go
/// directly into the image output handler without OCR + re-render. The fixture
/// <c>sample.sup</c> is shared with <see cref="OcrTest"/>.
/// </summary>
public class ImageToImageTest : IDisposable
{
    private readonly string _tempRoot;

    public ImageToImageTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "ImgToImg_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ConvertAsync_BluRaySupToBdnXml_PreservesSourceBitmaps()
    {
        // sample.sup → BDN-XML through the preserve-bitmaps path. The output PNGs should
        // be the actual decoded subpicture bitmaps from the .sup, not text re-rendered at
        // Arial 50pt — so dimensions reflect the source, not seconv's default --resolution.
        var input = Fixtures.Path("sample.sup");
        Assert.True(File.Exists(input), $"Fixture missing: {input}");
        var outputFolder = Path.Combine(_tempRoot, "bdn");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "bdnxml",
            OutputFolder = outputFolder,
            Overwrite = true,
            // Pick a deliberately oddball default so we can confirm it's not used —
            // the .sup's PCS header carries the real source resolution.
            Resolution = (400, 300),
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));

        // BDN-XML emits PNGs and an index.xml referencing them.
        var pngs = Directory.GetFiles(outputFolder, "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);

        // Each PNG starts with the standard PNG signature.
        var firstBytes = new byte[8];
        using (var fs = File.OpenRead(pngs[0]))
        {
            Assert.Equal(8, fs.Read(firstBytes, 0, 8));
        }
        Assert.Equal(0x89, firstBytes[0]);
        Assert.Equal((byte)'P', firstBytes[1]);
        Assert.Equal((byte)'N', firstBytes[2]);
        Assert.Equal((byte)'G', firstBytes[3]);

        // A re-rendered text bitmap at 400x300 would be 400 wide. The sample.sup's source
        // resolution is larger — the BDN index records the source frame size, so check it
        // doesn't match the deliberately-wrong --resolution we passed in.
        var indexes = Directory.GetFiles(outputFolder, "index.xml", SearchOption.AllDirectories);
        Assert.Single(indexes);
        var index = await File.ReadAllTextAsync(indexes[0], TestContext.Current.CancellationToken);
        Assert.Contains("<BDN", index);
        Assert.DoesNotContain("400 300", index); // would indicate the --resolution leaked through
        Assert.DoesNotContain("VideoFormat=\"400x300\"", index);
    }

    [Fact]
    public async Task ConvertAsync_BluRaySupToSrt_StillRoutesToOcr_NotPreservePath()
    {
        // Text target → OCR path; the preserve path must not swallow this case. We don't
        // care whether OCR succeeds (Tesseract may not be installed in CI), only that the
        // image-to-image path is skipped — confirmed by the error referencing Tesseract
        // rather than image-writer plumbing.
        if (TesseractOcrEngine.Detect() is not null)
        {
            return;
        }

        var input = Fixtures.Path("sample.sup");
        Assert.True(File.Exists(input));
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
        });

        Assert.False(result.Success);
        Assert.Contains("Tesseract not found on PATH", result.Errors[0]);
    }
}
