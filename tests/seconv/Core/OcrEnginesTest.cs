using Nikse.SubtitleEdit.UiLogic.Ocr.AppleVision;
using SeConv.Core;
using SkiaSharp;
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
        Assert.Contains("--ocr-db", ex.Message);
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
        // When --ocr-db is "Latin" (no extension), factory appends ".nocr" before checking
        var ex = Assert.Throws<FileNotFoundException>(() =>
            OcrEngineFactory.Create(Opts("nocr", Path.Combine(_tempRoot, "Latin"))));
        Assert.Contains("Latin.nocr", ex.Message);
    }

    [Fact]
    public void Factory_BinaryOcrWithoutOcrDb_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => OcrEngineFactory.Create(Opts("binaryocr")));
        Assert.Contains("--ocr-db", ex.Message);
        Assert.Contains(".db", ex.Message);
    }

    [Fact]
    public void Factory_BinaryOcrAutoAppendsDbExtension()
    {
        var ex = Assert.Throws<FileNotFoundException>(() =>
            OcrEngineFactory.Create(Opts("binaryocr", Path.Combine(_tempRoot, "Latin"))));
        Assert.Contains("Latin.db", ex.Message);
    }

    [Fact]
    public void Factory_UnknownEngine_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => OcrEngineFactory.Create(Opts("nope")));
        Assert.Contains("nope", ex.Message);
        Assert.Contains("tesseract", ex.Message);
        Assert.Contains("nocr", ex.Message);
        Assert.Contains("binaryocr", ex.Message);
        Assert.Contains("ollama", ex.Message);
        Assert.Contains("paddle", ex.Message);
        Assert.Contains("applevision", ex.Message);
    }

    [Fact]
    public void Factory_AppleVisionRouted()
    {
        if (!AppleVisionRecognizer.IsAvailable())
        {
            var ex = Assert.Throws<InvalidOperationException>(() => OcrEngineFactory.Create(Opts("applevision")));
            Assert.Contains("macOS", ex.Message);
        }
        else
        {
            // Opts() passes Tesseract's "eng", which has to land on Vision's own tag.
            using var engine = OcrEngineFactory.Create(Opts("apple-vision"));
            Assert.Equal("applevision", engine.Name);
            Assert.Equal("en-US", ((AppleVisionOcrEngine)engine).Language);
        }
    }

    [Theory]
    [InlineData(null, "en-US")]
    [InlineData("", "en-US")]
    [InlineData("de-DE", "de-DE")]
    [InlineData("DE-de", "de-DE")]
    [InlineData("de", "de-DE")]
    [InlineData("deu", "de-DE")]
    [InlineData("eng", "en-US")]
    [InlineData("German", "de-DE")]
    [InlineData("zh-Hant", "zh-Hant")]
    public void AppleVision_ResolveLanguage_MapsOntoVisionTags(string? requested, string expected)
    {
        Assert.Equal(expected, AppleVisionOcrEngine.ResolveLanguage(requested, VisionTags));
    }

    [Fact]
    public void AppleVision_ResolveLanguage_AmbiguousLanguageNamesTheCandidates()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AppleVisionOcrEngine.ResolveLanguage("zh", VisionTags));
        Assert.Contains("zh-Hans", ex.Message);
        Assert.Contains("zh-Hant", ex.Message);
    }

    [Fact]
    public void AppleVision_ResolveLanguage_UnknownLanguageListsWhatIsSupported()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AppleVisionOcrEngine.ResolveLanguage("Klingon", VisionTags));
        Assert.Contains("Klingon", ex.Message);
        Assert.Contains("en-US", ex.Message);
    }

    [Fact]
    public void AppleVision_ResolveLanguage_NoLanguageListLeavesTheChoiceToVision()
    {
        Assert.Null(AppleVisionOcrEngine.ResolveLanguage(null, []));
        Assert.Equal("de-DE", AppleVisionOcrEngine.ResolveLanguage("de-DE", []));
    }

    /// <summary>A slice of what Vision reports on macOS 15, so the mapping is testable on any OS.</summary>
    private static readonly string[] VisionTags = ["en-US", "fr-FR", "de-DE", "pt-BR", "zh-Hans", "zh-Hant", "ja-JP"];

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

    // Results are read from the "<stem>_res.json" files PaddleOCR writes under --save_path,
    // so every run needs that flag - a run without it produces no results at all.
    [Fact]
    public void Paddle_BuildArguments_AlwaysAsksForResultFiles()
    {
        if (PaddleOcrEngine.Detect() is null)
        {
            Assert.Skip("PaddleOCR not installed");
        }

        using var engine = PaddleOcrEngine.Create("en");
        var args = engine.BuildArguments(@"C:\in", @"C:\out");

        var savePath = args.IndexOf("--save_path");
        Assert.True(savePath >= 0, "--save_path missing: " + string.Join(' ', args));
        Assert.Equal(@"C:\out", args[savePath + 1]);

        var input = args.IndexOf("-i");
        Assert.True(input >= 0);
        Assert.Equal(@"C:\in", args[input + 1]);
    }

    [Fact]
    public void Paddle_RecognizeBatch_NoImages_DoesNotStartTheEngine()
    {
        if (PaddleOcrEngine.Detect() is null)
        {
            Assert.Skip("PaddleOCR not installed");
        }

        // Nothing to do must not cost a process start - the whole point of batching.
        using var engine = PaddleOcrEngine.Create("en");
        Assert.Empty(engine.RecognizeBatch(Array.Empty<SKBitmap>()));
    }
}
