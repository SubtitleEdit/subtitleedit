using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class FfmpegGeneratorCloneReferenceClipTests
{
    [Fact]
    public void CloneReferenceClip_Default_CutsTheRangeAsIs()
    {
        var args = FfmpegGenerator.ExtractCloneReferenceClipParameters("video.mp4", 12.5, 0.8, "clip.wav");

        Assert.Contains("-ss 12.500 -t 0.800", args);
        Assert.Contains("-ar 24000 -ac 1 -c:a pcm_s16le", args);
        Assert.DoesNotContain("apad", args);
    }

    [Fact]
    public void CloneReferenceClip_WithMinimum_PadsUpToItWithSilence()
    {
        // #14480: Higgs Audio v3's reference encoder rejects clips under ~1 s, so a boxed-in
        // short line is padded rather than sent as cut. whole_dur leaves longer clips alone.
        var args = FfmpegGenerator.ExtractCloneReferenceClipParameters(
            "video.mp4", 12.5, 0.8, "clip.wav", audioTrackFfIndex: 2, sampleRate: 24000, minimumSeconds: 1.0);

        Assert.Contains("-map 0:2 -af apad=whole_dur=1 -vn", args);
    }

    [Fact]
    public void CloneReferenceClip_MinimumIsFormattedInvariant()
    {
        var args = FfmpegGenerator.ExtractCloneReferenceClipParameters(
            "video.mp4", 0, 0.5, "clip.wav", minimumSeconds: 1.25);

        Assert.Contains("apad=whole_dur=1.25", args);
    }

    [Fact]
    public void PerLineReference_ShortLineBoxedInByNeighbours_IsWhyTheMinimumExists()
    {
        // A 400 ms line with other lines right before and after it cannot grow into silence,
        // so the cut range stays under the engine minimum and only the padding saves it.
        var paragraphs = new List<Paragraph>
        {
            new("A", 0, 5000),
            new("B", 5000, 5400),
            new("C", 5400, 9000),
        };

        var range = PerLineVoiceClone.GetReferenceRange(paragraphs, 1, 60);

        Assert.True(range.DurationSeconds < PerLineVoiceClone.MinimumReferenceSeconds);
        Assert.Equal(0.4, range.DurationSeconds, 3);
    }
}
