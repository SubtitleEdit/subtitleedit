using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibSETests.ContainerFormats;

/// <summary>
/// PSP UMD Video subtitles: png records in private stream 1 sub-streams 0x80+, split over as many
/// PES packets as they need. Streams built like the user's 00048.MPS (C:\Data\Issues\UMD).
/// </summary>
public class UmdVideoSubtitleReaderTest
{
    private const long Pts = 270_180; // 3.002 s

    private static byte[] MakePng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.White);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>Sub-stream id, 00, length, "0088", duration, 00 10, x, y, 00 00, png.</summary>
    private static byte[] MakeRecord(int subStreamId, uint durationTicks, int x, int y, byte[] png)
    {
        var header = new List<byte>();
        header.AddRange(Encoding.ASCII.GetBytes("0088"));
        header.AddRange(BigEndian(durationTicks));
        header.AddRange(new byte[] { 0x00, 0x10, (byte)(x >> 8), (byte)x, (byte)(y >> 8), (byte)y, 0, 0 });
        var record = new List<byte> { (byte)subStreamId, 0 };
        record.AddRange(BigEndian((uint)(header.Count + png.Length)));
        record.AddRange(header);
        record.AddRange(png);
        return record.ToArray();
    }

    private static byte[] BigEndian(uint value) => new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

    private static readonly byte[] PackHeader = { 0, 0, 1, 0xBA, 0x44, 0, 4, 0, 4, 1, 1, 0x89, 0xC3, 0xF8 };

    private static byte[] Pes(bool withPts, IEnumerable<byte> payload)
    {
        var pes = new List<byte> { 0, 0, 1, 0xBD, 0, 0, 0x81 };
        if (withPts)
        {
            pes.AddRange(new byte[] { 0x80, 5 });
            pes.AddRange(TransportStreamTestWriter.EncodePts(Pts));
        }
        else
        {
            pes.AddRange(new byte[] { 0x00, 0 });
        }

        pes.AddRange(payload);
        var length = pes.Count - 6;
        pes[4] = (byte)(length >> 8);
        pes[5] = (byte)length;
        return pes.ToArray();
    }

    /// <summary>
    /// A program stream with a DVD style AC-3 packet on sub-stream 0x80 (not a subtitle) and one
    /// subtitle record split over two packs - the continuation PES repeats sub-stream id and 00.
    /// </summary>
    private static byte[] BuildProgramStream(byte[] record)
    {
        var stream = new List<byte>();
        stream.AddRange(PackHeader);
        stream.AddRange(Pes(true, new byte[] { 0x80, 1, 0, 1, 0x0B, 0x77, 0, 0 }));
        var split = record.Length / 2;
        stream.AddRange(PackHeader);
        stream.AddRange(Pes(true, record.Take(split)));
        stream.AddRange(PackHeader);
        stream.AddRange(Pes(false, new byte[] { record[0], 0 }.Concat(record.Skip(split))));
        stream.AddRange(new byte[] { 0, 0, 1, 0xB9 });
        return stream.ToArray();
    }

    private static SortedDictionary<int, List<UmdVideoSubtitle>> ReadFile(byte[] content, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), "se_umd_" + System.Guid.NewGuid().ToString("N") + extension);
        try
        {
            File.WriteAllBytes(path, content);
            return UmdVideoSubtitleReader.Read(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void AssertPicture(UmdVideoSubtitle picture)
    {
        Assert.Equal(3002, picture.StartTime.TotalMilliseconds, 1);
        Assert.Equal(3002 + 5305.3, picture.EndTime.TotalMilliseconds, 1); // 477477 ticks
        Assert.Equal(186, picture.X);
        Assert.Equal(369, picture.Y);
        using var bitmap = picture.GetBitmap();
        Assert.Equal(347, bitmap.Width);
        Assert.Equal(58, bitmap.Height);
    }

    [Fact]
    public void ReadsSubtitleRecordsFromTheProgramStream()
    {
        var record = MakeRecord(0x81, 477_477, 186, 369, MakePng(347, 58));

        var tracks = ReadFile(BuildProgramStream(record), ".MPS");

        var track = Assert.Single(tracks); // the AC-3 packet on 0x80 is not a subtitle
        Assert.Equal(0x81, track.Key);
        AssertPicture(Assert.Single(track.Value));
    }

    [Fact]
    public void ReadsAPsmfMovieAfterItsHeader()
    {
        var header = new byte[2048];
        Encoding.ASCII.GetBytes("PSMF0015").CopyTo(header, 0);
        BigEndian(2048).CopyTo(header, 8);
        var record = MakeRecord(0x81, 477_477, 186, 369, MakePng(347, 58));

        var tracks = ReadFile(header.Concat(BuildProgramStream(record)).ToArray(), ".pmf");

        AssertPicture(Assert.Single(Assert.Single(tracks).Value));
    }

    /// <summary>
    /// A ".subs" dump is each record after its PES header data - and its writer leaves 10 bytes
    /// of the continuation PES header inside records longer than a pack, which broke the png.
    /// </summary>
    [Fact]
    public void ReadsASubsDumpAndRemovesContinuationHeaderRemnants()
    {
        var record = MakeRecord(0x81, 477_477, 186, 369, MakePng(347, 58));
        var pesHeaderData = TransportStreamTestWriter.EncodePts(Pts).Concat(new byte[] { 0x1E, 0x60, 0x80 });
        var stray = new byte[] { 0x00, 0x01, 0xBD, 0x00, 0x59, 0x80, 0x00, 0x00, 0x81, 0x00 };
        var cut = 30 + (record.Length - 30) / 2; // inside the png
        var dump = pesHeaderData.Concat(record.Take(cut)).Concat(stray).Concat(record.Skip(cut))
            .Concat(pesHeaderData).Concat(record) // a second, intact record
            .ToArray();

        var tracks = ReadFile(dump, ".subs");

        var pictures = Assert.Single(tracks).Value;
        Assert.Equal(2, pictures.Count);
        AssertPicture(pictures[0]);
        AssertPicture(pictures[1]);
    }

    [Fact]
    public void ARecordLengthNearIntMaxValueIsIgnored()
    {
        // record start + length overflowed int: a .subs dump threw, a program stream allocated ~2 GB
        var dump = new byte[64];
        TransportStreamTestWriter.EncodePts(Pts).CopyTo(dump, 0);
        dump[8] = 0x81;
        BigEndian(0x7FFFFFF8).CopyTo(dump, 10);
        Assert.Empty(ReadFile(dump, ".subs"));

        var record = new byte[64];
        record[0] = 0x81;
        BigEndian(0x7FFFFFFE).CopyTo(record, 2);
        Assert.Empty(ReadFile(BuildProgramStream(record), ".mps"));
    }

    [Fact]
    public void AnOrdinaryFileHasNoUmdSubtitles()
    {
        Assert.Empty(ReadFile(Encoding.ASCII.GetBytes("1\r\n00:00:01,000 --> 00:00:02,000\r\nHi\r\n"), ".subs"));
        Assert.Empty(ReadFile(BuildProgramStream(new byte[] { 0x81, 0, 0, 0, 0, 4, 1, 2, 3, 4 }), ".mps"));
    }
}
