using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Input patterns: a relative pattern with commas is a list ("a.srt,b.srt"), unless it names
/// an existing file - file names contain commas too.
/// </summary>
public class InputPatternTest : IDisposable
{
    private readonly string _tempRoot;

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello, World!

        """;

    public InputPatternTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "InputPattern_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private async Task<ConversionResult> Convert(string pattern) =>
        await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [pattern],
            InputFolder = _tempRoot,
            Format = "webvtt",
            OutputFolder = Path.Combine(_tempRoot, "out"),
        });

    [Fact]
    public async Task RelativeFileNameWithCommas_IsOneFile()
    {
        const string name = "00800 - 22 - Subtitle (PGS), English, 1792 captions.srt";
        File.WriteAllText(Path.Combine(_tempRoot, name), SrtContent);

        var result = await Convert(name);

        Assert.Empty(result.Errors);
        Assert.Equal(1, result.TotalFiles);
    }

    [Fact]
    public async Task CommaSeparatedList_StillSplit()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "a.srt"), SrtContent);
        File.WriteAllText(Path.Combine(_tempRoot, "b.srt"), SrtContent);

        var result = await Convert("a.srt,b.srt");

        Assert.Empty(result.Errors);
        Assert.Equal(2, result.TotalFiles);
    }
}
