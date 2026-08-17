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

    // The reported failure: another video in the batch, where stream 1 is video, not audio. The
    // audio-relative map is valid for any layout, and matches the first-in-container-order track
    // the source-decoding engines (Purfview XXL, whisper-ctranslate2) pick - ffmpeg's automatic
    // selection would follow the default disposition instead, which is not always the first track.
    [Fact]
    public void MapsFirstAudioTrack_ForAnotherFileInTheBatch()
    {
        Assert.Equal("-map 0:a:0?", SpeechToTextViewModel.BuildAudioMapParameter(@"G:\1.mp4", 1, Video));
    }

    // "Transcribe selected lines" feeds single-stream wav clips cut from the video.
    [Fact]
    public void MapsFirstAudioTrack_ForADemuxedAudioClip()
    {
        Assert.Equal("-map 0:a:0?", SpeechToTextViewModel.BuildAudioMapParameter(@"C:\Temp\se_audioclip_x.wav", 1, null));
    }

    // No track picked (no video loaded) - first audio track, same as the direct-source engines.
    [Fact]
    public void MapsFirstAudioTrack_WhenNoTrackWasChosen()
    {
        Assert.Equal("-map 0:a:0?", SpeechToTextViewModel.BuildAudioMapParameter(Video, -1, Video));
    }

    // Track 0 is a real choice, not "unset" - the reporter's file has its audio there.
    [Fact]
    public void MapsTrackZero()
    {
        Assert.Equal("-map 0:0?", SpeechToTextViewModel.BuildAudioMapParameter(Video, 0, Video));
    }
}
