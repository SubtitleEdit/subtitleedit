using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibSETests.ContainerFormats;

public class TransportStreamClosedCaptionTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    /// <summary>
    /// ffmpeg's fate-suite Closedcaption_rollup.m2v re-encoded with "-a53cc 1", so the CEA-608
    /// roll-up captions travel as cc_data in H.264 SEI (first file) and in MPEG-2 video user data
    /// (second file). ffmpeg's own decoder (lavfi movie=...[out0+subcc]) reads the same two cues.
    /// </summary>
    [Theory]
    [InlineData("sample_TS_cea608_h264.ts")]
    [InlineData("sample_TS_cea608_mpeg2.ts")]
    public void ReadsCea608RollUpFromVideoStream(string fileName)
    {
        var parser = new TransportStreamParser();
        parser.Parse(FilePath(fileName), null);

        var videoPid = Assert.Single(parser.ClosedCaptionSubtitlesLookup);
        Assert.True(videoPid.Value.ContainsKey(1), "CC1 track expected");
        var cc1 = videoPid.Value[1];

        // A roll-up line that is still being written grows its cue - no cue per character.
        Assert.Equal("(<i>inaudible radio chatter</i>)", cc1[0].Text);
        Assert.Equal(967, cc1[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal(3103, cc1[0].EndTime.TotalMilliseconds, 0);
        Assert.Equal("(<i>inaudible radio chatter</i>)" + System.Environment.NewLine + ">> Safety remains our number one", cc1[1].Text);
        Assert.Equal(4337, cc1[1].EndTime.TotalMilliseconds, 0);
    }

    /// <summary>
    /// H.265 prefix SEI, no PAT/PMT (codec unknown): a pop-on caption on field 1 (CC1), and one on
    /// field 2 (CC3) sent with the field 2 control codes (0x15) and preceded by an XDS packet whose
    /// payload bytes must not end up as caption text.
    /// </summary>
    [Fact]
    public void ReadsCea608FromH265SeiOnBothFields()
    {
        var frames = new List<byte[][]>
        {
            // field 1: RCL                          field 2: RCL (0x15 form), XDS start + payload (must be ignored)
            new[] { Cc(0, 0x14, 0x20),              Cc(1, 0x15, 0x20), Cc(1, 0x01, 0x03), Cc(1, 0x41, 0x42) },
            // field 1: "HI"                         field 2: XDS end + checksum, "OK"
            new[] { Cc(0, 0x48, 0x49),              Cc(1, 0x0F, 0x10), Cc(1, 0x4F, 0x4B) },
            // EOC on both fields - captions are displayed
            new[] { Cc(0, 0x14, 0x2F),              Cc(1, 0x15, 0x2F) },
            new[] { Cc(0, 0x80, 0x80) },
            // EDM on both fields - captions are erased
            new[] { Cc(0, 0x14, 0x2C),              Cc(1, 0x15, 0x2C) },
        };

        var parser = new TransportStreamParser();
        parser.Parse(new MemoryStream(BuildTransportStream(frames)), null);

        var tracks = Assert.Single(parser.ClosedCaptionSubtitlesLookup).Value;
        Assert.Equal(new[] { 1, 3 }, tracks.Keys.ToArray());

        var cc1 = Assert.Single(tracks[1]);
        Assert.Equal("HI", cc1.Text);
        Assert.Equal(2000, cc1.StartTime.TotalMilliseconds, 0);
        Assert.Equal(4000, cc1.EndTime.TotalMilliseconds, 0);

        var cc3 = Assert.Single(tracks[3]);
        Assert.Equal("OK", cc3.Text);
    }

    /// <summary>
    /// SCTE 20 captions (US cable, before ATSC A/53) in MPEG-2 user data: ffmpeg fate-suite
    /// sub/scte20.ts with the picture slices removed (headers, user data and timing kept). ffmpeg's
    /// decoder reads the same lines from the original. The recording starts in the middle of a
    /// roll-up caption, before any control code: its first partial line is kept once the first
    /// command turns out to be a roll-up (it used to be dropped).
    /// </summary>
    [Fact]
    public void ReadsScte20CaptionsFromMpeg2UserData()
    {
        var parser = new TransportStreamParser();
        parser.Parse(FilePath("sample_TS_cea608_scte20.ts"), null);

        var tracks = Assert.Single(parser.ClosedCaptionSubtitlesLookup).Value;
        var cc1 = tracks[1];
        Assert.Equal("BESIDES THE", cc1[0].Text);
        Assert.Equal("BESIDES THE" + System.Environment.NewLine + "SPENDING AND THIS, IS THAT CAR", cc1[1].Text);
        Assert.Equal("SPENDING AND THIS, IS THAT CAR" + System.Environment.NewLine + "MANUFACTURERS ARE ABOUT AS", cc1[2].Text);
        Assert.InRange(cc1[2].StartTime.TotalMilliseconds, 3900, 4100);
    }

    /// <summary>
    /// SCTE 20 field numbers are in transmission order: 1 (and 3, a repeated first field) is CEA-608
    /// field 1 when the picture is top field first, the fields swap when it is bottom field first.
    /// The caption bytes are sent least significant bit first.
    /// </summary>
    [Theory]
    [InlineData(true, 1, 0)]
    [InlineData(true, 2, 1)]
    [InlineData(true, 3, 0)]
    [InlineData(false, 1, 1)]
    [InlineData(false, 2, 0)]
    public void Scte20FieldNumberAndBitOrder(bool topFieldFirst, int field, int expectedCcType)
    {
        // user_data_type_code 3, 0x81, then cc_count (5 bits) and one 26-bit pair:
        // priority (2), field (2), line offset (5), cc_data_1 (8, reversed), cc_data_2 (8, reversed), marker (1)
        static string Reverse(int b) => new string(System.Convert.ToString(b, 2).PadLeft(8, '0').Reverse().ToArray());
        var bits = "00001" + "00" + System.Convert.ToString(field, 2).PadLeft(2, '0') + "10101" + Reverse(0x94) + Reverse(0x2C) + "1";
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '0');
        var userData = new List<byte> { 0x03, 0x81 };
        for (var i = 0; i < bits.Length; i += 8)
        {
            userData.Add(System.Convert.ToByte(bits.Substring(i, 8), 2));
        }

        var ccData = new List<Nikse.SubtitleEdit.Core.Cea608.CcData>();
        Nikse.SubtitleEdit.Core.Cea608.GetCcDataHelper.ParseCcDataFromScte20UserData(userData.ToArray(), topFieldFirst, ccData);

        var cc = Assert.Single(ccData);
        Assert.Equal(expectedCcType, cc.Type);
        Assert.Equal(0x94, cc.Data1);
        Assert.Equal(0x2C, cc.Data2);
    }

    private static byte[] Cc(int ccType, int data1, int data2) => new[] { (byte)(0xF8 | 0x04 | ccType), (byte)data1, (byte)data2 };

    // One PES packet per frame on PID 0x100, one second apart, starting at PTS 0.
    private static byte[] BuildTransportStream(List<byte[][]> frames)
    {
        var ts = new MemoryStream();
        var continuityCounter = 0;
        for (var i = 0; i < frames.Count; i++)
        {
            var sei = BuildH265SeiNal(frames[i]);
            var pes = new List<byte> { 0, 0, 1, 0xE0, 0, 0, 0x80, 0x80, 5 };
            pes.AddRange(EncodePts(90000L * i));
            pes.AddRange(new byte[] { 0, 0, 0, 1 });
            pes.AddRange(sei);
            WritePes(ts, pes.ToArray(), ref continuityCounter);
        }

        return ts.ToArray();
    }

    private static byte[] BuildH265SeiNal(byte[][] triplets)
    {
        var payload = new List<byte> { 0xB5, 0x00, 0x31, 0x47, 0x41, 0x39, 0x34, 0x03, (byte)(0x40 | triplets.Length), 0xFF };
        foreach (var triplet in triplets)
        {
            payload.AddRange(triplet);
        }

        payload.Add(0xFF);

        var nal = new List<byte> { 0x4E, 0x01, 0x04, (byte)payload.Count }; // prefix SEI, payload type 4
        nal.AddRange(payload);
        nal.Add(0x80); // rbsp trailing bits
        return nal.ToArray();
    }

    private static byte[] EncodePts(long pts)
    {
        return new[]
        {
            (byte)(0x21 | ((pts >> 29) & 0x0E)),
            (byte)(pts >> 22),
            (byte)(0x01 | ((pts >> 14) & 0xFE)),
            (byte)(pts >> 7),
            (byte)(0x01 | ((pts << 1) & 0xFE)),
        };
    }

    private static void WritePes(Stream ts, byte[] pes, ref int continuityCounter)
    {
        var offset = 0;
        while (offset < pes.Length)
        {
            var packet = new byte[188];
            packet[0] = 0x47;
            packet[1] = (byte)((offset == 0 ? 0x40 : 0) | 0x01);
            packet[2] = 0x00;
            var remaining = pes.Length - offset;
            int headerLength;
            if (remaining >= 184)
            {
                packet[3] = (byte)(0x10 | continuityCounter);
                headerLength = 4;
            }
            else
            {
                // adaptation field stuffing to fill the packet
                packet[3] = (byte)(0x30 | continuityCounter);
                var adaptationFieldLength = 183 - remaining;
                packet[4] = (byte)adaptationFieldLength;
                if (adaptationFieldLength > 0)
                {
                    packet[5] = 0x00;
                    for (var i = 6; i < 5 + adaptationFieldLength; i++)
                    {
                        packet[i] = 0xFF;
                    }
                }

                headerLength = 5 + adaptationFieldLength;
            }

            var count = 188 - headerLength;
            System.Array.Copy(pes, offset, packet, headerLength, count);
            offset += count;
            continuityCounter = (continuityCounter + 1) & 0x0F;
            ts.Write(packet, 0, packet.Length);
        }
    }
}
