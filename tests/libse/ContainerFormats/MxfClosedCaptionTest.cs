using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;
using System.IO;

namespace LibSETests.ContainerFormats;

public class MxfClosedCaptionTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    /// <summary>
    /// ffmpeg fate-suite Closedcaption_rollup.m2v captions muxed by ffmpeg into MXF as a SMPTE 436M
    /// ANC track (eia608_to_smpte436m, 8-bit luma sample coding, 29.97 fps edit rate) - ffmpeg
    /// reads the same two roll-up cues back from it (smpte436m_to_eia608).
    /// </summary>
    [Fact]
    public void ReadsCea608AndCea708FromSmpte436MAncTrack()
    {
        var parser = new MxfParser(FilePath("sample_mxf_436m_cea608_708.mxf"));

        Assert.True(parser.IsValid);
        Assert.Empty(parser.GetSubtitles());
        Assert.Equal(new[] { 1, 101 }, parser.ClosedCaptionTracks.Keys);

        var cc1 = parser.ClosedCaptionTracks[1];
        Assert.Equal("(<i>inaudible radio chatter</i>)", cc1[0].Text);
        Assert.InRange(cc1[0].StartTime.TotalMilliseconds, 933, 935); // frame 28 at 30000/1001
        Assert.Equal("(<i>inaudible radio chatter</i>)" + System.Environment.NewLine + ">> Safety remains our number one", cc1[1].Text);

        Assert.Equal(">> Safety remains our number one", parser.ClosedCaptionTracks[101][1].Text);
    }
}
