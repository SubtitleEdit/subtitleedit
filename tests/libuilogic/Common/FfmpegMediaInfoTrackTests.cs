using Nikse.SubtitleEdit.UiLogic.Media;
using System.Linq;
using Xunit;

namespace LibUiLogicTests.Common;

public class FfmpegMediaInfoTrackTests
{
    // Real "ffmpeg -i" shape: the video is stream 0, so the first audio track is stream 1.
    // Callers hold these global stream indexes (they pass them to "-map 0:N"), which is why
    // the track lookup has to match on them rather than on the position in the audio list.
    private const string LogVideoPlusTwoAudio = @"
Input #0, matroska,webm, from 'movie.mkv':
  Duration: 00:01:00.00, start: 0.000000, bitrate: 1234 kb/s
    Stream #0:0(eng): Video: h264 (High), yuv420p, 1920x1080, 23.98 fps, 23.98 tbr
    Stream #0:1(eng): Audio: aac (LC), 48000 Hz, stereo, fltp, 128 kb/s
    Stream #0:2(eng): Audio: dts (DTS-HD MA), 48000 Hz, 5.1(side), s32p, 1536 kb/s
";

    [Fact]
    public void ParseLog_CapturesGlobalStreamIndex()
    {
        var info = FfmpegMediaInfo.ParseLog(LogVideoPlusTwoAudio);

        var audio = info.Tracks.Where(t => t.TrackType == FfmpegTrackType.Audio).ToList();
        Assert.Equal(2, audio.Count);
        Assert.Equal(1, audio[0].StreamIndex);
        Assert.Equal(2, audio[1].StreamIndex);
        Assert.Equal(0, info.Tracks.First(t => t.TrackType == FfmpegTrackType.Video).StreamIndex);
    }

    // The 5.1 track is stream 2 - the second audio track but the third stream. Indexing the
    // audio-only list with the global index missed it entirely (2 >= 2 audio tracks => false),
    // silently disabling "use center channel only".
    [Fact]
    public void HasFrontCenterAudio_MatchesOnGlobalStreamIndex()
    {
        var info = FfmpegMediaInfo.ParseLog(LogVideoPlusTwoAudio);

        Assert.True(info.HasFrontCenterAudio(2));   // the 5.1 track
        Assert.False(info.HasFrontCenterAudio(1));  // the stereo track
    }

    [Fact]
    public void HasFrontCenterAudio_NegativeIndexUsesFirstAudioTrack()
    {
        var info = FfmpegMediaInfo.ParseLog(LogVideoPlusTwoAudio);

        // First audio track is stereo.
        Assert.False(info.HasFrontCenterAudio(-1));

        var surroundFirst = FfmpegMediaInfo.ParseLog(@"
Input #0, matroska,webm, from 'movie.mkv':
    Stream #0:0(eng): Video: h264 (High), yuv420p, 1920x1080, 23.98 fps, 23.98 tbr
    Stream #0:1(eng): Audio: dts (DTS-HD MA), 48000 Hz, 7.1, s32p, 1536 kb/s
");
        Assert.True(surroundFirst.HasFrontCenterAudio(-1));
    }

    [Fact]
    public void HasFrontCenterAudio_UnknownStreamIndexIsFalse()
    {
        var info = FfmpegMediaInfo.ParseLog(LogVideoPlusTwoAudio);

        Assert.False(info.HasFrontCenterAudio(99));
    }

    [Fact]
    public void HasFrontCenterAudio_NoAudioTracksIsFalse()
    {
        var info = FfmpegMediaInfo.ParseLog(@"
Input #0, matroska,webm, from 'movie.mkv':
    Stream #0:0(eng): Video: h264 (High), yuv420p, 1920x1080, 23.98 fps, 23.98 tbr
");
        Assert.False(info.HasFrontCenterAudio(0));
        Assert.False(info.HasFrontCenterAudio(-1));
    }
}
