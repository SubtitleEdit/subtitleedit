using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// --keep-timestamp (discussion #13963): outputs get the source file's modified date rather
/// than the conversion time. Off by default.
/// </summary>
public class KeepTimestampTest : IDisposable
{
    private readonly string _tempRoot;

    private const string SrtContent = """
        1
        00:00:01,000 --> 00:00:03,000
        First.

        """;

    public KeepTimestampTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "KeepTimestampTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private async Task<(string input, string output, DateTime sourceTime)> RunAsync(bool keepTimestamp)
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);
        var sourceTime = new DateTime(2019, 5, 17, 12, 34, 56, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(input, sourceTime);

        var outDir = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outDir);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "WebVTT",
            OutputFolder = outDir,
            Overwrite = true,
            KeepTimestamp = keepTimestamp,
        });
        Assert.True(result.Success, string.Join("; ", result.Errors));

        return (input, Path.Combine(outDir, "in.vtt"), sourceTime);
    }

    [Fact]
    public async Task KeepTimestamp_CopiesSourceLastWriteTime()
    {
        var (_, output, sourceTime) = await RunAsync(keepTimestamp: true);
        Assert.True(File.Exists(output));
        Assert.Equal(sourceTime, File.GetLastWriteTimeUtc(output));
    }

    [Fact]
    public async Task Default_OutputGetsCurrentTime()
    {
        var (_, output, sourceTime) = await RunAsync(keepTimestamp: false);
        Assert.True(File.Exists(output));
        Assert.NotEqual(sourceTime, File.GetLastWriteTimeUtc(output));
        Assert.True(File.GetLastWriteTimeUtc(output) > DateTime.UtcNow.AddMinutes(-5));
    }
}
