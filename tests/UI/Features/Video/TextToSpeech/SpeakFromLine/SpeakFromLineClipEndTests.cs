using Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

namespace UITests.Features.Video.TextToSpeech.SpeakFromLine;

/// <summary>
/// The player's cached pause state (and the position read through it) can still hold the pause
/// the previous clip ended with while mpv plays the next one, so the reader times each clip's end
/// itself instead of waiting for the player to report it. Waiting cut lines off, or once
/// un-paused, left a five-second gap after every line.
/// </summary>
public class SpeakFromLineClipEndTests
{
    [Fact]
    public void StuckPosition_ClipIsOverOnceItsLengthHasPlayed()
    {
        // A 1.29 s clip whose (cached) position never moves past 0.276.
        Assert.False(SpeakFromLineReader.IsClipOver(0.5, 1.0, 1.29, 0.276));
        Assert.False(SpeakFromLineReader.IsClipOver(1.29 + SpeakFromLineReader.EndSafetySeconds - 0.05, 1.0, 1.29, 0.276));
        Assert.True(SpeakFromLineReader.IsClipOver(1.29 + SpeakFromLineReader.EndSafetySeconds + 0.05, 1.0, 1.29, 0.276));
    }

    [Fact]
    public void PositionAtTheEnd_ClipIsOver()
    {
        Assert.True(SpeakFromLineReader.IsClipOver(0.4, 1.0, 2.0, 2.0 - SpeakFromLineReader.EndToleranceSeconds));
        Assert.False(SpeakFromLineReader.IsClipOver(0.4, 1.0, 2.0, 1.5));
    }

    [Fact]
    public void DoubleSpeed_HalvesTheTimedEnd()
    {
        // A 3 s clip at 2x is over after 1.5 s plus the margin (in clip time), not after 3 s.
        Assert.False(SpeakFromLineReader.IsClipOver(1.5, 2.0, 3.0, 0));
        Assert.True(SpeakFromLineReader.IsClipOver(1.8, 2.0, 3.0, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void UnknownDuration_NeverOver(double duration)
    {
        Assert.False(SpeakFromLineReader.IsClipOver(60, 1.0, duration, 0));
    }
}
