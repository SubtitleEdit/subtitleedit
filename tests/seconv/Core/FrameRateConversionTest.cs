using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class FrameRateConversionTest : IDisposable
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

    public FrameRateConversionTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "FpsTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public async Task ConvertAsync_FpsAndTargetFps_ScalesTimesWithLibseRatio()
    {
        // 25 -> 23.976 should scale times by 25/23.976 ≈ 1.04270
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent);
        var outDir = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outDir);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outDir,
            Overwrite = true,
            Fps = 25.0,
            TargetFps = 23.976,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));

        var outFile = Path.Combine(outDir, "in.srt");
        Assert.True(File.Exists(outFile));
        var outText = await File.ReadAllTextAsync(outFile);

        // 1.000s * (25/23.976) ≈ 1.04270s -> "00:00:01,042"
        Assert.Contains("00:00:01,042", outText);
        // 5.000s * (25/23.976) ≈ 5.21354s -> "00:00:05,213"
        Assert.Contains("00:00:05,213", outText);
    }
}
