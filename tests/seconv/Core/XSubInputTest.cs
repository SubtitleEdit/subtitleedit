using SeConv.Core;
using System.Text.RegularExpressions;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Verifies the .avi / XSUB ("DivX subtitles") input path. The fixture is a 10 second 640x480
/// clip with three XSUB events; the assertions avoid OCR (which needs an engine installed) by
/// going through --time-codes-only and through the image-to-image path.
/// </summary>
public class XSubInputTest : IDisposable
{
    private readonly string _tempRoot;

    public XSubInputTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "XSub_" + Guid.NewGuid().ToString("N"));
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
    public async Task ConvertAsync_AviWithXSub_TimeCodesOnly_EmitsEveryCue()
    {
        var input = Fixtures.Path("container_xsub.avi");
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

        Assert.True(result.Success, string.Join("; ", result.Errors));

        var srt = Assert.Single(Directory.GetFiles(outputFolder, "*.srt"));

        // Single-stream files keep the plain output name (no track suffix).
        Assert.Equal("container_xsub.srt", Path.GetFileName(srt));

        var content = await File.ReadAllTextAsync(srt, TestContext.Current.CancellationToken);
        Assert.Equal(3, content.Split("-->").Length - 1);
        Assert.Contains("00:00:01,000 --> 00:00:02,991", content);
        Assert.Contains("00:00:07,000 --> 00:00:08,991", content);
    }

    [Fact]
    public async Task ConvertAsync_AviWithXSubToBluRaySup_PreservesBitmapsAndPlacement()
    {
        // Image target + image source: the XSUB bitmaps go straight into the .sup, keeping
        // the AVI's 640x480 frame and each caption's position instead of being re-centred in
        // the CLI's default resolution.
        var input = Fixtures.Path("container_xsub.avi");
        Assert.True(File.Exists(input), $"Fixture missing: {input}");
        var outputFolder = Path.Combine(_tempRoot, "sup");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "bluraysup",
            OutputFolder = outputFolder,
            Overwrite = true,
            // Deliberately not the source resolution - it must not be used.
            Resolution = (400, 300),
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));

        var sup = Assert.Single(Directory.GetFiles(outputFolder, "*.sup"));
        var log = new System.Text.StringBuilder();
        var pcsList = Nikse.SubtitleEdit.Core.BluRaySup.BluRaySupParser.ParseBluRaySup(sup, log);
        Assert.Equal(3, pcsList.Count);

        foreach (var pcs in pcsList)
        {
            var size = pcs.GetScreenSize();
            Assert.Equal(640, size.Width);
            Assert.Equal(480, size.Height);

            using var bitmap = pcs.GetBitmap();
            Assert.NotNull(bitmap);

            // The captions sit near the bottom of the frame, where they were in the .avi.
            var position = pcs.GetPosition();
            Assert.True(position.Top > size.Height / 2, $"caption drawn at y={position.Top}");
        }
    }

    [Fact]
    public async Task ConvertAsync_AviWithoutXSub_FailsWithAnXSubError()
    {
        // An .avi with no subtitle stream must say so rather than being misparsed as a text
        // subtitle by the fallback loader.
        var input = Path.Combine(_tempRoot, "no_subtitles.avi");
        await File.WriteAllBytesAsync(input, "RIFF____AVI not really a movie"u8.ToArray(), TestContext.Current.CancellationToken);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = _tempRoot,
            Overwrite = true,
            TimeCodesOnly = true,
        });

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => Regex.IsMatch(e, "XSUB", RegexOptions.IgnoreCase));
    }
}
