using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class OcrEnginesTest : IDisposable
{
    private readonly string _tempRoot;

    public OcrEnginesTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "OcrEngines_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static ConversionOptions Opts(string engine, string? ocrDb = null) => new()
    {
        Patterns = ["dummy.sup"],
        Format = "SubRip",
        OcrEngine = engine,
        OcrLanguage = "eng",
        OcrDb = ocrDb,
    };

    [Fact]
    public void Factory_NOcrWithoutOcrDb_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => OcrEngineFactory.Create(Opts("nocr")));
        Assert.Contains("--ocrdb", ex.Message);
    }

    [Fact]
    public void Factory_NOcrMissingFile_Throws()
    {
        var ex = Assert.Throws<FileNotFoundException>(() =>
            OcrEngineFactory.Create(Opts("nocr", Path.Combine(_tempRoot, "missing.nocr"))));
        Assert.Contains("missing.nocr", ex.Message);
    }

    [Fact]
    public void Factory_NOcrAutoAppendsExtension()
    {
        // When --ocrdb is "Latin" (no extension), factory appends ".nocr" before checking
        var ex = Assert.Throws<FileNotFoundException>(() =>
            OcrEngineFactory.Create(Opts("nocr", Path.Combine(_tempRoot, "Latin"))));
        Assert.Contains("Latin.nocr", ex.Message);
    }

    [Fact]
    public void Factory_UnknownEngine_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => OcrEngineFactory.Create(Opts("nope")));
        Assert.Contains("nope", ex.Message);
        Assert.Contains("tesseract", ex.Message);
        Assert.Contains("nocr", ex.Message);
        Assert.Contains("ollama", ex.Message);
        Assert.Contains("paddle", ex.Message);
    }

    [Fact]
    public void Factory_TesseractRouted()
    {
        // If Tesseract is installed, this succeeds. Otherwise InvalidOperationException
        // with the install hint.
        if (TesseractOcrEngine.Detect() is null)
        {
            Assert.Throws<InvalidOperationException>(() => OcrEngineFactory.Create(Opts("tesseract")));
        }
        else
        {
            using var engine = OcrEngineFactory.Create(Opts("tesseract"));
            Assert.Equal("tesseract", engine.Name);
        }
    }

    [Fact]
    public void Factory_PaddleRouted()
    {
        if (PaddleOcrEngine.Detect() is null)
        {
            Assert.Throws<InvalidOperationException>(() => OcrEngineFactory.Create(Opts("paddle")));
        }
        else
        {
            using var engine = OcrEngineFactory.Create(Opts("paddle"));
            Assert.Equal("paddleocr", engine.Name);
        }
    }

    [Fact]
    public void Factory_OllamaConstructionAlwaysSucceeds()
    {
        // Ollama doesn't probe at construction time — it's an HTTP client.
        // Bad URL only fails when Recognize() is called.
        using var engine = OcrEngineFactory.Create(Opts("ollama"));
        Assert.Equal("ollama", engine.Name);
    }

    [Fact]
    public void Paddle_ParseStdout_ExtractsTextLines()
    {
        // Real paddleocr output sample
        var stdout = """
            [[10, 20], [100, 20], [100, 40], [10, 40]] ('Hello world', 0.95)
            [[10, 50], [100, 50], [100, 70], [10, 70]] ('second line', 0.91)
            """;
        var text = PaddleOcrEngine.ParseStdout(stdout);
        Assert.Contains("Hello world", text);
        Assert.Contains("second line", text);
    }

    [Fact]
    public void Paddle_ParseStdout_NoMatches_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, PaddleOcrEngine.ParseStdout("no recognized text here"));
        Assert.Equal(string.Empty, PaddleOcrEngine.ParseStdout(""));
    }
}
