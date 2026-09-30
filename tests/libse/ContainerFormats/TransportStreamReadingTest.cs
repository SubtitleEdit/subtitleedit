using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibSETests.ContainerFormats;

/// <summary>
/// Transport stream reading, with streams re-muxed from the sample files: several programs,
/// time stamps across the 33-bit wrap, missing time stamps and segments, PGS in a plain .ts.
/// </summary>
public class TransportStreamReadingTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    private const int GraphicsPid = 41; // DVB subtitles in sample_TS_with_graphics.ts
    private const int TeletextPid = 5104; // teletext in sample_TS_with_teletext.ts

    private static List<(ulong Start, ulong End)> Times(List<TransportStreamSubtitle> subtitles) =>
        subtitles.Select(p => (p.StartMilliseconds, p.EndMilliseconds)).ToList();

    private static TransportStreamParser Parse(byte[] ts)
    {
        var parser = new TransportStreamParser();
        parser.Parse(new MemoryStream(ts), null);
        return parser;
    }

    /// <summary>
    /// The DVB subtitles of sample_TS_with_graphics.ts on <paramref name="subtitlePid"/>, their PTS
    /// moved by <paramref name="shift"/>, after a video PES one second before the first of them.
    /// </summary>
    private static void WriteGraphicsProgram(TransportStreamTestWriter writer, int videoPid, int subtitlePid, long shift, bool writeVideo = true)
    {
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_graphics.ts"), GraphicsPid);
        if (writeVideo)
        {
            writer.WritePes(videoPid, TransportStreamTestWriter.MakeVideoPes(TransportStreamTestWriter.GetPts(pesList[0]) - 90000 + shift));
        }

        foreach (var pes in pesList)
        {
            writer.WritePes(subtitlePid, TransportStreamTestWriter.WithPts(pes, TransportStreamTestWriter.GetPts(pes) + shift));
        }
    }

    private static byte[] BuildGraphicsStream(long shift)
    {
        var writer = new TransportStreamTestWriter();
        WriteGraphicsProgram(writer, 0x100, 0x101, shift);
        return writer.ToArray();
    }

    /// <summary>
    /// Every program of a multi-program stream (e.g. a whole DVB-T mux) has its own clock. The
    /// subtitles used to be timed by the first video of any program, so another program's
    /// subtitles came out hours off.
    /// </summary>
    [Fact]
    public void MultiProgramStreamTimesSubtitlesByTheirOwnProgram()
    {
        var expected = Times(Parse(BuildGraphicsStream(0)).GetDvbSubtitles(0x101));
        Assert.Equal(10, expected.Count);
        Assert.InRange(expected[0].Start, 1049UL, 1051UL); // the first subtitle is 1.05 s after the video

        var writer = new TransportStreamTestWriter();
        writer.WriteProgramAssociationTable((1, 0x1000), (2, 0x1001));
        writer.WriteProgramMapTable(0x1000, 1, 0x100, (0x02, 0x100), (0x06, 0x101));
        writer.WriteProgramMapTable(0x1001, 2, 0x200, (0x02, 0x200), (0x06, 0x201));
        WriteGraphicsProgram(writer, 0x200, 0x201, 90000L * 3600 * 5); // this program's video comes first
        WriteGraphicsProgram(writer, 0x100, 0x101, 0);

        var parser = Parse(writer.ToArray());

        Assert.Equal(expected, Times(parser.GetDvbSubtitles(0x101)));
        Assert.Equal(expected, Times(parser.GetDvbSubtitles(0x201)));
    }

    /// <summary>
    /// Time code zero is where the program starts - its earliest video or audio time stamp, as
    /// ffmpeg, mpv and the waveform count - not its first video frame: audio that starts 0.3 s
    /// before the video put every subtitle 0.3 s early.
    /// </summary>
    [Theory]
    [InlineData(0x03, 0xC0, false)] // MPEG audio
    [InlineData(0x81, 0xBD, true)] // AC-3 in private stream 1, listed in the PMT
    public void AudioThatStartsFirstIsTimeCodeZero(int audioStreamType, int audioStreamId, bool ac3Payload)
    {
        var expected = Times(Parse(BuildGraphicsStream(0)).GetDvbSubtitles(0x101));
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_graphics.ts"), GraphicsPid);
        var videoPts = TransportStreamTestWriter.GetPts(pesList[0]) - 90000;

        var writer = new TransportStreamTestWriter();
        writer.WriteProgramAssociationTable((1, 0x1000));
        writer.WriteProgramMapTable(0x1000, 1, 0x100, (0x02, 0x100), (0x06, 0x101), (audioStreamType, 0x102));
        var payload = ac3Payload ? new byte[] { 0x0B, 0x77, 0, 0, 0, 0 } : new byte[] { 0xFF, 0xFD, 0, 0, 0, 0 };
        writer.WritePes(0x102, TransportStreamTestWriter.MakePes(audioStreamId, videoPts - 27000, payload));
        WriteGraphicsProgram(writer, 0x100, 0x101, 0);

        var times = Times(Parse(writer.ToArray()).GetDvbSubtitles(0x101));

        Assert.Equal(expected.Select(p => (p.Start + 300, p.End + 300)), times);
    }

    /// <summary>
    /// Without a PMT, AC-3 in private stream 1 is told from the subtitles and teletext that share
    /// that stream id by its sync word - and the DVB subtitles themselves never count as audio.
    /// </summary>
    [Fact]
    public void Ac3WithoutProgramMapTableIsAudio()
    {
        var expected = Times(Parse(BuildGraphicsStream(0)).GetDvbSubtitles(0x101));
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_graphics.ts"), GraphicsPid);
        var videoPts = TransportStreamTestWriter.GetPts(pesList[0]) - 90000;

        var writer = new TransportStreamTestWriter();
        writer.WritePes(0x102, TransportStreamTestWriter.MakePes(0xBD, videoPts - 45000, new byte[] { 0x0B, 0x77, 0, 0, 0, 0 }));
        WriteGraphicsProgram(writer, 0x100, 0x101, 0);

        Assert.Equal(expected.Select(p => (p.Start + 500, p.End + 500)), Times(Parse(writer.ToArray()).GetDvbSubtitles(0x101)));
    }

    /// <summary>
    /// VC-1 video (Blu-ray) is sent with stream id 0xFD, outside the MPEG video range: its start
    /// was never found, and the subtitles kept their raw time stamps - 01:15:29 for a recording
    /// whose clock started there.
    /// </summary>
    [Fact]
    public void Vc1VideoOnAnExtendedStreamIdIsTimeCodeZero()
    {
        var expected = Times(Parse(BuildGraphicsStream(0)).GetDvbSubtitles(0x101));
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_graphics.ts"), GraphicsPid);
        var shift = 90000L * 4529;

        var writer = new TransportStreamTestWriter();
        writer.WriteProgramAssociationTable((1, 0x1000));
        writer.WriteProgramMapTable(0x1000, 1, 0x100, (0xEA, 0x100), (0x06, 0x101));
        writer.WritePes(0x100, TransportStreamTestWriter.MakePes(0xFD, TransportStreamTestWriter.GetPts(pesList[0]) - 90000 + shift, new byte[] { 0, 0, 1, 0x0F, 0, 0 }));
        WriteGraphicsProgram(writer, 0x100, 0x101, shift, writeVideo: false);

        Assert.Equal(expected, Times(Parse(writer.ToArray()).GetDvbSubtitles(0x101)));
    }

    /// <summary>
    /// PTS are 33-bit and wrap every ~26.5 hours of stream clock. Subtitles after the wrap used to
    /// be moved to a second after the previous one, losing the real gap.
    /// </summary>
    [Fact]
    public void SubtitlesAcrossTheTimestampWrapKeepTheirTimes()
    {
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_graphics.ts"), GraphicsPid);
        var firstPts = TransportStreamTestWriter.GetPts(pesList[0]);
        var expected = Times(Parse(BuildGraphicsStream(0)).GetDvbSubtitles(0x101));

        // the wrap falls while the 4th subtitle is shown - it ends, and the rest start, after it
        var wrapAt = (TransportStreamTestWriter.GetPts(pesList[7]) + TransportStreamTestWriter.GetPts(pesList[8])) / 2;
        var wrapped = Parse(BuildGraphicsStream(TransportStreamTestWriter.TimestampWrap - wrapAt));

        Assert.True(firstPts < wrapAt);
        Assert.Equal(expected, Times(wrapped.GetDvbSubtitles(0x101)));
    }

    /// <summary>
    /// Older encoders send no end of display set segment. The next display set - here the empty
    /// page that clears the screen - must still end the subtitle; they all used to run for the
    /// maximum duration and overlap.
    /// </summary>
    [Fact]
    public void DisplaySetsWithoutEndSegmentEndAtTheNextPageComposition()
    {
        var expected = Times(Parse(BuildGraphicsStream(0)).GetDvbSubtitles(0x101));

        var writer = new TransportStreamTestWriter();
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_graphics.ts"), GraphicsPid);
        writer.WritePes(0x100, TransportStreamTestWriter.MakeVideoPes(TransportStreamTestWriter.GetPts(pesList[0]) - 90000));
        foreach (var pes in pesList)
        {
            writer.WritePes(0x101, RemoveDvbSegments(pes, 0x80));
        }

        Assert.Equal(expected, Times(Parse(writer.ToArray()).GetDvbSubtitles(0x101)));
    }

    /// <summary>
    /// Removes the DVB subtitle segments of <paramref name="segmentType"/> from a PES packet.
    /// </summary>
    private static byte[] RemoveDvbSegments(byte[] pes, int segmentType)
    {
        var dataStart = 9 + pes[8];
        var result = pes.Take(dataStart + 2).ToList(); // data_identifier, subtitle_stream_id
        var i = dataStart + 2;
        while (i + 6 <= pes.Length && pes[i] == 0x0F)
        {
            var length = 6 + ((pes[i + 4] << 8) | pes[i + 5]);
            if (pes[i + 1] != segmentType)
            {
                result.AddRange(pes.Skip(i).Take(length));
            }

            i += length;
        }

        result.AddRange(pes.Skip(i)); // end_of_PES_data_field_marker
        var pesLength = result.Count - 6;
        result[4] = (byte)(pesLength >> 8);
        result[5] = (byte)pesLength;
        return result.ToArray();
    }

    /// <summary>
    /// Blu-ray PGS remuxed into a plain 188-byte transport stream (tsMuxeR, ffmpeg) used to be
    /// ignored - only .m2ts was read as PGS. The video starts 80 ms after the first subtitle,
    /// which must show at zero rather than wrap around to a huge unsigned time.
    /// </summary>
    [Fact]
    public void ReadsPgsFromPlainTransportStream()
    {
        var supFileName = FilePath("sample_BDSUP_multi_image.sup");
        var expected = BluRaySupParser.ParseBluRaySup(supFileName, new StringBuilder());
        Assert.True(expected.Count > 1);

        // "PG", PTS (32 bits), DTS (32 bits), segment type, segment size - one PES per segment
        var segments = new List<(long Pts, byte[] Segment)>();
        var data = File.ReadAllBytes(supFileName);
        for (var i = 0; i + 13 <= data.Length;)
        {
            var pts = ((long)data[i + 2] << 24) | ((long)data[i + 3] << 16) | ((long)data[i + 4] << 8) | data[i + 5];
            var size = (data[i + 11] << 8) | data[i + 12];
            segments.Add((pts, data.Skip(i + 10).Take(3 + size).ToArray()));
            i += 13 + size;
        }

        var firstPts = expected[0].StartTime;
        var writer = new TransportStreamTestWriter();
        writer.WritePes(0x1011, TransportStreamTestWriter.MakeVideoPes(firstPts + 80 * 90));
        foreach (var segment in segments)
        {
            writer.WritePes(0x1200, TransportStreamTestWriter.MakePes(0xBD, segment.Pts, segment.Segment));
        }

        var parser = Parse(writer.ToArray());

        var pid = Assert.Single(parser.SubtitlePacketIds);
        var subtitles = parser.GetDvbSubtitles(pid);
        Assert.All(subtitles, p => Assert.True(p.IsBluRaySup));
        Assert.Equal(expected.Count, subtitles.Count);
        Assert.Equal(0UL, subtitles[0].StartMilliseconds);
        for (var i = 1; i < expected.Count; i++)
        {
            var expectedMs = (expected[i].StartTime - firstPts) / 90.0 - 80;
            Assert.InRange(subtitles[i].StartMilliseconds, (ulong)(expectedMs - 1), (ulong)(expectedMs + 1));
        }
    }

    /// <summary>
    /// Some recorders (Topfield) strip the PTS from teletext. The program clock at the time the
    /// packet arrives is the next best time stamp - every caption used to land at 00:00:00.040.
    /// </summary>
    [Fact]
    public void TeletextWithoutPtsIsTimedByTheProgramClock()
    {
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_teletext.ts"), TeletextPid);

        byte[] Build(bool withPts)
        {
            var writer = new TransportStreamTestWriter();
            writer.WritePes(0x100, TransportStreamTestWriter.MakeVideoPes(TransportStreamTestWriter.GetPts(pesList[0])));
            foreach (var pes in pesList)
            {
                writer.WriteProgramClockReference(0x100, TransportStreamTestWriter.GetPts(pes));
                writer.WritePes(TeletextPid, withPts ? pes : TransportStreamTestWriter.WithPts(pes, null));
            }

            return writer.ToArray();
        }

        var expected = Parse(Build(withPts: true)).TeletextSubtitlesLookup[TeletextPid];
        var actual = Parse(Build(withPts: false)).TeletextSubtitlesLookup[TeletextPid];

        Assert.Equal(expected.Keys, actual.Keys);
        Assert.Equal(3, expected.Values.Sum(p => p.Count));
        foreach (var page in expected.Keys)
        {
            Assert.All(expected[page], p => Assert.True(p.StartTime.TotalMilliseconds > 1000 && p.EndTime.TotalMilliseconds < 10000));
            Assert.Equal(expected[page].Select(p => p.Text), actual[page].Select(p => p.Text));
            for (var i = 0; i < expected[page].Count; i++)
            {
                // the PTS based start is rounded to whole milliseconds, the program clock's truncated
                Assert.InRange(actual[page][i].StartTime.TotalMilliseconds, expected[page][i].StartTime.TotalMilliseconds - 1, expected[page][i].StartTime.TotalMilliseconds + 1);
                Assert.InRange(actual[page][i].EndTime.TotalMilliseconds, expected[page][i].EndTime.TotalMilliseconds - 1, expected[page][i].EndTime.TotalMilliseconds + 1);
            }
        }
    }

    /// <summary>
    /// Live subtitling paints a line word by word: each "normal case" update (page_state 0) sends
    /// only the new word, painted into a region that keeps what was painted before - and no CLUT.
    /// The update's image must show the whole page in the right colours, not the new word alone.
    /// </summary>
    [Fact]
    public void NormalCasePageUpdateShowsThePreviouslyPaintedObjects()
    {
        var pesList = TransportStreamTestWriter.ReadPesPackets(FilePath("sample_TS_with_graphics.ts"), GraphicsPid);
        var source = pesList.First(p => GetSegments(p).Any(s => s[1] == 0x13)); // first display set with an object
        var clut = GetSegments(source).First(s => s[1] == 0x12);
        var objectData = GetSegments(source).First(s => s[1] == 0x13);
        var basePts = TransportStreamTestWriter.GetPts(source);
        var clutId = clut[6];

        // acquisition point: region 0 filled, object 1 at the top
        var first = DvbPes(basePts,
            PageComposition(1, 0),
            RegionComposition(0, clutId, fill: true, (1, 0)),
            clut,
            WithObjectId(objectData, 1));

        // normal case: object 2 painted half way down the same region - no fill, no CLUT
        var second = DvbPes(basePts + 90000,
            PageComposition(0, 0),
            RegionComposition(0, clutId, fill: false, (2, Half)),
            WithObjectId(objectData, 2));

        var writer = new TransportStreamTestWriter();
        writer.WritePes(0x100, TransportStreamTestWriter.MakeVideoPes(basePts));
        writer.WritePes(0x101, first);
        writer.WritePes(0x101, second);

        var subtitles = Parse(writer.ToArray()).GetDvbSubtitles(0x101);
        Assert.Equal(2, subtitles.Count);

        using var firstImage = subtitles[0].GetBitmap();
        using var secondImage = subtitles[1].GetBitmap();
        var firstWord = CountOpaque(firstImage, 0, Half);
        Assert.True(firstWord > 0);
        Assert.Equal(0, CountOpaque(firstImage, Half, firstImage.Height));

        // the earlier word is still there, the new one below it, both in the same colours
        Assert.Equal(firstWord, CountOpaque(secondImage, 0, Half));
        Assert.Equal(firstWord, CountOpaque(secondImage, Half, secondImage.Height));
        for (var y = 0; y < Half; y++)
        {
            for (var x = 0; x < firstImage.Width; x++)
            {
                Assert.Equal(firstImage.GetPixel(x, y), secondImage.GetPixel(x, y + Half));
            }
        }
    }

    private const int Half = 288;

    /// <summary>
    /// The PCR is a 33-bit base, 6 reserved bits and a 9-bit extension in six bytes - it was read
    /// as four bytes, and the private data/extension flags from the wrong bits.
    /// </summary>
    [Fact]
    public void AdaptationFieldReadsProgramClockReferenceAndFlags()
    {
        var pcrBase = 0x1_2345_6789UL;
        var pcrExtension = 0x123;
        var packet = new byte[188];
        packet[0] = 0x47;
        packet[3] = 0x20; // adaptation field only
        packet[4] = 183;
        packet[5] = 0x10 | 0x02; // PCR, transport private data
        packet[6] = (byte)(pcrBase >> 25);
        packet[7] = (byte)(pcrBase >> 17);
        packet[8] = (byte)(pcrBase >> 9);
        packet[9] = (byte)(pcrBase >> 1);
        packet[10] = (byte)((int)((pcrBase & 1) << 7) | 0x7E | (pcrExtension >> 8));
        packet[11] = (byte)(pcrExtension & 0xFF);
        packet[12] = 2; // private data length
        packet[13] = 0xAB;
        packet[14] = 0xCD;

        var adaptationField = new AdaptationField(packet);

        Assert.True(adaptationField.PcrFlag);
        Assert.Equal(pcrBase, adaptationField.ProgramClockReferenceBase);
        Assert.Equal(pcrExtension, adaptationField.ProgramClockReferenceExtension);
        Assert.False(adaptationField.SplicingPointFlag);
        Assert.True(adaptationField.TransportPrivateDataFlag);
        Assert.False(adaptationField.AdaptationFieldExtensionFlag);
        Assert.Equal(new byte[] { 0xAB, 0xCD }, adaptationField.TransportPrivateData);
        Assert.True(Packet.TryPeekProgramClockReference(packet, out var peeked));
        Assert.Equal(pcrBase, peeked);
    }

    /// <summary>
    /// The pointer_field counts the bytes between itself and the section - one more than it
    /// used to skip when it was not zero.
    /// </summary>
    [Fact]
    public void ProgramMapTableWithPointerField()
    {
        var payload = new List<byte> { 3, 0xFF, 0xFF, 0xFF }; // pointer_field 3 + 3 bytes before the section
        payload.AddRange(new byte[]
        {
            0x02, 0xB0, 18, 0x00, 0x01, 0xC1, 0, 0, 0xE1, 0x00, 0xF0, 0,
            0x06, 0xE1, 0x01, 0xF0, 0,
            0, 0, 0, 0, // CRC (not checked)
        });

        var programMapTable = new ProgramMapTable(payload.ToArray(), 0);

        Assert.Equal(1, programMapTable.ProgramNumber);
        Assert.Equal(0x100, programMapTable.PcrId);
        var stream = Assert.Single(programMapTable.Streams);
        Assert.Equal(0x101, stream.ElementaryPid);
    }

    private static int CountOpaque(SkiaSharp.SKBitmap bitmap, int fromY, int toY)
    {
        var count = 0;
        for (var y = fromY; y < toY; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha > 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// The DVB subtitle segments of a PES packet, each with its 6-byte header.
    /// </summary>
    private static List<byte[]> GetSegments(byte[] pes)
    {
        var segments = new List<byte[]>();
        var i = 9 + pes[8] + 2;
        while (i + 6 <= pes.Length && pes[i] == 0x0F)
        {
            var length = 6 + ((pes[i + 4] << 8) | pes[i + 5]);
            segments.Add(pes.Skip(i).Take(length).ToArray());
            i += length;
        }

        return segments;
    }

    private static byte[] Segment(int type, params byte[] data)
    {
        return new byte[] { 0x0F, (byte)type, 0, 1, (byte)(data.Length >> 8), (byte)data.Length }.Concat(data).ToArray();
    }

    private static byte[] PageComposition(int pageState, int regionId)
    {
        // time-out 30 s, version 0, page state; the region at (0, 0)
        return Segment(0x10, 30, (byte)(pageState << 2), (byte)regionId, 0, 0, 0, 0, 0);
    }

    private static byte[] RegionComposition(int regionId, int clutId, bool fill, params (int ObjectId, int Y)[] objects)
    {
        var data = new List<byte>
        {
            (byte)regionId, (byte)(fill ? 0x08 : 0), 0x02, 0xD0, 0x02, 0x40, // 720x576
            0x48, (byte)clutId, 0, 0, // 4-bit level and depth, CLUT, pixel codes 0 (transparent)
        };
        foreach (var o in objects)
        {
            data.Add((byte)(o.ObjectId >> 8));
            data.Add((byte)o.ObjectId);
            data.Add(0); // basic bitmap object, x = 0
            data.Add(0);
            data.Add((byte)((o.Y >> 8) & 0x0F));
            data.Add((byte)o.Y);
        }

        return Segment(0x11, data.ToArray());
    }

    private static byte[] WithObjectId(byte[] objectDataSegment, int objectId)
    {
        var copy = (byte[])objectDataSegment.Clone();
        copy[2] = 0;
        copy[3] = 1; // page 1
        copy[6] = (byte)(objectId >> 8);
        copy[7] = (byte)objectId;
        return copy;
    }

    private static byte[] DvbPes(long pts, params byte[][] segments)
    {
        var payload = new List<byte> { 0x20, 0x00 };
        foreach (var segment in segments)
        {
            var copy = (byte[])segment.Clone();
            copy[2] = 0;
            copy[3] = 1; // page 1
            payload.AddRange(copy);
        }

        payload.AddRange(Segment(0x80));
        payload.Add(0xFF);
        return TransportStreamTestWriter.MakePes(0xBD, pts, payload.ToArray());
    }
}
