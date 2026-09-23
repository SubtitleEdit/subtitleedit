using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class OutputFileNameTest : IDisposable
{
    private readonly string _tempRoot;

    public OutputFileNameTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "OutputFileName_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private ConversionOptions Opts(bool overwrite = false, string? outputFilename = null, bool noLanguageSuffix = false) =>
        new()
        {
            Patterns = ["dummy.srt"],
            Format = "WebVTT",
            OutputFolder = _tempRoot,
            Overwrite = overwrite,
            OutputFilename = outputFilename,
            NoLanguageSuffix = noLanguageSuffix,
        };

    [Fact]
    public void Resolve_FileNotExists_ReturnsBasePath()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts());

        Assert.Equal(Path.Combine(_tempRoot, "input.vtt"), result);
    }

    [Fact]
    public void Resolve_FileExistsAndOverwrite_ReturnsBasePath()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        File.WriteAllText(Path.Combine(_tempRoot, "input.vtt"), "preexisting");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true));

        Assert.Equal(Path.Combine(_tempRoot, "input.vtt"), result);
    }

    [Fact]
    public void Resolve_FileExistsNoOverwrite_RotatesToUnderscore2()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        File.WriteAllText(Path.Combine(_tempRoot, "input.vtt"), "preexisting");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: false));

        Assert.Equal(Path.Combine(_tempRoot, "input_2.vtt"), result);
    }

    [Fact]
    public void Resolve_MultipleCollisions_PicksNextFreeNumber()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        File.WriteAllText(Path.Combine(_tempRoot, "input.vtt"), "");
        File.WriteAllText(Path.Combine(_tempRoot, "input_2.vtt"), "");
        File.WriteAllText(Path.Combine(_tempRoot, "input_3.vtt"), "");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: false));

        Assert.Equal(Path.Combine(_tempRoot, "input_4.vtt"), result);
    }

    [Fact]
    public void Resolve_OutputFilenameSet_UsesItVerbatim()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        var explicitOutput = Path.Combine(_tempRoot, "renamed.vtt");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true, outputFilename: explicitOutput));

        Assert.Equal(explicitOutput, result);
    }

    [Fact]
    public void Resolve_RelativeOutputFilenameWithOutputFolder_CombinesThem()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true, outputFilename: "renamed.vtt"));

        Assert.Equal(Path.Combine(_tempRoot, "renamed.vtt"), result);
    }

    [Fact]
    public void Resolve_TwoSameLanguageTracksWithOverwrite_SecondGetsTrackName()
    {
        var input = Path.Combine(_tempRoot, "video.mkv");
        File.WriteAllText(input, "");
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var first = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true), "en", 3, used);
        var second = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true), "en", 4, used);

        Assert.Equal(Path.Combine(_tempRoot, "video.en.vtt"), first);
        Assert.Equal(Path.Combine(_tempRoot, "video.#4.en.vtt"), second);
    }

    [Fact]
    public void Resolve_SameNameTwiceInRunNoTrackNumber_SecondRotates()
    {
        var input = Path.Combine(_tempRoot, "video.srt");
        File.WriteAllText(input, "");
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Nothing on disk yet - only the run's own bookkeeping forces the rotation,
        // as when the first output has been resolved but not written.
        var first = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true), usedNames: used);
        var second = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true), usedNames: used);

        Assert.Equal(Path.Combine(_tempRoot, "video.vtt"), first);
        Assert.Equal(Path.Combine(_tempRoot, "video_2.vtt"), second);
    }

    [Fact]
    public void Resolve_TrackNameTakenByRunAndDiskFull_RotatesPastBoth()
    {
        var input = Path.Combine(_tempRoot, "video.mkv");
        File.WriteAllText(input, "");
        File.WriteAllText(Path.Combine(_tempRoot, "video.en.vtt"), "preexisting");
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.Combine(_tempRoot, "video.#4.en.vtt"),
        };

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: false), "en", 4, used);

        Assert.Equal(Path.Combine(_tempRoot, "video.en_2.vtt"), result);
    }

    [Fact]
    public void Resolve_AbsoluteOutputFilename_IgnoresOutputFolder()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");
        var other = Path.Combine(_tempRoot, "elsewhere");
        Directory.CreateDirectory(other);
        var absolute = Path.Combine(other, "renamed.vtt");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(overwrite: true, outputFilename: absolute));

        Assert.Equal(absolute, result);
    }

    // Forced tracks (MKV forced flag, MP4 tx3g forced displayFlags) get the player
    // convention's name token, so they no longer collide with the same-language full track.
    [Theory]
    [InlineData("eng", true, "eng.forced")]
    [InlineData("eng", false, "eng")]
    [InlineData("", true, "forced")]
    [InlineData(null, true, "forced")]
    [InlineData(null, false, null)]
    public void AppendForcedToken_Cases(string? languageSuffix, bool isForced, string? expected)
    {
        Assert.Equal(expected, SubtitleConverter.AppendForcedToken(languageSuffix, isForced));
    }

    [Fact]
    public void Resolve_ForcedSuffix_DoesNotCollideWithFullTrack()
    {
        var input = Path.Combine(_tempRoot, "video.mp4");
        File.WriteAllText(input, "");
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var full = SubtitleConverter.ResolveOutputFileName(input, Opts(), SubtitleConverter.AppendForcedToken("eng", false), 2, used);
        var forced = SubtitleConverter.ResolveOutputFileName(input, Opts(), SubtitleConverter.AppendForcedToken("eng", true), 3, used);

        Assert.Equal(Path.Combine(_tempRoot, "video.eng.vtt"), full);
        Assert.Equal(Path.Combine(_tempRoot, "video.eng.forced.vtt"), forced);
    }

    [Fact]
    public void Resolve_OutputFilenameAppend_IsAddedToStem()
    {
        var input = Path.Combine(_tempRoot, "movie.ts");
        File.WriteAllText(input, "");
        var opts = new ConversionOptions
        {
            Patterns = [input],
            Format = "WebVTT",
            OutputFolder = _tempRoot,
            OutputFilenameAppend = "_fixed",
        };

        Assert.Equal(Path.Combine(_tempRoot, "movie_fixed.vtt"), SubtitleConverter.ResolveOutputFileName(input, opts));
        Assert.Equal(Path.Combine(_tempRoot, "movie_fixed.eng.vtt"), SubtitleConverter.ResolveOutputFileName(input, opts, languageSuffix: "eng"));
    }

    [Fact]
    public void Resolve_OutputFilenameAppend_IgnoredWithExplicitOutputFilename()
    {
        var input = Path.Combine(_tempRoot, "movie.ts");
        File.WriteAllText(input, "");
        var opts = new ConversionOptions
        {
            Patterns = [input],
            Format = "WebVTT",
            OutputFolder = _tempRoot,
            OutputFilename = "out.vtt",
            OutputFilenameAppend = "_fixed",
        };

        Assert.Equal(Path.Combine(_tempRoot, "out.vtt"), SubtitleConverter.ResolveOutputFileName(input, opts));
    }

    [Fact]
    public void Resolve_LanguageSuffix_InsertedBeforeExtension()
    {
        var input = Path.Combine(_tempRoot, "input.srt");
        File.WriteAllText(input, "");

        var result = SubtitleConverter.ResolveOutputFileName(input, Opts(), "en");

        Assert.Equal(Path.Combine(_tempRoot, "input.en.vtt"), result);
    }

    [Fact]
    public void Resolve_NoLanguageSuffixWithOverwrite_TranslatesInPlace()
    {
        // #15156: --no-language-suffix --overwrite --translate-to must hand back the
        // input's own name (same format) instead of input.en.vtt.
        var input = Path.Combine(_tempRoot, "input.vtt");
        File.WriteAllText(input, "");
        var opts = Opts(overwrite: true, noLanguageSuffix: true);

        var result = SubtitleConverter.ResolveOutputFileName(input, opts, SubtitleConverter.LanguageToken(opts, "en"));

        Assert.Equal(input, result);
    }

    [Fact]
    public void Resolve_NoLanguageSuffixWithoutOverwrite_RotatesInsteadOfClobberingInput()
    {
        var input = Path.Combine(_tempRoot, "input.vtt");
        File.WriteAllText(input, "");
        var opts = Opts(overwrite: false, noLanguageSuffix: true);

        var result = SubtitleConverter.ResolveOutputFileName(input, opts, SubtitleConverter.LanguageToken(opts, "en"));

        Assert.Equal(Path.Combine(_tempRoot, "input_2.vtt"), result);
    }

    [Fact]
    public void Resolve_NoLanguageSuffix_ContainerTrackCollisionUsesCounter()
    {
        var input = Path.Combine(_tempRoot, "video.mkv");
        File.WriteAllText(input, "");
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var opts = Opts(overwrite: true, noLanguageSuffix: true);

        var first = SubtitleConverter.ResolveOutputFileName(input, opts, SubtitleConverter.LanguageToken(opts, "en"), 3, used);
        var second = SubtitleConverter.ResolveOutputFileName(input, opts, SubtitleConverter.LanguageToken(opts, "en"), 4, used);

        Assert.Equal(Path.Combine(_tempRoot, "video.vtt"), first);
        Assert.Equal(Path.Combine(_tempRoot, "video_2.vtt"), second);
    }

    [Fact]
    public void Resolve_NoLanguageSuffix_KeepsForcedTokenAndStreamLabels()
    {
        // Only the language goes: a forced track must stay recognisable next to the full
        // track of the same language, and DVB/XSUB stream labels are not languages.
        var input = Path.Combine(_tempRoot, "video.mkv");
        File.WriteAllText(input, "");
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var opts = Opts(overwrite: true, noLanguageSuffix: true);

        var full = SubtitleConverter.ResolveOutputFileName(input, opts, SubtitleConverter.AppendForcedToken(SubtitleConverter.LanguageToken(opts, "eng"), false), 3, used);
        var forced = SubtitleConverter.ResolveOutputFileName(input, opts, SubtitleConverter.AppendForcedToken(SubtitleConverter.LanguageToken(opts, "eng"), true), 4, used);
        var dvb = SubtitleConverter.ResolveOutputFileName(input, opts, "dvb_pid301", 301, used);

        Assert.Equal(Path.Combine(_tempRoot, "video.vtt"), full);
        Assert.Equal(Path.Combine(_tempRoot, "video.forced.vtt"), forced);
        Assert.Equal(Path.Combine(_tempRoot, "video.dvb_pid301.vtt"), dvb);
    }
}
