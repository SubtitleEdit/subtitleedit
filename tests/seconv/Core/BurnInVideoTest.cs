using System.Diagnostics;
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
