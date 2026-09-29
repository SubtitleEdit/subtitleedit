using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using System.IO;
using System.Linq;

namespace LibSETests.ContainerFormats;

/// <summary>
/// DVD style Line 21 captions ("CC" GOP user data) in MPEG program streams, built from the caption
/// stream of ffmpeg's fate-suite Closedcaption_rollup.m2v: the video re-encoded without captions
/// (closed GOPs), one DVD caption user data packet per GOP with a caption block per frame, muxed by
/// ffmpeg as MPEG-2 (.vob) and MPEG-1 (.mpg) program streams. ffmpeg's decoder reads the same two
/// roll-up cues from both (it puts a whole GOP's captions on one frame, so its times are coarser).
/// </summary>
public class ProgramStreamClosedCaptionTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    /// <summary>
    /// Caption blocks go to the GOP's frames in display order, timed from the picture PTS and
    /// temporal_reference - the same cues and times as the TS/MKV/MXF samples of these captions.
    /// </summary>
    [Theory]
    [InlineData("sample_vob_dvd_captions.vob")]
    [InlineData("sample_mpg_dvd_captions.mpg")]
    public void ReadsDvdCaptions(string fileName)
    {
        var path = FilePath(fileName);
        Assert.True(ProgramStreamClosedCaptionReader.IsProgramStream(path));

        var tracks = ProgramStreamClosedCaptionReader.Read(path, ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null);

        var cc1 = Assert.Single(tracks).Value;
        Assert.Equal("(<i>inaudible radio chatter</i>)", cc1[0].Text);
        Assert.InRange(cc1[0].StartTime.TotalMilliseconds, 966, 970);
        Assert.Equal("(<i>inaudible radio chatter</i>)" + System.Environment.NewLine + ">> Safety remains our number one", cc1[1].Text);
        Assert.InRange(cc1[1].StartTime.TotalMilliseconds, 3101, 3105);
    }

    /// <summary>
    /// Only the last PES packet has a PTS: GOPs without one continue from the previous GOP, so the
    /// captions still come out in the right order.
    /// </summary>
    [Fact]
    public void ReadsDvdCaptionsWithoutPts()
    {
        var tracks = ProgramStreamClosedCaptionReader.Read(FilePath("sample_vob_dvd_captions_one_pts.vob"), ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null);

        var cc1 = Assert.Single(tracks).Value;
        Assert.Equal("(<i>inaudible radio chatter</i>)", cc1[0].Text);
        Assert.Equal("(<i>inaudible radio chatter</i>)" + System.Environment.NewLine + ">> Safety remains our number one", cc1[1].Text);
    }

    /// <summary>
    /// A caption block is two caption words in field order, marker 0xFF for the odd field (CEA-608
    /// field 1) and 0xFE for the even field (field 2) - mapped like ffmpeg does it.
    /// </summary>
    [Theory]
    [InlineData(true, 0, 1)]
    [InlineData(false, 1, 0)]
    public void DvdCaptionFieldMapping(bool oddFieldFirst, int firstWordType, int secondWordType)
    {
        byte first = oddFieldFirst ? (byte)0xFF : (byte)0xFE;
        byte second = oddFieldFirst ? (byte)0xFE : (byte)0xFF;
        var userData = new byte[] { 0x43, 0x43, 0x01, 0xF8, (byte)((oddFieldFirst ? 0x80 : 0) | (2 << 1)), first, 0x94, 0x20, second, 0x15, 0x2C, first, 0xC8, 0x49, second, 0x80, 0x80 };

        var frames = GetCcDataHelper.ParseDvdCaptionUserData(userData);

        Assert.Equal(2, frames.Count);
        Assert.Equal(new[] { firstWordType, secondWordType }, frames[0].Select(p => p.Type));
        Assert.Equal(new[] { 0x94, 0x15 }, frames[0].Select(p => p.Data1));
        var single = Assert.Single(frames[1]); // the 0x80 0x80 padding word is left out
        Assert.Equal(0xC8, single.Data1);
    }
}
