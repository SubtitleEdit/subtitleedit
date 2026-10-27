using System.Diagnostics;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// <c>--burn-in-video</c>: the .ass seconv writes is burned into a video with ffmpeg.
/// The end-to-end tests need ffmpeg on PATH and are skipped without it.
/// </summary>
public class BurnInVideoTest : IDisposable
{
    private readonly string _tempRoot;

    private const string SrtContent = """
        1
        00:00:00,000 --> 00:00:02,000
        WHITE TEXT

        """;

    public BurnInVideoTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "BurnInVideoTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public void GetOutputFileName_UsesSubtitleNameAndVideoExtension()
    {
        var output = BurnInRunner.GetOutputFileName(Path.Combine(_tempRoot, "movie.ass"), Path.Combine("elsewhere", "clip.mkv"));

        Assert.Equal(Path.Combine(_tempRoot, "movie_burn-in.mkv"), output);
    }

    [Fact]
    public async Task ConvertAsync_BurnInEffects_WritesTheOverrideTagsIntoEveryLine()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);
        var style = Path.Combine(_tempRoot, "style.ass");
        await File.WriteAllTextAsync(style, """
            [V4+ Styles]
            Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
            Style: Default,Arial,50,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,0,2,10,10,10,1

            [Events]
            Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
            """, TestContext.Current.CancellationToken);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "ass",
            OutputFolder = _tempRoot,
            AssaStyleFile = style,
            BurnInEffects = BurnInRunner.ParseEffects("fade:400, font-kerning, font-size-change"),
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var ass = await File.ReadAllTextAsync(Path.Combine(_tempRoot, "in.ass"), TestContext.Current.CancellationToken);
        // Each effect is prepended in turn (as in the UI), so the last one listed comes first;
        // the size change targets 120% of the style's 50.
        Assert.Contains(@"{\t(\fs60)}{\t(\fsp4)}{\fad(400,400)}WHITE TEXT", ass);
    }

    [Fact]
    public async Task ConvertAsync_BurnInEffects_PositionAndTimingEffectsUseResolutionFontSizeAndDuration()
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken); // one 2000 ms line

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "ass",
            OutputFolder = _tempRoot,
            Resolution = (1920, 1080),
            BurnInEffects = BurnInRunner.ParseEffects("scroll-up,rotate-in,font-size-bounce-in"),
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var ass = await File.ReadAllTextAsync(Path.Combine(_tempRoot, "in.ass"), TestContext.Current.CancellationToken);
        var dialogue = ass.Split('\n').Single(l => l.StartsWith("Dialogue:")).TrimEnd('\r');
        Assert.Contains(@"{\move(960,1020,960,1000)}", dialogue); // centered, 60 -> 80 px above the bottom of 1080
        Assert.Contains(@"{\frx-30\t(0,1000,\frx0)}", dialogue); // straightens over half the 2000 ms line
        var fontSize = (int)AdvancedSubStationAlpha.GetSsaStyle("Default", AdvancedSubStationAlpha.DefaultHeader).FontSize;
        Assert.Contains($@"{{\fs{(int)(fontSize * 0.5)}\t(0,250,", dialogue); // bounce starts at half the style size
        Assert.Contains($@"\t(750,2000,\fs{fontSize})}}", dialogue); // ...and settles on it by the line's end
    }

    [Theory]
    [InlineData("glow")]
    [InlineData("fade:0")]
    [InlineData("fade:fast")]
    [InlineData("scroll-left")]
    [InlineData("font-kerning:5")]
    [InlineData("fix-rtl:1")]
    public void ParseEffects_RejectsUnknownEffectsAndBadFadeLengths(string value)
    {
        Assert.Throws<FormatException>(() => BurnInRunner.ParseEffects(value));
    }

    [Fact]
    public async Task ConvertAsync_BurnInVideo_DrawsTheSubtitleOnTheVideo()
    {
        if (!FfmpegAvailable())
        {
            Assert.Skip("ffmpeg not on PATH");
        }

        var video = Path.Combine(_tempRoot, "black.mp4");
        Ffmpeg($"-f lavfi -i color=c=black:s=320x240:d=2 -c:v libx264 -pix_fmt yuv420p \"{video}\"");
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "ass",
            OutputFolder = _tempRoot,
            Resolution = (320, 240),
            BurnInVideo = video,
            BurnInFfmpegArgs = "-c:v libx264 -preset ultrafast -pix_fmt yuv420p",
        });

        Assert.True(result.Success, string.Join("; ", result.Errors));
        var burned = Path.Combine(_tempRoot, "in_burn-in.mp4");
        Assert.True(File.Exists(burned));

        // The source is all black, so any bright pixel at 1s is the burned-in text.
        var frame = Path.Combine(_tempRoot, "frame.gray");
        Ffmpeg($"-ss 1 -i \"{burned}\" -frames:v 1 -f rawvideo -pix_fmt gray \"{frame}\"");
        Assert.Contains(await File.ReadAllBytesAsync(frame, TestContext.Current.CancellationToken), b => b > 200);
    }

    [Fact]
    public async Task ConvertAsync_BurnInVideo_KeepsAnExistingOutputWithoutOverwrite()
    {
        var video = Path.Combine(_tempRoot, "movie.mp4");
        await File.WriteAllTextAsync(video, "not touched", TestContext.Current.CancellationToken);
        var existing = Path.Combine(_tempRoot, "in_burn-in.mp4");
        await File.WriteAllTextAsync(existing, "earlier output", TestContext.Current.CancellationToken);
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "ass",
            OutputFolder = _tempRoot,
            BurnInVideo = video,
        });

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("--overwrite"));
        Assert.Equal("earlier output", await File.ReadAllTextAsync(existing, TestContext.Current.CancellationToken));
        Assert.False(File.Exists(Path.Combine(_tempRoot, "in.ass"))); // refused before the .ass is written
    }

    [Theory]
    [InlineData("out.mp4", true)]
    [InlineData("out.MOV", true)]
    [InlineData("out.mkv", false)]
    [InlineData("out.ts", false)]
    public void BuildArguments_AddsFaststartOnlyForMp4Family(string output, bool expected)
    {
        var args = BurnInRunner.BuildArguments("in.mp4", output, null);

        Assert.Equal(expected, args.Contains("-movflags +faststart"));
        Assert.Contains(BurnInRunner.DefaultFfmpegArgs, args);
    }

    [Fact]
    public void BuildArguments_UsesVp9DefaultForWebm()
    {
        Assert.Contains(BurnInRunner.DefaultWebmFfmpegArgs, BurnInRunner.BuildArguments("in.webm", "out.webm", null));
        Assert.Contains("-c:v libx264 -crf 20", BurnInRunner.BuildArguments("in.webm", "out.webm", "-c:v libx264 -crf 20"));
    }

    [Theory]
    [InlineData("ass", true)]
    [InlineData("assa", true)]
    [InlineData("AdvancedSubStationAlpha", true)]
    [InlineData("advancedsubstationalpha", true)]
    [InlineData("Advanced Sub Station Alpha", true)]
    [InlineData("srt", false)]
    [InlineData("ssa", false)]
    public void IsAssTarget_AcceptsEveryAssName(string format, bool expected)
    {
        Assert.Equal(expected, SeConv.Commands.ConvertCommand.IsAssTarget(format));
    }

    [Fact]
    public async Task ConvertAsync_BurnInVideo_FailedEncodeKeepsTheEarlierOutput()
    {
        if (!FfmpegAvailable())
        {
            Assert.Skip("ffmpeg not on PATH");
        }

        var video = Path.Combine(_tempRoot, "black.mp4");
        Ffmpeg($"-f lavfi -i color=c=black:s=320x240:d=1 -c:v libx264 -pix_fmt yuv420p \"{video}\"");
        var existing = Path.Combine(_tempRoot, "in_burn-in.mp4");
        await File.WriteAllTextAsync(existing, "earlier output", TestContext.Current.CancellationToken);
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, SrtContent, TestContext.Current.CancellationToken);

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = "ass",
            OutputFolder = _tempRoot,
            Overwrite = true,
            BurnInVideo = video,
            BurnInFfmpegArgs = "-c:v no-such-encoder",
        });

        Assert.False(result.Success);
        Assert.Equal("earlier output", await File.ReadAllTextAsync(existing, TestContext.Current.CancellationToken));
        Assert.DoesNotContain(Directory.GetFiles(_tempRoot), f => Path.GetFileName(f).StartsWith(".in_burn-in.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Duration: 00:11:37.04, start: 0.000000, bitrate: 5000 kb/s", "Duration:", 697)]
    [InlineData("frame=  250 fps=50 q=28.0 size=512kB time=00:00:10.00 bitrate=419.4kbits/s speed=2x", "time=", 10)]
    [InlineData("Duration: N/A, bitrate: N/A", "Duration:", 0)]
    [InlineData("no time here", "time=", 0)]
    public void ParseSeconds_ReadsFfmpegDurationAndStatsTime(string line, string prefix, int expected)
    {
        Assert.Equal(expected, BurnInRunner.ParseSeconds(line, prefix));
    }

    private static bool FfmpegAvailable()
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("ffmpeg", "-version") { RedirectStandardOutput = true, UseShellExecute = false });
            p!.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void Ffmpeg(string arguments)
    {
        using var p = Process.Start(new ProcessStartInfo("ffmpeg", "-hide_banner -loglevel error -y " + arguments)
        {
            RedirectStandardError = true,
            UseShellExecute = false,
        });
        var err = p!.StandardError.ReadToEnd();
        p.WaitForExit();
        Assert.True(p.ExitCode == 0, err);
    }
}
