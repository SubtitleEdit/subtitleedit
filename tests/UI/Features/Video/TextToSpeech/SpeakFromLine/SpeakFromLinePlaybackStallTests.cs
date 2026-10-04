using Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

namespace UITests.Features.Video.TextToSpeech.SpeakFromLine;

/// <summary>
/// When mpv cannot load a clip it goes idle without pausing, so the end-of-clip pause the
/// reader waits for never comes; a position that stops moving must end the wait.
/// </summary>
public class SpeakFromLinePlaybackStallTests
{
    [Fact]
    public void PositionNeverMoves_StallsAfterTimeout()
    {
        var detector = new SpeakFromLineReader.PlaybackStallDetector(5000);

        Assert.False(detector.IsStalled(0, 50));
        Assert.False(detector.IsStalled(0, 4000));
        Assert.True(detector.IsStalled(0, 5050));
    }

    [Fact]
    public void PositionMoving_NeverStalls()
    {
        var detector = new SpeakFromLineReader.PlaybackStallDetector(5000);

        for (var ms = 50; ms <= 20_000; ms += 50)
        {
            Assert.False(detector.IsStalled(ms / 1000.0, ms));
        }
    }

    [Fact]
    public void PositionStopsAfterMoving_StallsTimeoutAfterLastChange()
    {
        var detector = new SpeakFromLineReader.PlaybackStallDetector(5000);

        Assert.False(detector.IsStalled(0, 50));
        Assert.False(detector.IsStalled(1.0, 1000));
        Assert.False(detector.IsStalled(1.0, 5900));
        Assert.True(detector.IsStalled(1.0, 6000));
    }
}
