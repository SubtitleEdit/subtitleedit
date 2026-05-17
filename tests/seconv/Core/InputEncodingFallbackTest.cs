using System.Text;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class InputEncodingFallbackTest : IDisposable
{
    private readonly string _tempRoot;

    private const string SrtContentAscii = """
        1
        00:00:01,000 --> 00:00:03,000
        Hello.

        """;

    // Polish 'ą' is the unambiguous case: CP1250 encodes it as the single byte 0xB9, which
    // is > 127 yet not a valid UTF-8 lead byte, so LooksLikeUtf8 must return false and
    // the fallback must take over.
    private const string SrtContentCentralEuropean = """
        1
        00:00:01,000 --> 00:00:03,000
        Cześć ąęłóż.

        """;

    public InputEncodingFallbackTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "InputEncFallbackTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public void DetectEncodingWithFallback_Utf8Bom_ReturnsUtf8()
    {
        var path = Path.Combine(_tempRoot, "bom.srt");
        File.WriteAllText(path, SrtContentCentralEuropean, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var result = LibSEIntegration.DetectEncodingWithFallback(path, "windows-1250");

        Assert.Equal(Encoding.UTF8.WebName, result.WebName);
    }

    [Fact]
    public void DetectEncodingWithFallback_Utf8NoBomValidMultibyte_ReturnsUtf8()
    {
        var path = Path.Combine(_tempRoot, "utf8.srt");
        File.WriteAllText(path, SrtContentCentralEuropean, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var result = LibSEIntegration.DetectEncodingWithFallback(path, "windows-1250");

        Assert.Equal(Encoding.UTF8.WebName, result.WebName);
    }

    [Fact]
    public void DetectEncodingWithFallback_CentralEuropeanBytesNoBom_ReturnsFallback()
    {
        var path = Path.Combine(_tempRoot, "cp1250.srt");
        // Encoding.GetEncoding(1250) requires the code pages provider, which seconv's
        // module initializer wires up; tests get it via the LibSE bootstrap as well.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var cp1250 = Encoding.GetEncoding(1250);
        File.WriteAllBytes(path, cp1250.GetBytes(SrtContentCentralEuropean));

        var result = LibSEIntegration.DetectEncodingWithFallback(path, "windows-1250");

        Assert.Equal(1250, result.CodePage);
    }

    [Fact]
    public void DetectEncodingWithFallback_AsciiOnly_ReturnsFallback()
    {
        // Pure ASCII has no UTF-8 multi-byte sequences, so LooksLikeUtf8 returns false
        // and the fallback takes over. (Decoded text is identical either way for ASCII.)
        var path = Path.Combine(_tempRoot, "ascii.srt");
        File.WriteAllBytes(path, Encoding.ASCII.GetBytes(SrtContentAscii));

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var result = LibSEIntegration.DetectEncodingWithFallback(path, "windows-1250");

        Assert.Equal(1250, result.CodePage);
    }

    [Fact]
    public async Task ConvertAsync_CentralEuropeanInputWithFallback_OutputsCorrectUtf8()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var cp1250 = Encoding.GetEncoding(1250);

        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllBytesAsync(input, cp1250.GetBytes(SrtContentCentralEuropean), TestContext.Current.CancellationToken);
        var outDir = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outDir);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outDir,
            Overwrite = true,
            InputEncodingFallback = "windows-1250",
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));

        var outText = await File.ReadAllTextAsync(Path.Combine(outDir, "in.srt"), Encoding.UTF8, TestContext.Current.CancellationToken);
        Assert.Contains("Cześć ąęłóż.", outText);
    }
}
