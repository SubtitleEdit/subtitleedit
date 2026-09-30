using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class ImageOutputTest : IDisposable
{
    private readonly string _tempRoot;

    public ImageOutputTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "ImgOut_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello, World!

        2
        00:00:05,000 --> 00:00:08,000
        This is a test subtitle.

        """;

    private async Task<ConversionResult> ConvertTo(string format, string outFolderName, ImageExportStyle? style = null)
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent);
        var outFolder = Path.Combine(_tempRoot, outFolderName);
        Directory.CreateDirectory(outFolder);

        var converter = new SubtitleConverter();
        return await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
            Resolution = (1920, 1080),
            ImageStyle = style ?? new ImageExportStyle(),
        });
    }

    [Fact]
    public async Task ConvertAsync_BluRaySupOutput_ProducesNonEmptyBinaryFile()
    {
        var result = await ConvertTo("bluraysup", "sup");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var supFiles = Directory.GetFiles(Path.Combine(_tempRoot, "sup"), "*.sup");
        Assert.Single(supFiles);
        Assert.True(new FileInfo(supFiles[0]).Length > 1000, "Blu-Ray sup file should be > 1 KB for 2 paragraphs");
    }

    [Fact]
    public async Task ConvertAsync_BdnXmlOutput_ProducesPngsAndIndex()
    {
        var result = await ConvertTo("bdnxml", "bdn");
        Assert.True(result.Success, string.Join("; ", result.Errors));

        // BDN-XML emits to a subfolder named after the input stem; recurse to find PNGs.
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "bdn"), "*.png", SearchOption.AllDirectories);
        Assert.Equal(2, pngs.Length);

        // Each PNG starts with the standard PNG signature
        foreach (var png in pngs)
        {
            var sig = new byte[8];
            using var fs = File.OpenRead(png);
            Assert.Equal(8, fs.Read(sig, 0, 8));
            Assert.Equal(0x89, sig[0]);
            Assert.Equal((byte)'P', sig[1]);
            Assert.Equal((byte)'N', sig[2]);
            Assert.Equal((byte)'G', sig[3]);
        }

        var indexes = Directory.GetFiles(Path.Combine(_tempRoot, "bdn"), "index.xml", SearchOption.AllDirectories);
        Assert.Single(indexes);
        var index = await File.ReadAllTextAsync(indexes[0], TestContext.Current.CancellationToken);
        Assert.Contains("<BDN", index);
        Assert.Contains("<Event", index);
        Assert.Contains("0001.png", index);
        Assert.Contains("0002.png", index);
    }

    [Fact]
    public async Task ConvertAsync_FcpImageOutput_Succeeds()
    {
        var result = await ConvertTo("fcpimage", "fcp");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "fcp"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }

    [Fact]
    public async Task ConvertAsync_DostOutput_Succeeds()
    {
        var result = await ConvertTo("dost", "dost");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "dost"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }

    [Fact]
    public async Task ConvertAsync_DCinemaInteropOutput_Succeeds()
    {
        var result = await ConvertTo("dcinemainterop", "dci");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "dci"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }

    [Fact]
    public async Task ConvertAsync_ImagesWithTimeCodeOutput_Succeeds()
    {
        var result = await ConvertTo("imageswithtimecode", "tc");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "tc"), "*.png", SearchOption.AllDirectories);
        Assert.NotEmpty(pngs);
    }

    [Fact]
    public async Task ConvertAsync_BdnXmlWithBackgroundBox_RendersOpaqueBox()
    {
        // A visible background colour implies a one-box background (Fredrik's v4
        // "black background" workflow). With the default transparent background the
        // trimmed bitmap's corner is transparent; with a box it must be the box colour.
        Assert.True(ImageExportStyle.TryParseColor("#FF000000", out var black));
        var style = new ImageExportStyle { BackgroundColor = black };

        var result = await ConvertTo("bdnxml", "bdnbox", style);
        Assert.True(result.Success, string.Join("; ", result.Errors));

        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "bdnbox"), "*.png", SearchOption.AllDirectories);
        Assert.Equal(2, pngs.Length);

        using var bitmap = SkiaSharp.SKBitmap.Decode(pngs[0]);
        var corner = bitmap.GetPixel(0, 0);
        Assert.Equal(255, corner.Alpha);
        Assert.Equal(0, corner.Red);
        Assert.Equal(0, corner.Green);
        Assert.Equal(0, corner.Blue);
    }

    [Fact]
    public async Task ConvertAsync_BdnXmlDefaultStyle_HasTransparentCorner()
    {
        var result = await ConvertTo("bdnxml", "bdnplain");
        Assert.True(result.Success, string.Join("; ", result.Errors));

        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "bdnplain"), "*.png", SearchOption.AllDirectories);
        Assert.Equal(2, pngs.Length);

        // Bottom-left corner: the bottom edge of the trimmed bitmap comes from the
        // comma's descender (mid-string), so the leftmost column is transparent there.
        // The top-left corner is unsafe to probe — the 'H' outline may touch it.
        using var bitmap = SkiaSharp.SKBitmap.Decode(pngs[0]);
        Assert.Equal(0, bitmap.GetPixel(0, bitmap.Height - 1).Alpha);
    }

    [Fact]
    public async Task ConvertAsync_DvdSupOutput_IsReadableAsSpDvdSup()
    {
        var result = await ConvertTo("dvdsup", "dvdsup");
        Assert.True(result.Success, string.Join("; ", result.Errors));

        var supFiles = Directory.GetFiles(Path.Combine(_tempRoot, "dvdsup"), "*.sup");
        Assert.Single(supFiles);
        // The same check File > Open uses to route a .sup to the DVD sup OCR import.
        Assert.True(Nikse.SubtitleEdit.Core.Common.FileUtil.IsSpDvdSup(supFiles[0]));
    }

    [Fact]
    public async Task ConvertAsync_DvdSupInput_ReadsTimeCodesAndPassesImagesThrough()
    {
        var result = await ConvertTo("dvdsup", "dvdsup");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var supFile = Directory.GetFiles(Path.Combine(_tempRoot, "dvdsup"), "*.sup").Single();

        // Used to go to the Blu-ray loader and fail with "No Blu-Ray sup subtitles found".
        var srtFolder = Path.Combine(_tempRoot, "fromdvdsup");
        Directory.CreateDirectory(srtFolder);
        result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [supFile],
            Format = "subrip",
            OutputFolder = srtFolder,
            Overwrite = true,
            TimeCodesOnly = true,
        });
        Assert.True(result.Success, string.Join("; ", result.Errors));
        var srt = await File.ReadAllTextAsync(Directory.GetFiles(srtFolder, "*.srt").Single(), TestContext.Current.CancellationToken);
        // DVD stores the duration in 1024/90000 s steps, so 3 s comes back as 2.992 s.
        Assert.Contains("00:00:01,000 --> 00:00:03,99", srt);
        Assert.Contains("00:00:05,000 --> 00:00:07,99", srt);

        var bdnFolder = Path.Combine(_tempRoot, "bdnfromdvdsup");
        Directory.CreateDirectory(bdnFolder);
        result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [supFile],
            Format = "bdn-xml",
            OutputFolder = bdnFolder,
            Overwrite = true,
        });
        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(2, Directory.GetFiles(bdnFolder, "*.png", SearchOption.AllDirectories).Length);
    }

    [Fact]
    public async Task ConvertAsync_ImscImageOutput_WritesOnePngPerCueNextToTheTtml()
    {
        var result = await ConvertTo("imscimage", "imsc");
        Assert.True(result.Success, string.Join("; ", result.Errors));

        var folder = Path.Combine(_tempRoot, "imsc");
        var ttmlFiles = Directory.GetFiles(folder, "*.ttml");
        Assert.Single(ttmlFiles);
        var ttml = await File.ReadAllTextAsync(ttmlFiles[0], TestContext.Current.CancellationToken);
        Assert.Contains("http://www.w3.org/ns/ttml/profile/imsc1/image", ttml);
        Assert.DoesNotContain("smpte:image", ttml); // prohibited in the image profile
        Assert.Equal(2, Directory.GetFiles(folder, "*.png").Length);

        // SE reads it back with both cues, their timing and their png files.
        var lines = ttml.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        var format = new Nikse.SubtitleEdit.Core.SubtitleFormats.TimedTextImage();
        Assert.True(format.IsMine(lines, ttmlFiles[0]));
        var subtitle = new Nikse.SubtitleEdit.Core.Common.Subtitle();
        format.LoadSubtitle(subtitle, lines, ttmlFiles[0]);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(8000, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.All(subtitle.Paragraphs, p => Assert.True(File.Exists(Path.Combine(folder, p.Text)), p.Text));
    }

    [Fact]
    public async Task ConvertAsync_WebVttThumbnailOutput_ProducesPngsAndIndexVtt()
    {
        // WebVTT thumbnail bundle: the handler treats the output path as a folder
        // (creates it, drops 0001.png, 0002.png, ..., index.vtt inside). Verify both
        // the PNG sprites and the index.vtt cueing them are produced.
        var result = await ConvertTo("webvttthumbnail", "vttthumb");
        Assert.True(result.Success, string.Join("; ", result.Errors));

        var pngs = Directory.GetFiles(Path.Combine(_tempRoot, "vttthumb"), "*.png", SearchOption.AllDirectories);
        Assert.Equal(2, pngs.Length);
        Assert.Contains(pngs, p => Path.GetFileName(p) == "0001.png");
        Assert.Contains(pngs, p => Path.GetFileName(p) == "0002.png");

        var indexes = Directory.GetFiles(Path.Combine(_tempRoot, "vttthumb"), "index.vtt", SearchOption.AllDirectories);
        Assert.Single(indexes);
        var index = await File.ReadAllTextAsync(indexes[0], TestContext.Current.CancellationToken);
        Assert.StartsWith("WEBVTT", index);
        Assert.Contains("0001.png", index);
        Assert.Contains("0002.png", index);
    }
}
