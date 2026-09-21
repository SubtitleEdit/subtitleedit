using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.CutVideo;

/// <summary>
/// The ffmpeg command lines of "Cut video": the ranges to keep are trimmed out of the input and
/// concatenated, with one leg per stream type the input actually has.
/// </summary>
public class CutVideoParametersTests
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

    [Fact]
    public void Merge_VideoAndAudio_TrimsBothAndConcatenatesThem()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((1, 2), (5, 7.5)), hasVideo: true);

        Assert.Contains("[0:v]trim=start=1:end=2,setpts=PTS-STARTPTS[v0]; [0:a]atrim=start=1:end=2,asetpts=PTS-STARTPTS[a0]", args);
        Assert.Contains("[0:a]atrim=start=5:end=7.5,asetpts=PTS-STARTPTS[a1]", args);
        Assert.Contains("[v0][a0][v1][a1]concat=n=2:v=1:a=1[outv][outa]", args);
        Assert.Contains("-map \"[outv]\" -map \"[outa]\" -c:v libx264", args);
        Assert.Contains("-c:a aac -b:a 192k", args);
    }

    /// <summary>
    /// The graph always referenced "[0:a]", so a video without an audio track failed with
    /// "Stream specifier ':a' ... matches no streams".
    /// </summary>
    [Fact]
    public void Merge_VideoWithoutAudio_HasNoAudioLeg()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((1, 2)), hasVideo: true, hasAudio: false);

        Assert.DoesNotContain("[0:a]", args);
        Assert.DoesNotContain("-c:a", args);
        Assert.Contains("[v0]concat=n=1:v=1:a=0[outv]\"", args);
        Assert.Contains("-map \"[outv]\" -c:v libx264", args);
    }

    [Fact]
    public void Remove_VideoWithoutAudio_HasNoAudioLeg()
    {
        var args = FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((10, 20)), hasVideo: true, hasAudio: false);

        Assert.DoesNotContain("[0:a]", args);
        Assert.Contains("[0:v]trim=start=0:end=10,setpts=PTS-STARTPTS[v0]; [0:v]trim=start=20,setpts=PTS-STARTPTS[v1]", args);
        Assert.Contains("[v0][v1]concat=n=2:v=1:a=0[outv]\"", args);
    }

    [Fact]
    public void Remove_KeepsWhatLiesBetweenTheSegmentsAndTheRestOfTheFile()
    {
        var args = FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((0, 5), (10, 20), (12, 15)), hasVideo: true);

        // Nothing before a segment that starts at zero, and the overlapped [12-15] does not
        // bring 15-20 back.
        Assert.Contains("[0:v]trim=start=5:end=10,", args);
        Assert.Contains("[0:v]trim=start=20,", args);
        Assert.Contains("concat=n=2:v=1:a=1[outv][outa]", args);
    }

    /// <summary>
    /// Audio-only output was always libmp3lame, so cutting a .wav gave a WAV file with an MP3
    /// stream inside.
    /// </summary>
    [Theory]
    [InlineData("out.wav", "-c:a pcm_s16le")]
    [InlineData("out.mp3", "-c:a libmp3lame -b:a 192k")]
    [InlineData("out.flac", "-c:a flac")]
    [InlineData("out.mkv", "-c:a aac -b:a 192k")]
    public void AudioOnly_EncoderFollowsTheOutputExtension(string outputFileName, string expected)
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.wav", outputFileName, MakeSegments((1, 2)), hasVideo: false);

        Assert.DoesNotContain("[0:v]", args);
        Assert.Contains("[a0]concat=n=1:v=0:a=1[outa]", args);
        Assert.Contains($"-map \"[outa]\" {expected} \"{outputFileName}\"", args);
    }
}
