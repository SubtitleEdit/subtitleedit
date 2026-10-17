using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace UITests.Logic;

/// <summary>
/// Video > Sync audio track: the ffmpeg command that copies a video with one audio track shifted.
/// Checked against real ffmpeg 4.4 and 9.0 by decoding a beep at 2.0 s: +0.5 s puts it at 2.5 s,
/// -0.5 s at 1.5 s, with the video still starting at 0 in .mkv and .mp4.
/// </summary>
public class FfmpegShiftAudioTrackTests
{
    private static readonly int[] Streams = { 0, 1, 2, 3 };

    [Fact]
    public void Later_KeepsStreamOrder_TakesTrackFromShiftedInput()
    {
        var args = FfmpegGenerator.GetShiftAudioTrackArguments("in.mkv", "out.mkv", Streams, 2, 2, 0.5, dropNegativePackets: true);

        Assert.Contains("-i \"in.mkv\" -itsoffset 0.500 -i \"in.mkv\" -map 0:0 -map 0:1 -map 1:2 -map 0:3 -c copy ", args);
        Assert.DoesNotContain("avoid_negative_ts", args);
        Assert.DoesNotContain("-bsf", args);
        Assert.EndsWith("-map_metadata 0 -map_chapters 0 \"out.mkv\"", args);
        Assert.StartsWith(FfmpegProgressTracker.ProgressArguments, args);
    }

    /// <summary>
    /// ffmpeg's default shifts the whole .mkv when the moved audio starts before zero - the video
    /// then started at the offset and no longer matched the subtitles.
    /// </summary>
    [Fact]
    public void Earlier_AllowsNegativeTimestamps_AndDropsThemWhenSupported()
    {
        var withDrop = FfmpegGenerator.GetShiftAudioTrackArguments("in.mkv", "out.mkv", Streams, 2, 2, -0.25, dropNegativePackets: true);
        Assert.Contains("-itsoffset -0.250 ", withDrop);
        Assert.Contains("-bsf:2 \"noise=drop=lt(pts*tb\\,0)\" -avoid_negative_ts disabled ", withDrop);

        var withoutDrop = FfmpegGenerator.GetShiftAudioTrackArguments("in.mkv", "out.mkv", Streams, 2, 2, -0.25, dropNegativePackets: false);
        Assert.Contains("-c copy -avoid_negative_ts disabled -map_metadata", withoutDrop);
        Assert.DoesNotContain("-bsf", withoutDrop);
    }

    [Fact]
    public void NoiseHelp_DropOption_NotDropAmount()
    {
        const string ffmpeg44 = "noise AVOptions:\n  -amount            <string>     ...VA...B..\n  -dropamount        <int>        ...VA...B.. (from 0 to INT_MAX) (default 0)\n";
        const string ffmpeg9 = "noise AVOptions:\n  -amount            <string>     ...VA...B..\n  -drop              <string>     ...VA...B..\n  -dropamount        <int>        ...VA...B.. (from 0 to INT_MAX) (default 0)\n";

        Assert.False(FfmpegGenerator.NoiseHelpHasDropOption(ffmpeg44));
        Assert.True(FfmpegGenerator.NoiseHelpHasDropOption(ffmpeg9));
    }

    [Fact]
    public void UnknownStreamIndexes_AppendsTheShiftedTrack()
    {
        var args = FfmpegGenerator.GetShiftAudioTrackArguments("in.mp4", "out.mp4", Array.Empty<int>(), 1, 1, 1.0, dropNegativePackets: true);

        Assert.Contains("-map 0 -map -0:a:0 -map -0:d -map 1:a:0 -c copy ", args);
    }

    [Fact]
    public void MediaInfo_HasStreamIndexForEveryStream()
    {
        const string log =
            "Input #0, matroska,webm, from 'in.mkv':\n" +
            "  Duration: 00:00:06.00, start: 0.000000, bitrate: 128 kb/s\n" +
            "  Stream #0:0: Video: h264 (High), yuv420p(progressive), 640x360, 25 fps, 25 tbr, 1k tbn (default)\n" +
            "  Stream #0:1(eng): Audio: aac (LC), 48000 Hz, stereo, fltp (default)\n" +
            "  Stream #0:2[0x3](dan): Audio: ac3, 48000 Hz, 5.1(side), fltp, 384 kb/s\n" +
            "  Stream #0:3: Attachment: ttf\n";

        var info = FfmpegMediaInfo2.ParseLog(log);

        Assert.Equal(new[] { 0, 1, 2, 3 }, info.Tracks.Select(t => t.StreamIndex));
        Assert.Equal(new[] { FfmpegTrackType.Audio, FfmpegTrackType.Audio }, info.Tracks.Where(t => t.StreamIndex is 1 or 2).Select(t => t.TrackType));
        Assert.Equal("dan", info.Tracks[2].Language);
    }
}
