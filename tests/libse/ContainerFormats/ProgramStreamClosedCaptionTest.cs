using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using System.Collections.Generic;
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

    /// <summary>
    /// DVD caption user data with only null padding for the first 70 seconds: the stream has
    /// captions, so reading must not give up after the 60 second probe.
    /// </summary>
    [Fact]
    public void ReadsDvdCaptionsStartingAfterProbeTime()
    {
        var pairs = new Dictionary<int, byte[]>
        {
            { 70, new byte[] { 0x14, 0x20 } }, // RCL
            { 71, Ascii("HE") },
            { 72, Ascii("LL") },
            { 73, Ascii("O ") },
            { 74, Ascii("WO") },
            { 75, Ascii("RL") },
            { 76, new byte[] { 0x44, 0x80 } }, // "D"
            { 77, new byte[] { 0x14, 0x2F } }, // EOC
            { 79, new byte[] { 0x14, 0x2C } }, // EDM
        };

        var pictures = new List<byte[]>();
        for (var second = 0; second < 80; second++)
        {
            var pair = pairs.TryGetValue(second, out var p) ? p : new byte[] { 0x80, 0x80 };
            var dvdUserData = new byte[] { 0x43, 0x43, 0x01, 0xF8, 0x82, 0xFF, pair[0], pair[1], 0xFE, 0x80, 0x80 };
            pictures.Add(Concat(GopHeader, UserData(dvdUserData), PictureHeader));
        }

        var tracks = ProgramStreamClosedCaptionReader.Read(new MemoryStream(BuildProgramStream(pictures, 0)), ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null);

        var cue = Assert.Single(Assert.Single(tracks).Value);
        Assert.Equal("HELLO WORLD", cue.Text);
        Assert.InRange(cue.StartTime.TotalMilliseconds, 76_900, 77_100);
    }

    /// <summary>
    /// The same captions sent both as ATSC A/53 (GA94) and SCTE 20 user data on every picture are
    /// decoded once - the stream is locked to the first form found.
    /// </summary>
    [Fact]
    public void ReadsAtscAndScte20DuplicateOnce()
    {
        var pictures = new List<byte[]>();
        foreach (var pair in HelloWorldPopOn())
        {
            pictures.Add(Concat(GopHeader, PictureHeader, UserData(AtscUserData(pair)), UserData(Scte20UserData(pair))));
        }

        var tracks = ProgramStreamClosedCaptionReader.Read(new MemoryStream(BuildProgramStream(pictures, 0)), ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null);

        var cue = Assert.Single(Assert.Single(tracks).Value);
        Assert.Equal("HELLO WORLD", cue.Text);
    }

    /// <summary>
    /// The 33-bit PTS wraps (every ~26.5 hours) in the middle of a caption - times keep going
    /// forward, like in the transport stream reader.
    /// </summary>
    [Fact]
    public void PtsWrapKeepsTimesInOrder()
    {
        var pictures = new List<byte[]>();
        foreach (var pair in HelloWorldPopOn())
        {
            pictures.Add(Concat(GopHeader, PictureHeader, UserData(AtscUserData(pair))));
        }

        // the first picture is 5 seconds before the wrap - the caption is shown at 7 s (after the wrap)
        var startPts = (1L << 33) - 5 * 90_000;
        var tracks = ProgramStreamClosedCaptionReader.Read(new MemoryStream(BuildProgramStream(pictures, startPts)), ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null);

        var cue = Assert.Single(Assert.Single(tracks).Value);
        Assert.Equal("HELLO WORLD", cue.Text);
        Assert.Equal(7000, cue.StartTime.TotalMilliseconds, 0);
        Assert.Equal(9000, cue.EndTime.TotalMilliseconds, 0);
    }

    /// <summary>
    /// A bare .m2v (video elementary stream, no PES timestamps) of film with 3:2 pulldown: four
    /// pictures per GOP that all repeat a field last six frames, and the DVD caption user data
    /// holds a block per frame. Spacing the GOPs by their four pictures overlapped them and
    /// interleaved the caption bytes of neighbouring GOPs.
    /// </summary>
    [Fact]
    public void ElementaryStreamWithPulldownKeepsCaptionBytesInOrder()
    {
        var pairs = HelloWorldPopOn();
        pairs.InsertRange(9, Enumerable.Repeat(new byte[] { 0x80, 0x80 }, 10)); // EDM 10 frames later
        var es = new List<byte> { 0, 0, 1, 0xB3, 0x2D, 0x01, 0xE0, 0x34, 0x12, 0x4F, 0xA3, 0x80 }; // 720x480, 29.97 fps
        for (var gop = 0; gop * 6 < pairs.Count; gop++)
        {
            var dvdUserData = new List<byte> { 0x43, 0x43, 0x01, 0xF8, 0x86 };
            for (var frame = gop * 6; frame < gop * 6 + 6; frame++)
            {
                var pair = frame < pairs.Count ? pairs[frame] : new byte[] { 0x80, 0x80 };
                dvdUserData.AddRange(new byte[] { 0xFF, pair[0], pair[1], 0xFE, 0x80, 0x80 });
            }

            es.AddRange(Concat(GopHeader, UserData(dvdUserData.ToArray())));
            foreach (var temporalReference in new[] { 0, 1, 2, 3 })
            {
                es.AddRange(new byte[] { 0, 0, 1, 0x00, 0x00, (byte)((temporalReference << 6) | 0x08), 0xFF, 0xF8 });
                es.AddRange(new byte[] { 0, 0, 1, 0xB5, 0x8F, 0xFF, 0x03, 0x82, 0x80 }); // frame picture, repeat_first_field
            }
        }

        var tracks = ProgramStreamClosedCaptionReader.ReadElementaryStream(new MemoryStream(es.ToArray()), ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null);

        var cue = Assert.Single(Assert.Single(tracks).Value);
        Assert.Equal("HELLO WORLD", cue.Text);
        Assert.Equal(7 * 1001 / 30.0, cue.StartTime.TotalMilliseconds, tolerance: 2); // EOC is frame 7
        Assert.Equal(19 * 1001 / 30.0, cue.EndTime.TotalMilliseconds, tolerance: 2); // EDM is frame 19
    }

    // One pair per picture (one picture per second): RCL, "HELLO WORLD", EOC at 7 s, padding, EDM at 9 s
    private static List<byte[]> HelloWorldPopOn()
    {
        return new List<byte[]>
        {
            new byte[] { 0x14, 0x20 },
            Ascii("HE"), Ascii("LL"), Ascii("O "), Ascii("WO"), Ascii("RL"), new byte[] { 0x44, 0x80 },
            new byte[] { 0x14, 0x2F },
            new byte[] { 0x80, 0x80 },
            new byte[] { 0x14, 0x2C },
        };
    }

    private static readonly byte[] GopHeader = { 0, 0, 1, 0xB8, 0x00, 0x08, 0x00, 0x40 };
    private static readonly byte[] PictureHeader = { 0, 0, 1, 0x00, 0x00, 0x0F, 0xFF, 0xF8 }; // temporal_reference 0, I picture

    private static byte[] Ascii(string text) => System.Text.Encoding.ASCII.GetBytes(text);

    private static byte[] Concat(params byte[][] parts) => parts.SelectMany(p => p).ToArray();

    private static byte[] UserData(byte[] data) => Concat(new byte[] { 0, 0, 1, 0xB2 }, data);

    private static byte[] AtscUserData(byte[] pair) => new byte[] { 0x47, 0x41, 0x39, 0x34, 0x03, 0x41, 0xFF, 0xFC, pair[0], pair[1], 0xFF };

    // user_data_type_code 3, 0x81, cc_count 1, then priority, field 1, line offset, the two bytes (reversed), marker
    private static byte[] Scte20UserData(byte[] pair)
    {
        static string Reverse(int b) => new string(System.Convert.ToString(b, 2).PadLeft(8, '0').Reverse().ToArray());
        var bits = "00001" + "00" + "01" + "10101" + Reverse(pair[0]) + Reverse(pair[1]) + "1";
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '0');
        var userData = new List<byte> { 0x03, 0x81 };
        for (var i = 0; i < bits.Length; i += 8)
        {
            userData.Add(System.Convert.ToByte(bits.Substring(i, 8), 2));
        }

        return userData.ToArray();
    }

    // MPEG-2 program stream: a pack header, then one video PES packet per picture, one second apart
    private static byte[] BuildProgramStream(List<byte[]> pictures, long startPts)
    {
        var ps = new List<byte> { 0, 0, 1, 0xBA, 0x44, 0x00, 0x04, 0x00, 0x04, 0x01, 0x01, 0x89, 0xC3, 0xF8 };
        for (var i = 0; i < pictures.Count; i++)
        {
            var pts = (startPts + i * 90_000L) & ((1L << 33) - 1);
            var pesBody = new List<byte> { 0x80, 0x80, 0x05 };
            pesBody.AddRange(new[]
            {
                (byte)(0x21 | ((pts >> 29) & 0x0E)),
                (byte)(pts >> 22),
                (byte)(0x01 | ((pts >> 14) & 0xFE)),
                (byte)(pts >> 7),
                (byte)(0x01 | ((pts << 1) & 0xFE)),
            });
            pesBody.AddRange(pictures[i]);
            ps.AddRange(new byte[] { 0, 0, 1, 0xE0, (byte)(pesBody.Count >> 8), (byte)pesBody.Count });
            ps.AddRange(pesBody);
        }

        return ps.ToArray();
    }
}
