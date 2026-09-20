using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class BinaryFormatRoundTripTest : IDisposable
{
    private readonly string _tempRoot;

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello, World!

        2
        00:00:05,000 --> 00:00:08,000
        This is a test subtitle.

        """;

    public BinaryFormatRoundTripTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "BinFmt_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Theory]
    [InlineData("pac", ".pac", 16)]
    [InlineData("ebustl", ".stl", 1024)]   // EBU STL has 1024-byte GSI block + TTI blocks
    [InlineData("dvbteletext", ".dvbttx", 1000)] // XML preamble + teletext PES payloads
    [InlineData("cavena890", ".890", 16)]
    [InlineData("cheetahcaption", ".cap", 16)]
    [InlineData("capmakerplus", ".cap", 16)]
    [InlineData("ayato", ".aya", 16)]
    public async Task ConvertAsync_SrtToBinary_ProducesNonEmptyFile(string format, string ext, int minBytes)
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = _tempRoot,
            Overwrite = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var outputs = Directory.GetFiles(_tempRoot, "in" + ext);
        Assert.Single(outputs);
        var size = new FileInfo(outputs[0]).Length;
        Assert.True(size >= minBytes, $"{format} output was only {size} bytes (expected >= {minBytes})");
    }

    [Fact]
    public async Task ConvertAsync_PacWithCodePage_RespectsCodePage()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "pac",
            OutputFolder = _tempRoot,
            Overwrite = true,
            PacCodePage = Nikse.SubtitleEdit.Core.SubtitleFormats.Pac.CodePageHebrew,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Single(Directory.GetFiles(_tempRoot, "in.pac"));
    }

    [Fact]
    public async Task ConvertAsync_DvbTeletextToSrt_ReadsTextAndColorsBack()
    {
        // Source detection for .dvbttx goes through the binary format list - a full round trip
        // proves both directions, including a Level 2.5 colour past the basic teletext eight.
        var input = Path.Combine(_tempRoot, "in.srt");
        var srt = """
            1
            00:00:01,000 --> 00:00:04,000
            <font color="#ff8822">Level 2.5 orange</font>

            """;
        await File.WriteAllTextAsync(input, srt, TestContext.Current.CancellationToken);

        var converter = new SubtitleConverter();
        var toDvbttx = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "dvbteletext",
            OutputFolder = _tempRoot,
            Overwrite = true,
        });
        Assert.True(toDvbttx.Success, string.Join("; ", toDvbttx.Errors));

        var backToSrt = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [Path.Combine(_tempRoot, "in.dvbttx")],
            Format = "subrip",
            OutputFolder = Path.Combine(_tempRoot, "back"),
            Overwrite = true,
        });
        Assert.True(backToSrt.Success, string.Join("; ", backToSrt.Errors));

        var text = await File.ReadAllTextAsync(Path.Combine(_tempRoot, "back", "in.srt"), TestContext.Current.CancellationToken);
        Assert.Contains("<font color=\"#ff8822\">Level 2.5 orange</font>", text);
    }

    [Fact]
    public async Task ConvertAsync_WebVttOutput_HasUtf8Bom()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "WebVTT",
            OutputFolder = _tempRoot,
            Overwrite = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var bytes = await File.ReadAllBytesAsync(Path.Combine(_tempRoot, "in.vtt"), TestContext.Current.CancellationToken);
        Assert.True(bytes.Length >= 3);
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);
    }
}
