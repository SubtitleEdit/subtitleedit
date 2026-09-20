using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// The video is stream-copied, so packets without a pts (MPEG-4 ASP/XviD with packed B-frames)
/// made the muxer abort with "Can't write packet with unknown timestamp". genpts is an input
/// option, so it has to come before the video's -i.
/// </summary>
public class AddAudioTrackGeneratePtsTests
{
    private const string Expected = "-nostdin -y -fflags +genpts -i \"in.mkv\" ";

    [Fact]
    public void AddAudioTrack_GeneratesMissingPtsForTheCopiedVideo()
    {
        using var process = FfmpegGenerator.AddAudioTrack("in.mkv", "tts.wav", "out.mkv", string.Empty, null);

        Assert.StartsWith(Expected, process.StartInfo.Arguments);
    }

    [Fact]
    public void AddAudioTrackWithDucking_GeneratesMissingPtsForTheCopiedVideo()
    {
        using var process = FfmpegGenerator.AddAudioTrackWithDucking("in.mkv", "tts.wav", "out.mkv", string.Empty, null, 15);

        Assert.StartsWith(Expected, process.StartInfo.Arguments);
    }

    [Fact]
    public void AddAudioTrackWithBackground_GeneratesMissingPtsForTheCopiedVideo()
    {
        using var process = FfmpegGenerator.AddAudioTrackWithBackground("in.mkv", "bg.wav", "tts.wav", "out.mkv", string.Empty, null, 15);

        Assert.StartsWith(Expected, process.StartInfo.Arguments);
    }
}
