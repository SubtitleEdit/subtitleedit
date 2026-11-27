using Nikse.SubtitleEdit.Features.Shared.Undocked;

namespace UITests.Logic;

// Video and waveform can be undocked on their own, keeping the other one docked (#17218).
public class UndockModeTests
{
    [Theory]
    [InlineData(UndockMode.None, false, false)]
    [InlineData(UndockMode.Both, true, true)]
    [InlineData(UndockMode.VideoOnly, true, false)]
    [InlineData(UndockMode.WaveformOnly, false, true)]
    public void WhatIsUndocked(UndockMode mode, bool video, bool waveform)
    {
        Assert.Equal(video, UndockModeHelper.IsVideoUndocked(mode));
        Assert.Equal(waveform, UndockModeHelper.IsWaveformUndocked(mode));
    }

    [Fact]
    public void FromSettings_NotUndocked_IsNone()
    {
        Assert.Equal(UndockMode.None, UndockModeHelper.FromSettings(false, "VideoOnly"));
    }

    [Theory]
    [InlineData("VideoOnly", UndockMode.VideoOnly)]
    [InlineData("waveformonly", UndockMode.WaveformOnly)]
    [InlineData("Both", UndockMode.Both)]
    public void FromSettings_RestoresSavedMode(string saved, UndockMode expected)
    {
        Assert.Equal(expected, UndockModeHelper.FromSettings(true, saved));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("garbage")]
    public void FromSettings_OldOrUnknownSettings_UndockBoth(string? saved)
    {
        // Settings from before #17218 only had UndockVideoControls = true, which meant both.
        Assert.Equal(UndockMode.Both, UndockModeHelper.FromSettings(true, saved));
    }
}
