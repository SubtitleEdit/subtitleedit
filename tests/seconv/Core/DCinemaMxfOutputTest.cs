using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class DCinemaMxfOutputTest : IDisposable
{
    private readonly string _tempRoot;

    public DCinemaMxfOutputTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "DcpMxf_" + Guid.NewGuid().ToString("N"));
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

    private async Task<(ConversionResult Result, string OutFolder)> Convert(string format, string? fontFile, string? reelDuration)
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent);
        var outFolder = Path.Combine(_tempRoot, "out_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outFolder);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
            Fps = 24,
            Resolution = (1998, 1080),
            DCinemaFontFile = fontFile,
            ReelDuration = reelDuration,
        });
        return (result, outFolder);
    }

    // ContainerDuration (local tag 3002) in the descriptor
    private static bool HasContainerDuration(string mxfFile, long frames)
    {
        var tag = new byte[] { 0x30, 0x02, 0x00, 0x08 }.Concat(BitConverter.GetBytes(frames).Reverse()).ToArray();
        return File.ReadAllBytes(mxfFile).AsSpan().IndexOf(tag) > 0;
    }

    [Fact]
    public async Task TextMxf_EmbedsFontAndXml()
    {
        var font = Path.Combine(_tempRoot, "font.ttf");
        await File.WriteAllBytesAsync(font, Enumerable.Range(0, 1000).Select(i => (byte)i).ToArray());

        var (result, outFolder) = await Convert("dcinemasmpte2014mxf", font, "00:00:12:00");

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var mxf = Assert.Single(Directory.GetFiles(outFolder, "*.mxf"));
        var xml = Assert.Single(new MxfParser(mxf).GetSubtitles());
        Assert.Contains("http://www.smpte-ra.org/schemas/428-7/2014/DCST", xml);
        Assert.Contains("Hello, World!", xml);
        Assert.Single(TimedTextMxfWriter.GetLoadFontIds(xml));
        Assert.True(HasContainerDuration(mxf, 288), "reel duration 12 s at 24 fps");
    }

    [Fact]
    public async Task TextMxf_WithoutFont_Fails()
    {
        var (result, outFolder) = await Convert("dcinema-smpte-mxf", null, null);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("--dcinema-font"));
        Assert.Empty(Directory.GetFiles(outFolder));
    }

    [Fact]
    public async Task PngMxf_EmbedsOneImagePerSubtitle()
    {
        var (result, outFolder) = await Convert("dcinemasmpte2014pngmxf", null, "00:00:12:00");

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var mxf = Assert.Single(Directory.GetFiles(outFolder, "*.mxf"));
        var parser = new MxfParser(mxf);
        Assert.Contains("<Image ", Assert.Single(parser.GetSubtitles()));
        Assert.Equal(2, parser.GetImages().Count);
        Assert.True(HasContainerDuration(mxf, 288), "reel duration 12 s at 24 fps");
    }

    [Fact]
    public async Task InvalidReelDuration_Fails()
    {
        var (result, _) = await Convert("dcinemasmpte2014pngmxf", null, "12 seconds");

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("--reel-duration"));
    }
}
