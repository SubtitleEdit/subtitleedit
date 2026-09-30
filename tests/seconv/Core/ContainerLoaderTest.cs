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
    public async Task ConvertAsync_Mp4WithCea608And708InVideo_ProducesSrtPerCaptionTrack()
    {
        // H.264 SEI captions are not a subtitle track; the MP4 loader used to report
        // "No subtitle tracks" although the parser had decoded them (the GUI offered them).
        var input = Fixtures.Path("container_cea608_708.mp4");
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
        Assert.Contains(outputs, p => Path.GetFileName(p).Contains("cea608_cc1"));
        Assert.Contains(outputs, p => Path.GetFileName(p).Contains("cea708_s1"));
        var cc1 = await File.ReadAllTextAsync(outputs.First(p => Path.GetFileName(p).Contains("cea608_cc1")), TestContext.Current.CancellationToken);
        Assert.Contains("inaudible radio chatter", cc1);
    }

    /// <summary>
    /// The loader goes by content before extension, as the GUI does: a Matroska file named .mp4,
    /// a Blu-ray .sup named .sub and a transport stream named .mpeg were read with the loader of
    /// their extension (or, for .mpeg, as text for minutes) and failed.
    /// </summary>
    [Theory]
    [InlineData("container_text.mkv", "movie.mp4", false)]
    [InlineData("sample.sup", "movie.sub", true)]
    [InlineData("container_teletext.ts", "recording.mpeg", false)]
    public async Task ConvertAsync_ContainerWithAnotherExtension_IsReadByItsContent(string fixture, string fileName, bool timeCodesOnly)
    {
        var input = Path.Combine(_tempRoot, fileName);
        File.Copy(Fixtures.Path(fixture), input);
        var outputFolder = Path.Combine(_tempRoot, "out");
        Directory.CreateDirectory(outputFolder);

        var converter = new SubtitleConverter();
        var result = await converter.ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "SubRip",
            OutputFolder = outputFolder,
            Overwrite = true,
            TimeCodesOnly = timeCodesOnly,
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var outputs = Directory.GetFiles(outputFolder, "*.srt");
        Assert.NotEmpty(outputs);
        Assert.Contains("-->", await File.ReadAllTextAsync(outputs[0], TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ConvertAsync_Mp4WithoutUsableTrack_ReportsNoSubtitleTracks()
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
            TrackNumbers = [9999],
        });

        // The MP4 error must surface as-is. It used to be swallowed, and the video was then
        // read as a text file and failed with "Unable to determine subtitle format".
        Assert.False(result.Success);
        Assert.Equal(0, result.SuccessfulFiles);
        Assert.Contains(result.Errors, e => e.Contains("No subtitle tracks in MP4 file"));
        Assert.DoesNotContain(result.Errors, e => e.Contains("Unable to determine subtitle format"));
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

        // The file has subtitle tracks but none matched the filter — that is an error,
        // not a silent zero-file success (matches the MP4/TS/MXF loaders).
        Assert.False(result.Success);
        Assert.Equal(0, result.SuccessfulFiles);
        Assert.Contains(result.Errors, e => e.Contains("--track-number"));
    }
}
