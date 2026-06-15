using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Routing for <c>.sub</c> inputs without an <c>.idx</c> companion: a binary VobSub stream
/// must be recognised as VobSub (read directly, with a default palette) and a text MicroDVD
/// <c>.sub</c> must still fall through to the text loader.
/// </summary>
public class VobSubRoutingTest : IDisposable
{
    private readonly string _tempRoot;

    public VobSubRoutingTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "VobRoute_" + Guid.NewGuid().ToString("N"));
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
    public void IsBinaryVobSub_TrueForMpegPackHeader_FalseForText()
    {
        // Binary VobSub starts with the MPEG-2 pack header 00 00 01 BA.
        var binary = Path.Combine(_tempRoot, "binary.sub");
        File.WriteAllBytes(binary, [0x00, 0x00, 0x01, 0xBA, 0x44, 0x00, 0x04, 0x00]);
        Assert.True(BitmapSubtitleLoader.IsBinaryVobSub(binary));

        // Text MicroDVD starts with '{'.
        var text = Path.Combine(_tempRoot, "text.sub");
        File.WriteAllText(text, "{0}{25}Hello world");
        Assert.False(BitmapSubtitleLoader.IsBinaryVobSub(text));
    }

    [Fact]
    public async Task TextMicroDvdSub_WithoutIdx_StillConvertsAsText()
    {
        // A MicroDVD .sub with no .idx must not be mistaken for VobSub — it should convert
        // as text, preserving its content.
        var input = Path.Combine(_tempRoot, "movie.sub");
        await File.WriteAllTextAsync(
            input,
            "{0}{25}Hello world" + Environment.NewLine + "{26}{50}Second cue",
            TestContext.Current.CancellationToken);

        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            Fps = 25,
            OutputFolder = outputFolder,
            Overwrite = true,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var srt = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Single(srt);
        var content = await File.ReadAllTextAsync(srt[0], TestContext.Current.CancellationToken);
        Assert.Contains("Hello world", content);
        Assert.Contains("Second cue", content);
    }
}
