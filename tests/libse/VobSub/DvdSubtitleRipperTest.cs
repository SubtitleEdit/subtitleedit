using Nikse.SubtitleEdit.Core.VobSub;
using System.Text;

namespace LibSETests.VobSub;

public class DvdSubtitleRipperTest : IDisposable
{
    private const int SectorSize = 2048;
    private readonly string _folder;

    public DvdSubtitleRipperTest()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-dvd-rip-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // ignore
        }
    }

    [Fact]
    public void Rip_ProgramChain_PlacesSubtitlesOnCellTimeline()
    {
        // cell 1 (vob 1/cell 1): PTS starts at 1000, subtitle 0.5 s in
        // cell 2 (vob 1/cell 2): PTS restarts at 500, subtitle 0.2 s in - plus a VOBU of another
        // angle (vob 1/cell 9) interleaved inside the cell, whose subtitle must be skipped.
        // The cells are split over two VOB files.
        var vob1 = WriteVob("VTS_01_1.VOB",
            MakeNavPack(1, 1, 1000, 30000),
            MakeSubtitlePack(0x20, 1000 + 45000),
            MakeVideoPack(),
            MakeNavPack(1, 2, 500, 20000));
        var vob2 = WriteVob("VTS_01_2.VOB",
            MakeSubtitlePack(0x21, 500 + 18000),
            MakeNavPack(1, 9, 700, 9000),
            MakeSubtitlePack(0x20, 99999));

        var programChain = new IfoParser.ProgramChain();
        programChain.Cells.Add(new IfoParser.Cell { VobId = 1, CellId = 1, Start = TimeSpan.Zero, Duration = TimeSpan.FromSeconds(10), FirstSector = 0, LastSector = 2 });
        programChain.Cells.Add(new IfoParser.Cell { VobId = 1, CellId = 2, Start = TimeSpan.FromSeconds(10), Duration = TimeSpan.FromSeconds(5), FirstSector = 3, LastSector = 6 });

        var progress = new List<(long Done, long Total)>();
        var packs = DvdSubtitleRipper.Rip(new[] { vob1, vob2 }, programChain, (done, total) => progress.Add((done, total)));

        Assert.Equal(2, packs.Count);
        Assert.Equal(0x20, packs[0].PacketizedElementaryStream.SubPictureStreamId);
        Assert.Equal(45000UL, packs[0].PacketizedElementaryStream.PresentationTimestamp);
        Assert.Equal(0x21, packs[1].PacketizedElementaryStream.SubPictureStreamId);
        Assert.Equal(900000UL + 18000UL, packs[1].PacketizedElementaryStream.PresentationTimestamp);
        Assert.Equal((7L, 7L), progress[^1]);
    }

    [Fact]
    public void Rip_ProgramChain_SkipsCellsInMissingVobFiles()
    {
        var vob1 = WriteVob("VTS_01_1.VOB",
            MakeNavPack(1, 1, 0, 30000),
            MakeSubtitlePack(0x20, 9000));

        var programChain = new IfoParser.ProgramChain();
        programChain.Cells.Add(new IfoParser.Cell { VobId = 1, CellId = 1, FirstSector = 0, LastSector = 1 });
        programChain.Cells.Add(new IfoParser.Cell { VobId = 1, CellId = 2, Start = TimeSpan.FromSeconds(1), FirstSector = 5000, LastSector = 6000 });

        var packs = DvdSubtitleRipper.Rip(new[] { vob1 }, programChain);

        Assert.Single(packs);
        Assert.Equal(9000UL, packs[0].PacketizedElementaryStream.PresentationTimestamp);
    }

    [Fact]
    public void Rip_ProgramChain_SkipsEmptyVobFile()
    {
        // an empty VOB in the middle shares its first sector with the next VOB - reading must not loop forever
        var vob1 = WriteVob("VTS_01_1.VOB",
            MakeNavPack(1, 1, 0, 30000),
            MakeSubtitlePack(0x20, 9000));
        var vob2 = WriteVob("VTS_01_2.VOB");
        var vob3 = WriteVob("VTS_01_3.VOB",
            MakeNavPack(1, 2, 0, 30000),
            MakeSubtitlePack(0x21, 4500));

        var programChain = new IfoParser.ProgramChain();
        programChain.Cells.Add(new IfoParser.Cell { VobId = 1, CellId = 1, FirstSector = 0, LastSector = 1 });
        programChain.Cells.Add(new IfoParser.Cell { VobId = 1, CellId = 2, Start = TimeSpan.FromSeconds(1), FirstSector = 2, LastSector = 3 });

        var task = Task.Run(() => DvdSubtitleRipper.Rip(new[] { vob1, vob2, vob3 }, programChain));
        Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "Rip did not finish");

        var packs = task.Result;
        Assert.Equal(2, packs.Count);
        Assert.Equal(9000UL, packs[0].PacketizedElementaryStream.PresentationTimestamp);
        Assert.Equal(0x21, packs[1].PacketizedElementaryStream.SubPictureStreamId);
        Assert.Equal(90000UL + 4500UL, packs[1].PacketizedElementaryStream.PresentationTimestamp);
    }

    [Fact]
    public void Rip_WithoutIfo_StitchesPtsRestartsFromNavPacks()
    {
        var vob = WriteVob("VTS_01_1.VOB",
            MakeNavPack(1, 1, 1000, 30000),
            MakeSubtitlePack(0x20, 1000 + 45000),
            MakeNavPack(1, 2, 500, 20000), // PTS restart: continues where the previous VOBU ended
            MakeSubtitlePack(0x20, 500 + 18000));

        var packs = DvdSubtitleRipper.Rip(new[] { vob });

        Assert.Equal(2, packs.Count);
        Assert.Equal(45000UL, packs[0].PacketizedElementaryStream.PresentationTimestamp);
        Assert.Equal(29000UL + 18000UL, packs[1].PacketizedElementaryStream.PresentationTimestamp);
    }

    [Fact]
    public void Rip_KeepsLargePtsIntact()
    {
        // all 33 bits, so the PTS rewrite must round trip every bit and marker
        const long pts = 0x1_2345_6789;
        var vob = WriteVob("VTS_01_1.VOB",
            MakeNavPack(1, 1, 0, 30000),
            MakeSubtitlePack(0x3F, pts));

        var programChain = new IfoParser.ProgramChain();
        programChain.Cells.Add(new IfoParser.Cell { VobId = 1, CellId = 1, Start = TimeSpan.FromMilliseconds(1), FirstSector = 0, LastSector = 1 });

        var packs = DvdSubtitleRipper.Rip(new[] { vob }, programChain);

        Assert.Equal((ulong)pts + 90, packs[0].PacketizedElementaryStream.PresentationTimestamp);
        Assert.Equal(0x3F, packs[0].PacketizedElementaryStream.SubPictureStreamId);
    }

    [Fact]
    public void CountEncrypted_CountsCssScrambledPacks()
    {
        var encrypted = MakeSubtitlePack(0x20, 9000);
        encrypted[14 + 6] |= 0b0011_0000; // PES scrambling control
        var vob = WriteVob("VTS_01_1.VOB",
            MakeNavPack(1, 1, 0, 30000),
            MakeSubtitlePack(0x20, 4500),
            encrypted);

        var packs = DvdSubtitleRipper.Rip(new[] { vob });

        Assert.Equal(2, packs.Count);
        Assert.Equal(1, DvdSubtitleRipper.CountEncrypted(packs));
    }

    private string WriteVob(string name, params byte[][] sectors)
    {
        var fileName = Path.Combine(_folder, name);
        File.WriteAllBytes(fileName, sectors.SelectMany(p => p).ToArray());
        return fileName;
    }

    private static byte[] MakePackHeader()
    {
        var buffer = new byte[SectorSize];
        buffer[2] = 1;
        buffer[3] = 0xBA;
        buffer[4] = 0x44;
        buffer[13] = 0xF8; // no stuffing
        return buffer;
    }

    private static byte[] MakeNavPack(int vobId, int cellId, uint vobuStartPts, uint vobuEndPts)
    {
        var buffer = MakePackHeader();
        buffer[14 + 2] = 1;
        buffer[14 + 3] = 0xBB; // system header
        buffer[0x26 + 2] = 1;
        buffer[0x26 + 3] = 0xBF; // PCI
        WriteUInt32(buffer, 0x39, vobuStartPts);
        WriteUInt32(buffer, 0x3D, vobuEndPts);
        buffer[0x400 + 2] = 1;
        buffer[0x400 + 3] = 0xBF; // DSI
        buffer[0x41F] = (byte)(vobId >> 8);
        buffer[0x420] = (byte)vobId;
        buffer[0x422] = (byte)cellId;
        return buffer;
    }

    private static byte[] MakeVideoPack()
    {
        var buffer = MakePackHeader();
        buffer[14 + 2] = 1;
        buffer[14 + 3] = 0xE0;
        return buffer;
    }

    private static byte[] MakeSubtitlePack(int streamId, long pts)
    {
        var buffer = MakePackHeader();
        const int pes = 14;
        const int pesLength = 100;
        buffer[pes + 2] = 1;
        buffer[pes + 3] = 0xBD;
        buffer[pes + 4] = 0;
        buffer[pes + 5] = pesLength;
        buffer[pes + 6] = 0x81;
        buffer[pes + 7] = 0x80; // PTS only
        buffer[pes + 8] = 5;
        buffer[pes + 9] = (byte)(0x21 | (int)((pts >> 29) & 0x0E));
        buffer[pes + 10] = (byte)(pts >> 22);
        buffer[pes + 11] = (byte)(((pts >> 14) & 0xFE) | 1);
        buffer[pes + 12] = (byte)(pts >> 7);
        buffer[pes + 13] = (byte)(((pts << 1) & 0xFE) | 1);
        buffer[pes + 14] = (byte)streamId;
        Encoding.ASCII.GetBytes("payload").CopyTo(buffer, pes + 15);
        return buffer;
    }

    private static void WriteUInt32(byte[] buffer, int index, uint value)
    {
        buffer[index] = (byte)(value >> 24);
        buffer[index + 1] = (byte)(value >> 16);
        buffer[index + 2] = (byte)(value >> 8);
        buffer[index + 3] = (byte)value;
    }
}
