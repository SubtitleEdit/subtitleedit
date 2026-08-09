using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.Engines;

namespace UITests.Features.Ocr.Engines;

public class CrispEmbedEngineTests
{
    [Fact]
    public void GetBackends_HasExpectedBackends()
    {
        var backends = CrispEmbedEngine.GetBackends();

        Assert.Equal(new[] { "PP-OCRv6", "GLM-OCR", "GOT-OCR2", "Qwen3-VL-2B", "DeepSeek-OCR-2" }, backends.Select(p => p.Name));
    }

    [Fact]
    public void GetBackends_OnlyPpOcrV6UsesTextDetector()
    {
        foreach (var backend in CrispEmbedEngine.GetBackends())
        {
            Assert.Equal(backend.Name == "PP-OCRv6", backend.UsesTextDetector);

            // A backend is all-or-nothing: the run path is picked from the backend, not per model.
            Assert.All(backend.Models, model => Assert.Equal(backend.UsesTextDetector, model.HasDetector));
        }
    }

    [Fact]
    public void GetBackends_DetectorModelsAreWellFormed()
    {
        var models = CrispEmbedEngine.GetBackends()
            .Where(p => p.UsesTextDetector)
            .SelectMany(p => p.Models)
            .ToList();

        Assert.NotEmpty(models);

        foreach (var model in models)
        {
            Assert.EndsWith(".gguf", model.DetectorName);
            Assert.StartsWith("https://huggingface.co/", model.DetectorUrl);
            Assert.EndsWith("/" + model.DetectorName, model.DetectorUrl);

            // The detector lands in the same models folder as the recognizer.
            Assert.NotEqual(model.Name, model.DetectorName);

            // Truncation floors must stay below the real file sizes or a good download reads
            // as "not installed" forever.
            Assert.InRange(model.MinimumSizeBytes, 1, 40_000_000);
            Assert.InRange(model.MinimumDetectorSizeBytes, 1, 40_000_000);
        }
    }

    [Fact]
    public void GetBackends_ModelFileNamesAreUniqueAcrossBackends()
    {
        var names = CrispEmbedEngine.GetBackends()
            .SelectMany(p => p.Models)
            .SelectMany(m => m.HasDetector ? new[] { m.Name, m.DetectorName } : new[] { m.Name })
            .ToList();

        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Theory]
    [InlineData(
        "ppocrv6-det: persistent GGML detector graph ready (CPU, 736x96)\n{\"n_regions\":2,\"mean_confidence\":0.92,\"full_text\":\"Line one\\nLine two\"}\n",
        "Line one\nLine two")]
    [InlineData("{\"n_regions\":0,\"mean_confidence\":0.0,\"full_text\":\"\"}", "")]
    [InlineData("ppocrv6-det: nothing but progress noise\n", "")]
    [InlineData("", "")]
    public void ParseCliPipelineOutput_ReturnsFullText(string output, string expected)
    {
        var actual = CrispEmbedOcr.ParseCliPipelineOutput(output);

        Assert.Equal(expected.Replace("\n", Environment.NewLine), actual);
    }

    [Fact]
    public void GetExitCodeHint_ExplainsLoaderFailures()
    {
        if (OperatingSystem.IsWindows())
        {
            // 126/127 are Unix loader/shell conventions - no hint to give on Windows.
            Assert.Equal(string.Empty, CrispEmbedOcr.GetExitCodeHint(127));
            return;
        }

        // 127 = the loader could not resolve a dependency before the server could report
        // anything itself. The OpenBLAS cause behind #13205 was fixed upstream in v0.17.5, so
        // the hint must not send users off to install it; glibc is the remaining cause.
        var hint127 = CrispEmbedOcr.GetExitCodeHint(127);
        Assert.Contains("shared library", hint127);
        Assert.Contains("glibc", hint127);
        Assert.DoesNotContain("OpenBLAS", hint127);
        Assert.DoesNotContain("libopenblas", hint127);
        Assert.Contains("executed", CrispEmbedOcr.GetExitCodeHint(126));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(139)]
    public void GetExitCodeHint_IsEmptyForCodesWithNothingToAdd(int exitCode)
    {
        Assert.Equal(string.Empty, CrispEmbedOcr.GetExitCodeHint(exitCode));
    }

    [Theory]
    [InlineData("Line one\n    Line two indented", "Line one\nLine two indented")]
    [InlineData("  padded  ", "padded")]
    [InlineData("One\r\nTwo", "One\nTwo")]
    [InlineData("", "")]
    public void NormalizeServerText_TrimsEachLine(string input, string expected)
    {
        Assert.Equal(expected.Replace("\n", Environment.NewLine), CrispEmbedOcr.NormalizeServerText(input));
    }

    [Fact]
    public void ParseCliPipelineOutput_SkipsNonResultJson()
    {
        const string output = "{\"warning\":\"some other object\"}\n{\"full_text\":\"the real result\"}\n";

        Assert.Equal("the real result", CrispEmbedOcr.ParseCliPipelineOutput(output));
    }

    [Fact]
    public void GetBackends_ModelsAreWellFormed()
    {
        foreach (var backend in CrispEmbedEngine.GetBackends())
        {
            Assert.NotEmpty(backend.Models);

            foreach (var model in backend.Models)
            {
                Assert.EndsWith(".gguf", model.Name);
                Assert.False(string.IsNullOrWhiteSpace(model.Size));

                // The model is downloaded to <models>/<Name>, so the URL must end in the
                // same file name or the install check would never see the download.
                Assert.StartsWith("https://huggingface.co/", model.Url);
                Assert.EndsWith("/" + model.Name, model.Url);
            }

            // No duplicate model file names within a backend - they share one models folder.
            Assert.Equal(backend.Models.Count, backend.Models.Select(p => p.Name).Distinct().Count());
        }
    }

    [Fact]
    public void OcrEngines_CrispEmbedListedOnSupportedPlatforms()
    {
        var engines = OcrEngineItem.GetOcrEngines();

        Assert.Equal(
            CrispEmbedEngine.CanBeDownloaded(),
            engines.Any(p => p.EngineType == OcrEngineType.CrispEmbed));
    }
}
