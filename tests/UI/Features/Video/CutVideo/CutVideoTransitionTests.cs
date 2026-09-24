using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.CutVideo;

/// <summary>
/// "Cut video" with transitions: the kept ranges are joined with xfade/acrossfade, which overlap
/// the parts they join, so the ffmpeg offsets and the re-timed subtitle must agree on the same
/// whole-frame ranges.
/// </summary>
public class CutVideoTransitionTests
{
    private static List<SubtitleLineViewModel> MakeSegments(params (double Start, double End)[] ranges)
    {
        var segments = new List<SubtitleLineViewModel>();
        foreach (var range in ranges)
        {
            segments.Add(new SubtitleLineViewModel(new Paragraph(string.Empty, range.Start * 1000.0, range.End * 1000.0), new SubRip()));
        }

        return segments;
    }

    private static CutVideoTransitionOptions MakeOptions(double transitionSeconds = 0.5, double fadeIn = 0, double fadeOut = 0) => new()
    {
        Transition = "wipeleft",
        TransitionSeconds = transitionSeconds,
        FadeInSeconds = fadeIn,
        FadeOutSeconds = fadeOut,
        FrameRate = 50,
        InputDurationSeconds = 60,
    };

    [Fact]
    public void NoEffects_KeepsThePlainConcatCommandLine()
    {
        var plain = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((1, 2), (5, 7.5)), hasVideo: true);
        var withEmptyOptions = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((1, 2), (5, 7.5)), true, true, new CutVideoTransitionOptions());

        Assert.Equal(plain, withEmptyOptions);
    }

    /// <summary>
    /// A plain cut trimmed before making the video constant rate: a cut point inside a held picture
    /// dropped that picture, so the part's video ran up to the length of the hold ahead of its audio.
    /// </summary>
    [Fact]
    public void PlainCut_WithKnownFrameRate_MakesTheVideoConstantRateBeforeTrimming()
    {
        var options = new CutVideoTransitionOptions { FrameRate = 50, InputDurationSeconds = 60 };
        var args = FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((10, 20)), true, true, options);

        Assert.Contains("[0:v]fps=50:start_time=0,trim=start=0:end=9.99,setpts=PTS-STARTPTS,settb=AVTB,format=yuv420p[v0]", args);
        Assert.Contains("[0:v]fps=50:start_time=0,trim=start=19.99:end=59.99,", args); // the open end is the input duration
        Assert.Contains("[v0][a0][v1][a1]concat=n=2:v=1:a=1[vc][ac]; [vc]null[outv]; [ac]anull[outa]", args);
        Assert.DoesNotContain("xfade", args);
    }

    [Fact]
    public void PlainCut_WithUnknownFrameRate_KeepsThePlainConcatCommandLine()
    {
        var options = new CutVideoTransitionOptions { InputDurationSeconds = 60 };
        var plain = FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((10, 20)), hasVideo: true);

        Assert.Equal(plain, FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((10, 20)), true, true, options));
    }

    [Fact]
    public void Merge_ChainsXfadeAndAcrossfadeAtTheRunningOutputLength()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((2, 6), (10, 14), (20, 25)), true, true, MakeOptions());

        // Video is made constant rate from 0 before it is trimmed, half a frame (0.01 s at 50 fps) early.
        Assert.Contains("[0:v]fps=50:start_time=0,trim=start=1.99:end=5.99,setpts=PTS-STARTPTS,settb=AVTB,format=yuv420p[v0]", args);
        Assert.Contains("[0:a]atrim=start=2:end=6,asetpts=PTS-STARTPTS[a0]", args);
        Assert.Contains("[v0][v1]xfade=transition=wipeleft:duration=0.5:offset=3.5[vx1]", args);
        Assert.Contains("[a0][a1]acrossfade=d=0.5:c1=tri:c2=tri[ax1]", args);

        // 4 + 4 - 0.5 = 7.5 s of output before the second join.
        Assert.Contains("[vx1][v2]xfade=transition=wipeleft:duration=0.5:offset=7[vx2]", args);
        Assert.Contains("[vx2]null[outv]; [ax2]anull[outa]", args);
        Assert.DoesNotContain("concat", args);
    }

    [Fact]
    public void Fades_UseTheOutputLength()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((2, 6), (10, 14)), true, true, MakeOptions(fadeIn: 1, fadeOut: 2));

        // Output: 4 + 4 - 0.5 = 7.5 s, so the fade out starts at 5.5.
        Assert.Contains("[vx1]fade=t=in:st=0:d=1,fade=t=out:st=5.5:d=2[outv]", args);
        Assert.Contains("[ax1]afade=t=in:st=0:d=1,afade=t=out:st=5.5:d=2[outa]", args);
    }

    [Fact]
    public void FadesOnly_ConcatenatesThenFades()
    {
        var options = MakeOptions(transitionSeconds: 0, fadeIn: 1);
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((2, 6), (10, 14)), true, true, options);

        Assert.Contains("[v0][a0][v1][a1]concat=n=2:v=1:a=1[vc][ac]", args);
        Assert.Contains("[vc]fade=t=in:st=0:d=1[outv]", args);
        Assert.DoesNotContain("xfade", args);
    }

    [Fact]
    public void Remove_EndsTheLastRangeAtTheInputDurationAndDropsAnEmptyRemainder()
    {
        var args = FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((10, 20), (50, 60)), true, true, MakeOptions());

        Assert.Contains("[0:a]atrim=start=0:end=10,", args);
        Assert.Contains("[0:a]atrim=start=20:end=50,", args);
        Assert.DoesNotContain("atrim=start=60", args); // nothing is left after a segment ending at the end
        Assert.Contains("[v0][v1]xfade=transition=wipeleft:duration=0.5:offset=9.5[vx1]", args);
    }

    [Fact]
    public void AudioOnly_UsesAcrossfadeAndAfade()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.wav", "out.wav", MakeSegments((2, 6), (10, 14)), false, true, MakeOptions(fadeIn: 1));

        Assert.DoesNotContain("[0:v]", args);
        Assert.DoesNotContain("xfade", args);
        Assert.Contains("[a0][a1]acrossfade=d=0.5:c1=tri:c2=tri[ax1]; [ax1]afade=t=in:st=0:d=1[outa]", args);
        Assert.Contains("-c:a pcm_s16le", args);
    }

    [Fact]
    public void Plan_ShortensTheTransitionToHalfTheShortestRange()
    {
        var plan = CutVideoTransitionPlan.Create(
            new List<(double? Start, double? End)> { (0, 10), (20, 20.6), (30, 40) },
            MakeOptions(transitionSeconds: 2));

        Assert.Equal(0.3, plan.TransitionSeconds, 6); // half of the 0.6 s range
    }

    [Fact]
    public void Plan_PutsRangesOnWholeFramesWithExactNtscRate()
    {
        var options = MakeOptions();
        options.FrameRate = 29.97;
        var plan = CutVideoTransitionPlan.Create(new List<(double? Start, double? End)> { (1.01, 2.0), (3, 4) }, options);

        Assert.Equal("30000/1001", plan.FrameRateExpression);
        Assert.Equal(30 * 1001 / 30000.0, plan.Ranges[0].Start, 9); // frame 30
        Assert.Equal(60 * 1001 / 30000.0, plan.Ranges[0].End!.Value, 9); // frame 60
    }

    [Fact]
    public void Plan_WithUnknownDuration_KeepsTheOpenEndAndSkipsTheFadeOut()
    {
        var options = MakeOptions(fadeOut: 1);
        options.InputDurationSeconds = 0;
        var plan = CutVideoTransitionPlan.Create(new List<(double? Start, double? End)> { (0, 10), (20, null) }, options);

        Assert.Null(plan.Ranges[1].End);
        Assert.Null(plan.OutputSeconds);
        Assert.Equal(0, plan.FadeOutSeconds);
        Assert.Equal(0.5, plan.TransitionSeconds);
    }

    [Fact]
    public void Subtitle_SwitchesOverAtTheMiddleOfTheTransition()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("a", 1000, 3900)); // runs to the end of segment 1
        subtitle.Paragraphs.Add(new Paragraph("b", 10000, 12000)); // starts with segment 2
        subtitle.Paragraphs.Add(new Paragraph("c", 13000, 14000));

        var cut = SubtitleSegmentCutter.KeepSegments(subtitle, new List<(double, double)> { (0, 4), (10, 14) }, 1.0);

        Assert.Equal(3, cut.Paragraphs.Count);
        Assert.Equal(1000, cut.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3500, cut.Paragraphs[0].EndTime.TotalMilliseconds, 3); // clipped at 4 - 0.5
        Assert.Equal(3500, cut.Paragraphs[1].StartTime.TotalMilliseconds, 3); // 10.5 s -> 3 + 0.5
        Assert.Equal(5000, cut.Paragraphs[1].EndTime.TotalMilliseconds, 3);
        Assert.Equal(6000, cut.Paragraphs[2].StartTime.TotalMilliseconds, 3); // segment 2 starts at 4 - 1
    }

    [Fact]
    public void Subtitle_WithoutTransition_IsUnchanged()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("a", 1000, 3900));
        subtitle.Paragraphs.Add(new Paragraph("b", 10000, 12000));
        var segments = new List<(double, double)> { (0, 4), (10, 14) };

        var withZero = SubtitleSegmentCutter.KeepSegments(subtitle, segments, 0);
        var plain = SubtitleSegmentCutter.KeepSegments(subtitle, segments);

        Assert.Equal(plain.ToText(new SubRip()), withZero.ToText(new SubRip()));
    }
}
