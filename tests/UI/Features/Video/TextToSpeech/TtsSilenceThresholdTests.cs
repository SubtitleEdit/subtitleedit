using Nikse.SubtitleEdit.Features.Video.TextToSpeech;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// The TTS silence trim, VAD compression and noise gate judge silence relative to the clip's
/// peak (#14480). A fixed -40 dBFS threshold cut the last word off quiet voice-clone output:
/// measured on a real clip, a -21 dBFS peak lost its final "s" and a -33 dBFS peak was trimmed
/// to nothing.
/// </summary>
public class TtsSilenceThresholdTests
{
    [Fact]
    public void FullScaleClip_KeepsTheLegacyThreshold()
    {
        Assert.Equal(-40.0, TtsSilenceThreshold.ThresholdDbfs(0.0), 6);
        Assert.Equal(0.01, TtsSilenceThreshold.Amplitude(0.0), 6);
        Assert.Equal("-40.0dB", TtsSilenceThreshold.DbLiteral(0.0));
    }

    [Fact]
    public void UnknownPeak_FallsBackToTheLegacyThreshold()
    {
        Assert.Equal(-40.0, TtsSilenceThreshold.ThresholdDbfs(null), 6);
        Assert.Equal(-40.0, TtsSilenceThreshold.ThresholdDbfs(double.NaN), 6);
        Assert.Equal(-40.0, TtsSilenceThreshold.ThresholdDbfs(double.NegativeInfinity), 6);
        Assert.Equal(0.01, TtsSilenceThreshold.Amplitude(null), 6);
        Assert.Equal("-40.0dB", TtsSilenceThreshold.DbLiteral(null));
    }

    [Theory]
    [InlineData(-2.8, -42.8)]
    [InlineData(-20.8, -60.8)]
    [InlineData(-26.8, -66.8)]
    public void QuietClip_ThresholdFollowsThePeak(double peakDbfs, double expectedThresholdDbfs)
    {
        Assert.Equal(expectedThresholdDbfs, TtsSilenceThreshold.ThresholdDbfs(peakDbfs), 6);
        Assert.Equal(Math.Pow(10, expectedThresholdDbfs / 20), TtsSilenceThreshold.Amplitude(peakDbfs), 9);
    }

    [Fact]
    public void VeryQuietClip_IsClampedAtTheNoiseFloor()
    {
        // -32.8 dBFS peak - 40 dB would be -72.8 dBFS, under a neural vocoder's noise floor;
        // the clamp keeps trailing silence trimmable.
        Assert.Equal(TtsSilenceThreshold.FloorDbfs, TtsSilenceThreshold.ThresholdDbfs(-32.8), 6);
        Assert.Equal(TtsSilenceThreshold.FloorDbfs, TtsSilenceThreshold.ThresholdDbfs(-90.0), 6);
    }

    [Fact]
    public void HotFloatClip_NeverTrimsHarderThanTheLegacyThreshold()
    {
        // A float WAV peaking above 0 dBFS would otherwise get a threshold above 0.01.
        Assert.Equal(-40.0, TtsSilenceThreshold.ThresholdDbfs(3.5), 6);
    }

    [Fact]
    public void DbLiteral_IsInvariantCulture()
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("da-DK");
            Assert.Equal("-60.8dB", TtsSilenceThreshold.DbLiteral(-20.8));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ParsePeakDbfs_FindsVolumedetectsMaxVolume()
    {
        var lines = new[]
        {
            "Input #0, wav, from 'x.wav':",
            "[Parsed_volumedetect_0 @ 0x7f8] n_samples: 33168",
            "[Parsed_volumedetect_0 @ 0x7f8] mean_volume: -16.0 dB",
            "[Parsed_volumedetect_0 @ 0x7f8] max_volume: -2.8 dB",
            "[Parsed_volumedetect_0 @ 0x7f8] histogram_2db: 12",
        };

        Assert.Equal(-2.8, TtsSilenceThreshold.ParsePeakDbfs(lines));
    }

    [Fact]
    public void ParsePeakDbfs_HandlesPositiveAndIntegerPeaks()
    {
        Assert.Equal(0.0, TtsSilenceThreshold.ParsePeakDbfs(new[] { "[Parsed_volumedetect_0 @ 0x1] max_volume: 0 dB" }));
        Assert.Equal(1.2, TtsSilenceThreshold.ParsePeakDbfs(new[] { "[Parsed_volumedetect_0 @ 0x1] max_volume: 1.2 dB" }));
    }

    [Fact]
    public void ParsePeakDbfs_ReturnsNullWithoutAPeakLine()
    {
        Assert.Null(TtsSilenceThreshold.ParsePeakDbfs(Array.Empty<string>()));
        Assert.Null(TtsSilenceThreshold.ParsePeakDbfs(new[] { "x.wav: No such file or directory" }));
    }
}
