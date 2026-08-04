using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class ContainerLoaderTest : IDisposable
{
    private readonly string _tempRoot;

    public ContainerLoaderTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "Container_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public async Task ConvertAsync_MkvWithTextTrack_ProducesSrtWithLanguageSuffix()
    {
        var input = Fixtures.Path("container_text.mkv");
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
        Assert.Equal(1, result.SuccessfulFiles);
        var outputs = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Single(outputs);
        // language suffix injected before extension
        Assert.Contains(".und.", Path.GetFileName(outputs[0]));
        var content = await File.ReadAllTextAsync(outputs[0], TestContext.Current.CancellationToken);
        Assert.Contains("Line 1", content);
        Assert.Contains("Line 2", content);
    }

    [Fact]
    public async Task ConvertAsync_MkvTextTrackWithoutDeclaredLanguage_AutoDetectsLanguage()
    {
        // container_text_undeclared_lang.mkv carries a Danish text track muxed without a
        // language declaration ("und") - the loader must auto-detect the language instead
        // of passing the undeclared tag through.
        var input = Fixtures.Path("container_text_undeclared_lang.mkv");
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
        var outputs = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Single(outputs);
        Assert.Contains(".da.", Path.GetFileName(outputs[0]));
    }

    [Fact]
    public async Task ConvertAsync_MkvWithImageTracks_TimeCodesOnly_ProducesBothTracks()
    {
        // container_image.mkv has both a VobSub (S_VOBSUB) and a PGS (S_HDMV/PGS)
        // image-subtitle track. With --time-codes-only both are decoded for timing and
        // emitted as text (empty text), no OCR engine required — so this runs everywhere,
        // and proves the VobSub-in-MKV path is wired (it used to be warned-and-skipped).
        var input = Fixtures.Path("container_image.mkv");
        Assert.True(File.Exists(input), $"Fixture missing: {input}");
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        // Overwrite=false so the two same-language ("und") tracks get distinct names via the
        // track-number disambiguation (movie.und.srt + movie.#N.und.srt) instead of one
        // clobbering the other.
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = false,
            TimeCodesOnly = true,
        });

        // Both image tracks (PGS + VobSub) convert: two successes, two .srt outputs.
        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(2, result.SuccessfulFiles);
        var outputs = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Equal(2, outputs.Length);

        // Every output carries time codes but no recognised text (no letters leaked in).
        foreach (var f in outputs)
        {
            var content = await File.ReadAllTextAsync(f, TestContext.Current.CancellationToken);
            Assert.Contains("-->", content);
            Assert.DoesNotContain(content, c => char.IsLetter(c));
        }
    }

    [Fact]
    public async Task ConvertAsync_Mp4WithTextTrack_ProducesSrt()
    {
        var input = Fixtures.Path("container_text.mp4");
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
        Assert.Equal(1, result.SuccessfulFiles);
        var outputs = Directory.GetFiles(outputFolder, "*.srt");
        Assert.Single(outputs);
        var content = await File.ReadAllTextAsync(outputs[0], TestContext.Current.CancellationToken);
        var normalized = content.Replace("\r", "").TrimStart('﻿');
        // Paragraphs should be properly numbered (1, 2, ...) not all 0
        Assert.StartsWith("1\n", normalized);
        Assert.Contains("\n2\n", normalized);
        Assert.DoesNotContain("\n0\n", normalized);
    }

    [Fact]
    public async Task ConvertAsync_TrackNumberFilter_ExcludesNonMatching()
    {
        var input = Fixtures.Path("container_text.mkv");
        Assert.True(File.Exists(input));
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        // The fixture's text track is at a known number; pick one we know is NOT it.
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
            TrackNumbers = [9999],
        });

        // No track matched — no failure, no success
        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Equal(0, result.SuccessfulFiles);
    }
}
