using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// The audio stream index the user picked belongs to one video. Batch mode reuses the same view
/// model for other files, and a file whose audio is stream 0 and video stream 1 then got
/// "-map 0:1" aimed at its video - which -vn dropped, leaving ffmpeg with no streams to write
/// ("Output file does not contain any stream", exit -22) and the run aborting with "Generated
/// audio file not found" (#13781). The trailing "?" does not help: stream 1 exists, it is just
/// the wrong kind.
/// </summary>
public class SpeechToTextAudioMapTests
{
    private const string Video = @"G:\the-video.mp4";

    [Fact]
    public void MapsTheChosenTrack_ForTheVideoItWasPickedFrom()
    {
        Assert.Equal("-map 0:1?", SpeechToTextViewModel.BuildAudioMapParameter(Video, 1, Video));
    }

    // Windows paths are case insensitive, and the two strings reach here from different places.
    [Fact]
    public void MapsTheChosenTrack_WhenOnlyTheCasingDiffers()
    {
        Assert.Equal("-map 0:1?", SpeechToTextViewModel.BuildAudioMapParameter(Video.ToUpperInvariant(), 1, Video));
    }

    // The reported failure: another video in the batch, where stream 1 is video, not audio. No
    // map at all - ffmpeg's automatic selection honors the default-track flag, the same track a
    // fresh mpv plays and the main window follows on open (#13233). Not "-map 0:a:0?" (#13787,
    // reverted): the first track in container order can be commentary or audio description.
    [Fact]
    public void MapsNothing_ForAnotherFileInTheBatch()
    {
        Assert.Equal(string.Empty, SpeechToTextViewModel.BuildAudioMapParameter(@"G:\1.mp4", 1, Video));
    }

    // "Transcribe selected lines" feeds single-stream wav clips cut from the video.
    [Fact]
    public void MapsNothing_ForADemuxedAudioClip()
    {
        Assert.Equal(string.Empty, SpeechToTextViewModel.BuildAudioMapParameter(@"C:\Temp\se_audioclip_x.wav", 1, null));
    }

    // No track picked (no video loaded) - ffmpeg selects the default-flagged audio itself.
    [Fact]
    public void MapsNothing_WhenNoTrackWasChosen()
    {
        Assert.Equal(string.Empty, SpeechToTextViewModel.BuildAudioMapParameter(Video, -1, Video));
    }

    // Track 0 is a real choice, not "unset" - the reporter's file has its audio there.
    [Fact]
    public void MapsTrackZero()
    {
        Assert.Equal("-map 0:0?", SpeechToTextViewModel.BuildAudioMapParameter(Video, 0, Video));
    }
}
