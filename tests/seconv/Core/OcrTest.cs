using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class OcrTest : IDisposable
{
    private readonly string _tempRoot;

    public OcrTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Ocr_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public void Tesseract_Detect_ReturnsAbsolutePathOrNull()
    {
        var path = TesseractOcrEngine.Detect();
        if (path is not null)
        {
            // If detected, it must be an absolute path to an existing file
            Assert.True(File.Exists(path), $"Detect() returned non-existent path: {path}");
            Assert.True(Path.IsPathRooted(path), $"Detect() returned non-rooted path: {path}");
        }
        // null is also acceptable — Tesseract may not be installed in CI
    }

    [Fact]
    public void Tesseract_Create_ThrowsWhenMissing_OrSucceedsWhenInstalled()
    {
        if (TesseractOcrEngine.Detect() is null)
        {
            var ex = Assert.Throws<InvalidOperationException>(() => TesseractOcrEngine.Create("eng"));
            Assert.Contains("Tesseract not found on PATH", ex.Message);
        }
        else
        {
            using var engine = TesseractOcrEngine.Create("eng");
            Assert.NotNull(engine.ExecutablePath);
            Assert.Equal("eng", engine.Language);
        }
    }

    [Fact]
    public async Task ConvertAsync_TransportStreamTeletext_ExtractsAsTextNoOcrNeeded()
    {
        var input = Fixtures.Path("container_teletext.ts");
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
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.True(result.SuccessfulFiles >= 1, "Expected at least one teletext page extracted.");
        var outputs = Directory.GetFiles(outputFolder, "*.srt");
        Assert.NotEmpty(outputs);
        // Output filename should encode the teletext page
        Assert.Contains(outputs, p => Path.GetFileName(p).Contains("teletext_"));

        // Verify the content is real subtitle text, not OCR garbage
        var content = await File.ReadAllTextAsync(outputs[0]);
        Assert.Contains("-->", content);
    }

    [Fact]
    public async Task ConvertAsync_BluRaySup_WithoutTesseract_FailsWithClearError()
    {
        if (TesseractOcrEngine.Detect() is not null)
        {
            return; // Skip when Tesseract is actually installed
        }

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
        });

        Assert.False(result.Success);
        Assert.Single(result.Errors);
        Assert.Contains("Tesseract not found on PATH", result.Errors[0]);
    }

    [Fact]
    public async Task ConvertAsync_TeletextOnlyPage_FiltersOutOtherPages()
    {
        var input = Fixtures.Path("container_teletext.ts");
        Assert.True(File.Exists(input));
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();

        // Step 1: extract everything to count pages
        var allResult = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
        });
        Assert.True(allResult.Success);
        var totalPages = allResult.SuccessfulFiles;

        if (totalPages < 2)
        {
            return; // Fixture only has one page; nothing to filter against
        }

        // Step 2: find a specific page in the produced filenames (format: name.teletext_PID_pPAGE.srt)
        var sampleOutput = Directory.GetFiles(outputFolder, "*.srt")[0];
        var pageMatch = System.Text.RegularExpressions.Regex.Match(Path.GetFileName(sampleOutput), @"_p(\d+)\.srt$");
        Assert.True(pageMatch.Success, $"Couldn't extract page number from: {sampleOutput}");
        var picked = int.Parse(pageMatch.Groups[1].Value);

        // Step 3: extract just that one page
        var filteredFolder = Path.Combine(_tempRoot, "filtered");
        Directory.CreateDirectory(filteredFolder);
        var filteredResult = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = filteredFolder,
            Overwrite = true,
            TeletextOnlyPage = picked,
        });
        Assert.True(filteredResult.Success);
        Assert.Equal(1, filteredResult.SuccessfulFiles);
    }
}
