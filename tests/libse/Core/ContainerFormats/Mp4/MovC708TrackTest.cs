using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class MovC708TrackTest
{
    /// <summary>
    /// sample_MOV_C708.mov is the QuickTime "c708" closed caption track of a user's file (the
    /// other tracks dropped): each sample is a "ccdp" atom holding a SMPTE 334 caption
    /// distribution packet with CEA-608 pairs and CEA-708 packets. It was read as tx3g text - a
    /// zero length - so the file had "no subtitle tracks".
    /// </summary>
    [Fact]
    public void C708Track_DecodesCea608AndCea708()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MOV_C708.mov");

        var parser = new MP4Parser(path);

        Assert.Equal(new[] { 1, 101 }, parser.ClosedCaptionTracks.Keys);
        Assert.Equal(8, parser.ClosedCaptionTracks[1].Count); // CC1
        Assert.Equal(9, parser.ClosedCaptionTracks[101].Count); // CEA-708 service 1
        Assert.InRange(parser.ClosedCaptionTracks[1][0].StartTime.TotalMilliseconds, 590, 610);
        Assert.StartsWith("N/A, N/A, N/A", parser.ClosedCaptionTracks[1][0].Text);
    }
}
