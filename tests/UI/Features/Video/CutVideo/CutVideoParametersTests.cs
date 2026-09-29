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

        Assert.StartsWith("-y -ss 1 -t 1 -i \"in.mp4\" -ss 5 -t 2.5 -i \"in.mp4\" ", args);
        Assert.Contains("[0:v]setpts=PTS-STARTPTS[v0]; [0:a]asetpts=PTS-STARTPTS[a0]", args);
        Assert.Contains("[1:a]asetpts=PTS-STARTPTS[a1]", args);
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
        Assert.StartsWith("-y -t 10 -i \"in.mp4\" -ss 20 -i \"in.mp4\" ", args);
        Assert.Contains("[0:v]setpts=PTS-STARTPTS[v0]; [1:v]setpts=PTS-STARTPTS[v1]", args);
        Assert.Contains("[v0][v1]concat=n=2:v=1:a=0[outv]\"", args);
    }

    [Fact]
    public void Remove_KeepsWhatLiesBetweenTheSegmentsAndTheRestOfTheFile()
    {
        var args = FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((0, 5), (10, 20), (12, 15)), hasVideo: true);

        // Nothing before a segment that starts at zero, and the overlapped [12-15] does not
        // bring 15-20 back.
        Assert.StartsWith("-y -ss 5 -t 5 -i \"in.mp4\" -ss 20 -i \"in.mp4\" ", args);
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

        Assert.DoesNotContain(":v]", args);
        Assert.Contains("[a0]concat=n=1:v=0:a=1[outa]", args);
        Assert.Contains($"-map \"[outa]\" {expected} \"{outputFileName}\"", args);
    }

    /// <summary>
    /// A trim filter on the whole input decoded everything before a range, so a short clip from
    /// late in a long video took minutes. Each range is now seeked to with -ss before its -i.
    /// </summary>
    [Fact]
    public void Merge_SeeksToEachRangeInsteadOfDecodingFromTheStart()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((3600, 3610)), hasVideo: true);

        Assert.StartsWith("-y -ss 3600 -t 10 -i \"in.mp4\" -filter_complex ", args);
        Assert.DoesNotContain("trim=", args);
    }

    /// <summary>
    /// A .ts has no seek index: "-ss" lands on a non-keyframe and the picture of the range started
    /// up to a GOP late. The input is opened earlier and the range trimmed exactly.
    /// </summary>
    [Fact]
    public void Merge_TransportStream_SeeksEarlierAndTrimsToTheRange()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.ts", "out.mp4", MakeSegments((20.5, 25.5), (5, 8)), hasVideo: true);

        Assert.StartsWith("-y -ss 5.5 -t 20 -i \"in.ts\" -t 8 -i \"in.ts\" -filter_complex ", args);
        Assert.Contains("[0:v]trim=start=15:end=20,setpts=PTS-STARTPTS[v0]", args);
        Assert.Contains("[0:a]atrim=start=15:end=20,asetpts=PTS-STARTPTS[a0]", args);
        Assert.Contains("[1:v]trim=start=5:end=8,setpts=PTS-STARTPTS[v1]", args);
    }

    /// <summary>
    /// Many ranges share one input (seeked to the first range, stopped after the last) rather
    /// than opening the video once per range.
    /// </summary>
    [Fact]
    public void Merge_ManyRanges_ShareOneSeekedInput()
    {
        var ranges = new List<(double Start, double End)>();
        for (var i = 0; i < 40; i++)
        {
            ranges.Add((100 + i * 10, 105 + i * 10));
        }

        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments(ranges.ToArray()), hasVideo: true);

        Assert.StartsWith("-y -ss 100 -t 395 -i \"in.mp4\" -filter_complex ", args);
        Assert.Contains("[0:v]trim=start=0:end=5,setpts=PTS-STARTPTS[v0]", args);
        Assert.Contains("[0:a]atrim=start=390:end=395,asetpts=PTS-STARTPTS[a39]", args);
        Assert.DoesNotContain("[1:v]", args);
    }

    [Theory]
    [InlineData("libx264", "-c:v libx264 -preset veryfast -crf 23 ")]
    [InlineData("libx265", "-c:v libx265 -preset veryfast -crf 26 -tag:v hvc1 ")]
    [InlineData("h264_nvenc", "-c:v h264_nvenc -preset p4 -rc vbr -cq 23 -b:v 0 ")]
    [InlineData("hevc_nvenc", "-c:v hevc_nvenc -preset p4 -rc vbr -cq 23 -b:v 0 -tag:v hvc1 ")]
    [InlineData("h264_qsv", "-c:v h264_qsv -preset veryfast -global_quality 23 ")]
    [InlineData("h264_amf", "-c:v h264_amf -quality balanced -rc cqp -qp_i 22 -qp_p 24 ")]
    [InlineData("h264_videotoolbox", "-c:v h264_videotoolbox -q:v 65 ")]
    public void VideoEncoderIsTheChosenOne(string videoEncoding, string expected)
    {
        var merge = FfmpegGenerator.GetMergeSegmentsParameters("in.mp4", "out.mp4", MakeSegments((1, 2)), hasVideo: true, videoEncoding: videoEncoding);
        var remove = FfmpegGenerator.GetRemoveSegmentsParameters("in.mp4", "out.mp4", MakeSegments((1, 2)), hasVideo: true, videoEncoding: videoEncoding);

        Assert.Contains(expected, merge);
        Assert.Contains(expected, remove);
    }

    [Fact]
    public void AudioOnly_IgnoresTheVideoEncoder()
    {
        var args = FfmpegGenerator.GetMergeSegmentsParameters("in.wav", "out.wav", MakeSegments((1, 2)), hasVideo: false, videoEncoding: "h264_nvenc");

        Assert.DoesNotContain("-c:v", args);
    }
}
