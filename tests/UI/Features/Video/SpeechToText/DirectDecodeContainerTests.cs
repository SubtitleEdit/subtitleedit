using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

/// <summary>
/// #16772: Purfview XXL could not decode the VBR MP3 inside some AVI files ("Header missing",
/// 0 audio seconds), so only well-behaved containers are sent to the engine directly; the rest
/// get SE's extracted WAV.
/// </summary>
public class DirectDecodeContainerTests
{
    [Theory]
    [InlineData(@"C:\video\movie.mp4")]
    [InlineData(@"C:\video\movie.MKV")]
    [InlineData("/home/user/clip.webm")]
    [InlineData("/home/user/clip.mov")]
    [InlineData("/home/user/audio.m4a")]
    [InlineData("/home/user/audio.flac")]
    public void WellBehavedContainers_AreSentDirectly(string fileName)
    {
        Assert.True(SpeechToTextViewModel.IsDirectDecodeContainer(fileName));
    }

    [Theory]
    [InlineData(@"C:\video\movie.avi")]
    [InlineData(@"C:\video\movie.AVI")]
    [InlineData(@"C:\video\movie.wmv")]
    [InlineData(@"C:\video\movie.ts")]
    [InlineData(@"C:\video\movie.vob")]
    [InlineData(@"C:\video\movie")]
    public void OtherContainers_GoThroughWav(string fileName)
    {
        Assert.False(SpeechToTextViewModel.IsDirectDecodeContainer(fileName));
    }
}
