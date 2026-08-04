using Nikse.SubtitleEdit.Core.BluRaySup;
using SeConv.Core;
using System.Text;
using System.Text.RegularExpressions;
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

    // Three lines the export places in three different spots, so a re-export that drops the
    // source position (and re-centres everything at the bottom) cannot pass by accident.
    private const string PositionedAssContent = """
        [Script Info]
        ScriptType: v4.00+
        PlayResX: 1920
        PlayResY: 1080

        [V4+ Styles]
        Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
        Style: Default,Arial,48,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

        [Events]
        Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
        Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,{\an7\pos(100,100)}Top left corner
        Dialogue: 0,0:00:03.00,0:00:04.00,Default,,0,0,0,,{\an5\pos(960,540)}Middle of the frame
        Dialogue: 0,0:00:05.00,0:00:06.00,Default,,0,0,0,,{\an8}Top center

        """;

    [Fact]
    public async Task ConvertAsync_BluRaySupToBdnXml_PreservesSourcePositions()
    {
        var input = Path.Combine(_tempRoot, "positioned.ass");
        await File.WriteAllTextAsync(input, PositionedAssContent, TestContext.Current.CancellationToken);

        var supFolder = Path.Combine(_tempRoot, "sup");
        Directory.CreateDirectory(supFolder);
        var converter = new SubtitleConverter();
        var supResult = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "bluraysup",
            OutputFolder = supFolder,
            Overwrite = true,
            Resolution = (1920, 1080),
            ImageStyle = new ImageExportStyle(),
        });
        Assert.True(supResult.Success, string.Join("; ", supResult.Errors));
        var supFile = Assert.Single(Directory.GetFiles(supFolder, "*.sup"));

        var expected = BluRaySupParser.ParseBluRaySup(supFile, new StringBuilder())
            .Select(p => p.GetPosition())
            .ToList();
        Assert.Equal(3, expected.Count);
        // The three lines really are in three different places in the source
        Assert.Equal(3, expected.Select(p => p.Top).Distinct().Count());

        // Re-export the .sup as BDN-XML, which records each bitmap's X/Y
        var bdnFolder = Path.Combine(_tempRoot, "bdn2");
        Directory.CreateDirectory(bdnFolder);
        var bdnResult = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [supFile],
            Format = "bdnxml",
            OutputFolder = bdnFolder,
            Overwrite = true,
        });
        Assert.True(bdnResult.Success, string.Join("; ", bdnResult.Errors));

        var indexFile = Assert.Single(Directory.GetFiles(bdnFolder, "index.xml", SearchOption.AllDirectories));
        var index = await File.ReadAllTextAsync(indexFile, TestContext.Current.CancellationToken);
        var written = Regex.Matches(index, @"<Graphic[^>]*\sX=""(\d+)""\s+Y=""(\d+)""")
            .Select(m => (X: int.Parse(m.Groups[1].Value), Y: int.Parse(m.Groups[2].Value)))
            .ToList();

        Assert.Equal(expected.Count, written.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Left, written[i].X);
            Assert.Equal(expected[i].Top, written[i].Y);
        }
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

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:02,000
        Hello

        2
        00:00:03,000 --> 00:00:04,000
        World

        """;

    [Fact]
    public async Task ConvertAsync_VobSubToBdnXml_KeepsTheIdxFrameSize_NotDvdPal()
    {
        // A 1920x1080 VobSub - Subtitle Edit writes "size: 1920x1080" into the .idx - used to
        // come back as DVD PAL 720x576, which moves every subpicture inside a smaller frame.
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var vobFolder = Path.Combine(_tempRoot, "vob");
        Directory.CreateDirectory(vobFolder);
        var converter = new SubtitleConverter();
        var vobResult = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "vobsub",
            OutputFolder = vobFolder,
            Overwrite = true,
            Resolution = (1920, 1080),
            ImageStyle = new ImageExportStyle(),
        });
        Assert.True(vobResult.Success, string.Join("; ", vobResult.Errors));

        var subFile = Assert.Single(Directory.GetFiles(vobFolder, "*.sub"));
        var idxFile = Path.ChangeExtension(subFile, ".idx");
        Assert.Contains("size: 1920x1080",
            await File.ReadAllTextAsync(idxFile, TestContext.Current.CancellationToken));

        var bdnFolder = Path.Combine(_tempRoot, "bdn3");
        Directory.CreateDirectory(bdnFolder);
        var bdnResult = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [subFile],
            Format = "bdnxml",
            OutputFolder = bdnFolder,
            Overwrite = true,
        });
        Assert.True(bdnResult.Success, string.Join("; ", bdnResult.Errors));

        var indexFile = Assert.Single(Directory.GetFiles(bdnFolder, "index.xml", SearchOption.AllDirectories));
        var index = await File.ReadAllTextAsync(indexFile, TestContext.Current.CancellationToken);
        Assert.Contains("VideoFormat=\"1080p\"", index);
    }

    [Theory]
    [InlineData("size: 720x480\npalette: 000000", 720, 480)]
    [InlineData("SIZE: 1920x1080", 1920, 1080)]
    public void TryGetVobSubIdxScreenSize_SizeLine_IsParsed(string idxText, int width, int height)
    {
        Assert.Equal((width, height), BitmapSubtitleLoader.TryGetVobSubIdxScreenSize(idxText));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("palette: 000000")]
    [InlineData("size: banana")]
    public void TryGetVobSubIdxScreenSize_NoUsableSizeLine_ReturnsNull(string? idxText)
    {
        // null, not a guess - the caller knows whether PAL or NTSC is the right default
        Assert.Null(BitmapSubtitleLoader.TryGetVobSubIdxScreenSize(idxText));
    }
}
