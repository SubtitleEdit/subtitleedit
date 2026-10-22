using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// The retry when input seeking gives no frame. Decoding from the start of the file took 13-18 s
/// a few minutes in, and the Set position preview waited for it (#16289).
/// </summary>
public class FfmpegGeneratorScreenShotFallbackTests
{
    [Theory]
    [InlineData("777.7", "-y -ss 767.7 -i \"in.mkv\" -ss 10  -frames:v 1 -c:v png \"out.png\"")]
    [InlineData("00:12:57.7", "-y -ss 767.7 -i \"in.mkv\" -ss 10  -frames:v 1 -c:v png \"out.png\"")]
    [InlineData("12:57.7", "-y -ss 767.7 -i \"in.mkv\" -ss 10  -frames:v 1 -c:v png \"out.png\"")]
    public void LateTime_InputSeeksCloseBeforeIt(string timeCode, string expected)
    {
        Assert.Equal(expected, FfmpegGenerator.GetScreenShotFallbackArguments("in.mkv", timeCode, string.Empty, "out.png"));
    }

    [Theory]
    [InlineData("8.5")]
    [InlineData("10")]
    [InlineData("not a time")]
    public void EarlyOrUnparsedTime_OutputSeeksFromStart(string timeCode)
    {
        Assert.Equal($"-y -i \"in.mkv\" -ss {timeCode}  -frames:v 1 -c:v png \"out.png\"",
            FfmpegGenerator.GetScreenShotFallbackArguments("in.mkv", timeCode, string.Empty, "out.png"));
    }
}
