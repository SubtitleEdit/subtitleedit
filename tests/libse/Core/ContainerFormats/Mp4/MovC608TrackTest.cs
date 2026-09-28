using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class MovC608TrackTest
{
    /// <summary>
    /// sample_MOV_C608.mov is ffmpeg's "-c:s copy" of a Scenarist .scc into a QuickTime "c608"
    /// closed caption track (#15382). Each sample wraps its byte pairs in a "cdat" atom, which
    /// used to be decoded as text too ("Hello worldcdat"), and the track's edit list (a 264 ms
    /// delay) was ignored, so every caption came 264 ms early.
    /// </summary>
    [Fact]
    public void C608Track_DecodesCdatAtomsAndHonoursEditList()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MOV_C608.mov");

        var parser = new MP4Parser(path);
        var track = Assert.Single(parser.GetSubtitleTracks());
        Assert.True(track.Mdia.IsClosedCaption);

        var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();
        Assert.Equal(2, paragraphs.Count);
        Assert.Equal("Hello world", paragraphs[0].Text);
        Assert.Equal(1000, paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(3000, paragraphs[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal("Second line", paragraphs[1].Text);
        Assert.Equal(4000, paragraphs[1].StartTime.TotalMilliseconds, 1);
        Assert.Equal(6000, paragraphs[1].EndTime.TotalMilliseconds, 1);
    }
}
