using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Behavioural tests for the VOB → .sub/.idx extraction path added for
/// subtitleedit-cli issue #15. Real VOBs are too large to ship as fixtures,
/// so the round-trip extraction itself is exercised end-to-end by users; here
/// we just lock down the user-visible behaviour around the new path:
///
///  - VOB inputs route to the new batch path (not the text loader)
///  - Wrong target format produces the helpful error message, not a
///    "input file too large" crash deep in the libse text path
/// </summary>
public class VobSubExtractorTest : IDisposable
{
    private readonly string _tempRoot;

    public VobSubExtractorTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Vob_" + Guid.NewGuid().ToString("N"));
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
    public async Task ConvertAsync_VobInput_NonVobSubTarget_ErrorsWithGuidance()
    {
        // Drop a tiny stub .vob — content doesn't matter, we never reach the parser
        // because the format check fires first.
        var fake = Path.Combine(_tempRoot, "fake.vob");
        await File.WriteAllBytesAsync(fake, new byte[] { 0, 1, 2, 3 }, TestContext.Current.CancellationToken);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [fake],
            Format = "SubRip",
            Overwrite = true,
        });

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Contains("VobSub", result.Errors[0]);
        Assert.Contains("--format VobSub", result.Errors[0]);
        // And critically: no "input file too large" crash from the text path.
        Assert.DoesNotContain("too large", result.Errors[0]);
    }

    [Fact]
    public async Task ConvertAsync_VobInput_WithVobSubTarget_AttemptsExtraction()
    {
        // A 4-byte stub isn't a valid MPEG-PS, so the VobSubParser will produce no
        // packs and we should see the friendly "no subpicture packets" error —
        // proving we reached the extraction path (and not the text-too-large guard).
        var fake = Path.Combine(_tempRoot, "fake.vob");
        await File.WriteAllBytesAsync(fake, new byte[] { 0, 1, 2, 3 }, TestContext.Current.CancellationToken);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [fake],
            Format = "VobSub",
            Overwrite = true,
        });

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Contains("VobSub", result.Errors[0]);
        Assert.Contains("VOB extraction failed", result.Errors[0]);
        // No "input file too large" leak.
        Assert.DoesNotContain("too large", result.Errors[0]);
    }
}
