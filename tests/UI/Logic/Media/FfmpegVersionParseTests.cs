using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class FfmpegVersionParseTests
{
    [Theory]
    [InlineData("ffmpeg version 6.1.1 Copyright (c) 2000-2023 the FFmpeg developers", 6)]
    [InlineData("ffmpeg version 8.1 Copyright (c) 2000-2025", 8)]
    [InlineData("ffmpeg version n7.0 Copyright", 7)] // git/release "n" prefix
    [InlineData("ffmpeg version 4.4.2-0ubuntu0.22.04.1 Copyright", 4)] // distro build
    [InlineData("ffmpeg version 3.4.11-0+deb10u1 Copyright", 3)]
    [InlineData("FFMPEG VERSION 5.0 COPYRIGHT", 5)] // case-insensitive
    public void ParsesMajorVersion(string banner, int expected)
    {
        Assert.Equal(expected, FfmpegHelper.ParseMajorVersion(banner));
    }

    [Theory]
    [InlineData("ffmpeg version N-109000-g1234567 Copyright")] // nightly build, no numeric major
    [InlineData("some unrelated text")]
    [InlineData("")]
    [InlineData(null)]
    public void ReturnsNullWhenVersionCannotBeDetermined(string? banner)
    {
        Assert.Null(FfmpegHelper.ParseMajorVersion(banner!));
    }

    [Theory]
    [InlineData("ffmpeg version 8.1 Copyright", true)]
    [InlineData("ffmpeg version 7.0 Copyright", true)] // exactly the minimum
    [InlineData("ffmpeg version 6.1.1 Copyright", false)] // below minimum
    [InlineData("ffmpeg version 4.4.2-0ubuntu Copyright", false)] // common distro build, below minimum
    [InlineData("ffmpeg version N-109000-g1234567 Copyright", true)] // unknown -> assume usable
    public void SupportedDecisionMatchesMajorVersion(string banner, bool expectedSupported)
    {
        var major = FfmpegHelper.ParseMajorVersion(banner);

        // Mirror IsSupportedVersion's rule: unknown (null) is treated as usable.
        var supported = major == null || major.Value >= FfmpegHelper.MinimumFfmpegMajorVersion;

        Assert.Equal(expectedSupported, supported);
    }
}
