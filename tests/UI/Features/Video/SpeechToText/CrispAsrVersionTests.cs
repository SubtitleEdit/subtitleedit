using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText;

public class CrispAsrVersionTests
{
    [Theory]
    [InlineData("0.8.41", true)]
    [InlineData("v0.8.41", true)]
    [InlineData("0.9.0", true)]
    [InlineData("0.8.41-dirty", true)]
    [InlineData("0.8.40", false)]
    [InlineData("0.8.4", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("unknown", true)]
    public void IndexEcho_OnlyCrispAsrKnownToBeTooOldIsRefused(string? version, bool expected)
    {
        Assert.Equal(expected, CrispAsrVersion.IsAtLeast(version, CrispAsrIndexEcho.MinimumCrispAsrVersion));
    }
}
