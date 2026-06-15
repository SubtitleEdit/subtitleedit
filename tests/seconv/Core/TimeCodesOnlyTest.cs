using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Verifies the --time-codes-only path: an image-based source (Blu-Ray .sup) converts to a
/// text format keeping its timing but with empty text, without running OCR. This must work
/// even when no OCR engine is installed, since recognition is skipped entirely.
/// </summary>
public class TimeCodesOnlyTest : IDisposable
{
    private readonly string _tempRoot;

    public TimeCodesOnlyTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "TimeCodesOnly_" + Guid.NewGuid().ToString("N"));
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
    public async Task ConvertAsync_BluRaySupToSrt_TimeCodesOnly_EmitsTimingWithoutOcr()
    {
        var input = Fixtures.Path("sample.sup");
        Assert.True(File.Exists(input), $"Fixture missing: {input}");
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
            TimeCodesOnly = true,
        });

        // Succeeds with no OCR engine present — recognition is skipped.
        Assert.True(result.Success, string.Join("; ", result.Errors));

        var srt = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Single(srt);
        var content = await File.ReadAllTextAsync(srt[0], TestContext.Current.CancellationToken);

        // Every cue keeps its time code (the "-->" separator) ...
        var cueCount = content.Split("-->").Length - 1;
        Assert.True(cueCount > 0, "expected at least one time-coded cue");

        // ... and carries no recognised text: only digits, time codes, arrows and
        // whitespace should appear. Any letter would mean OCR text leaked in.
        Assert.DoesNotContain(content, c => char.IsLetter(c));
    }
}
