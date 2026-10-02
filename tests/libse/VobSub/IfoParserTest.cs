using Nikse.SubtitleEdit.Core.VobSub;
using System.Text;

namespace LibSETests.VobSub;

public class IfoParserTest
{
    private const int SectorSize = 2048;
    private const int PgciSector = 1;
    private const int CellAddressSector = 2;

    [Fact]
    public void ParsesVideoAudioAndSubtitleAttributes()
    {
        var ifo = new IfoParser(BuildVtsIfo(wideScreen: true));

        Assert.Null(ifo.ErrorMessage);
        Assert.Equal(IfoParser.IfoType.VideoTitleSet, ifo.Type);
        Assert.True(ifo.IsPal);
        Assert.True(ifo.Video.IsWideScreen);
        Assert.Equal(720, ifo.Video.Width);
        Assert.Equal(576, ifo.Video.Height);

        Assert.Single(ifo.AudioStreams);
        Assert.Equal("en", ifo.AudioStreams[0].LanguageCode);
        Assert.Equal("AC3", ifo.AudioStreams[0].CodingMode);
        Assert.Equal(6, ifo.AudioStreams[0].Channels);

        Assert.Equal(2, ifo.SubtitleStreams.Count);
        Assert.Equal("en", ifo.SubtitleStreams[0].LanguageCode);
        Assert.Equal("da", ifo.SubtitleStreams[1].LanguageCode);
        Assert.True(ifo.SubtitleStreams[1].IsForced);
    }

    [Fact]
    public void WideScreen_MapsWideAndLetterboxStreamIds()
    {
        var ifo = new IfoParser(BuildVtsIfo(wideScreen: true));

        // en: wide 0x20, letterbox 0x21 - da: wide 0x22, letterbox 0x23
        Assert.Equal(0x20, ifo.SubtitleStreams[0].StreamIds[IfoParser.SubtitleVariant.Wide]);
        Assert.Equal(0x21, ifo.SubtitleStreams[0].StreamIds[IfoParser.SubtitleVariant.Letterbox]);
        Assert.Equal(0x22, ifo.SubtitleStreams[1].StreamIds[IfoParser.SubtitleVariant.Wide]);

        var languages = ifo.GetLanguages();
        Assert.Equal(4, languages.Count);
        Assert.EndsWith("(0x20)", languages[0]);
        Assert.EndsWith("(0x22)", languages[1]);
        Assert.Contains("forced", languages[1]);
        Assert.Contains("letterbox", languages[2]);
        Assert.Equal("da", ifo.GetLanguageCode(0x23));
        Assert.Null(ifo.GetLanguageCode(0x30));
    }

    [Fact]
    public void FourThree_UsesFirstControlByte()
    {
        var ifo = new IfoParser(BuildVtsIfo(wideScreen: false));

        Assert.Equal(0x20, ifo.SubtitleStreams[0].StreamIds[IfoParser.SubtitleVariant.Normal]);
        Assert.Equal(0x21, ifo.SubtitleStreams[1].StreamIds[IfoParser.SubtitleVariant.Normal]);
        Assert.Single(ifo.SubtitleStreams[0].StreamIds);
    }

    [Fact]
    public void ParsesProgramChainCellsAndPalette()
    {
        var ifo = new IfoParser(BuildVtsIfo(wideScreen: true));

        var programChain = Assert.Single(ifo.ProgramChains);
        Assert.Equal(new TimeSpan(0, 1, 2, 3, 400), programChain.Duration);
        Assert.Equal(2, programChain.ProgramCount);
        Assert.Equal(2, programChain.Cells.Count);
        Assert.Equal(1, programChain.Cells[1].VobId);
        Assert.Equal(2, programChain.Cells[1].CellId);
        Assert.Equal(TimeSpan.FromSeconds(10), programChain.Cells[1].Start);
        Assert.Equal(1000, programChain.Cells[1].FirstSector);
        Assert.Equal(1999, programChain.Cells[1].LastSector);
        Assert.Equal(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(10) }, programChain.ProgramStarts);
        Assert.True(programChain.HasSubtitleStream(1));
        Assert.False(programChain.HasSubtitleStream(2));

        Assert.Equal(16, ifo.Palette.Count);
        Assert.Equal(new SkiaSharp.SKColor(255, 255, 255), ifo.Palette[1]); // Y=235, Cr=Cb=128
        Assert.Equal(new SkiaSharp.SKColor(0, 0, 0), ifo.Palette[0]); // Y=16, Cr=Cb=128
    }

    [Fact]
    public void AngleBlock_KeepsFirstAngleAndCountsTimeOnce()
    {
        // cell 1 normal, cells 2-3 an angle block (angle 1 + angle 2), cell 4 normal - 10 seconds each
        var ifo = new IfoParser(BuildVtsIfo(
            wideScreen: true,
            cellCategories: new byte[] { 0, 0b01_01_0000, 0b11_01_0000, 0 },
            programEntryCells: new byte[] { 1, 2, 4 }));

        var programChain = Assert.Single(ifo.ProgramChains);
        Assert.Equal(3, programChain.Cells.Count);
        Assert.Equal(new[] { 1, 2, 4 }, programChain.Cells.Select(p => p.CellId));
        Assert.Equal(new[] { 0.0, 10, 20 }, programChain.Cells.Select(p => p.Start.TotalSeconds));
        Assert.Equal(1000, programChain.Cells[1].FirstSector);
        Assert.Equal(3000, programChain.Cells[2].FirstSector);
        Assert.Equal(new[] { 0.0, 10, 20 }, programChain.ProgramStarts.Select(p => p.TotalSeconds));
    }

    [Fact]
    public void LowercaseDvdFolder_FindsIfoAndVobFiles()
    {
        // a DVD copied to a case sensitive file system: video_ts/vts_01_0.ifo, vts_01_1.vob, ...
        var folder = Path.Combine(Path.GetTempPath(), "se-ifo-" + Guid.NewGuid().ToString("N"), "video_ts");
        Directory.CreateDirectory(folder);
        try
        {
            var vmg = new byte[SectorSize];
            Encoding.ASCII.GetBytes("DVDVIDEO-VMG").CopyTo(vmg, 0);
            File.WriteAllBytes(Path.Combine(folder, "video_ts.ifo"), vmg);
            File.WriteAllBytes(Path.Combine(folder, "vts_01_0.ifo"), BuildVtsIfo(wideScreen: true));
            File.WriteAllBytes(Path.Combine(folder, "vts_01_0.vob"), new byte[SectorSize]);
            File.WriteAllBytes(Path.Combine(folder, "vts_01_1.vob"), new byte[SectorSize * 2000]);
            File.WriteAllBytes(Path.Combine(folder, "vts_01_2.vob"), new byte[SectorSize]);

            var vobFiles = IfoParser.GetTitleVobFiles(Path.Combine(folder, "VTS_01_0.IFO"));
            Assert.Equal(2, vobFiles.Count);
            Assert.All(vobFiles, p => Assert.True(File.Exists(p)));
            Assert.EndsWith("1.vob", vobFiles[0], StringComparison.OrdinalIgnoreCase);

            var ifoFileName = IfoParser.GetIfoFileName(Path.Combine(folder, "vts_01_2.vob"));
            Assert.NotNull(ifoFileName);
            Assert.True(File.Exists(ifoFileName));

            var title = Assert.Single(DvdTitle.Find(Path.Combine(folder, "video_ts.ifo")));
            Assert.Equal(1, title.TitleSetNumber);
            Assert.Equal(2, title.VobFileNames.Count);
            Assert.True(title.IsComplete);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(folder)!, true);
        }
    }

    [Fact]
    public void RecognizesVideoManager()
    {
        var buffer = new byte[SectorSize];
        Encoding.ASCII.GetBytes("DVDVIDEO-VMG").CopyTo(buffer, 0);
        buffer[0x3F] = 3;

        var ifo = new IfoParser(buffer);

        Assert.Equal(IfoParser.IfoType.VideoManager, ifo.Type);
        Assert.Equal(3, ifo.TitleSetCount);
        Assert.Empty(ifo.ProgramChains);
    }

    [Fact]
    public void RejectsOtherFiles()
    {
        var ifo = new IfoParser(new byte[SectorSize]);

        Assert.Equal(IfoParser.IfoType.Unknown, ifo.Type);
        Assert.NotNull(ifo.ErrorMessage);
    }

    private static byte[] BuildVtsIfo(bool wideScreen, byte[]? cellCategories = null, byte[]? programEntryCells = null)
    {
        cellCategories ??= new byte[2];
        programEntryCells ??= new byte[] { 1, 2 };
        var cellCount = cellCategories.Length;
        var buffer = new byte[SectorSize * 3];
        Encoding.ASCII.GetBytes("DVDVIDEO-VTS").CopyTo(buffer, 0);
        WriteUInt32(buffer, 0xCC, PgciSector);
        WriteUInt32(buffer, 0xE0, CellAddressSector);

        // video: MPEG-2, PAL, 16:9 or 4:3, 720x576
        buffer[0x200] = (byte)(0b01_01_00_00 | (wideScreen ? 0b1100 : 0));

        // one AC3 6 channel English audio stream
        buffer[0x203] = 1;
        buffer[0x204] = 0b000_0_01_00;
        buffer[0x205] = 5;
        Encoding.ASCII.GetBytes("en").CopyTo(buffer, 0x206);

        // subtitles: English (normal), Danish (forced)
        buffer[0x255] = 2;
        buffer[0x256] = 1;
        Encoding.ASCII.GetBytes("en").CopyTo(buffer, 0x258);
        buffer[0x25B] = 1;
        buffer[0x25C] = 1;
        Encoding.ASCII.GetBytes("da").CopyTo(buffer, 0x25E);
        buffer[0x261] = 9;

        // PGCI with one program chain
        var table = PgciSector * SectorSize;
        buffer[table + 1] = 1;
        const int pgcOffset = 16;
        WriteUInt32(buffer, table + 12, pgcOffset);
        var pgc = table + pgcOffset;
        buffer[pgc + 2] = (byte)programEntryCells.Length; // programs
        buffer[pgc + 3] = (byte)cellCount; // cells
        buffer[pgc + 4] = 0x01;
        buffer[pgc + 5] = 0x02;
        buffer[pgc + 6] = 0x03;
        buffer[pgc + 7] = 0b01_00_1010; // 25 fps, 10 frames = 400 ms

        // stream control: 4:3, wide, letterbox, pan&scan
        buffer[pgc + 0x1C] = 0x80;
        buffer[pgc + 0x1D] = 0;
        buffer[pgc + 0x1E] = 1;
        buffer[pgc + 0x1F] = 0;
        buffer[pgc + 0x20] = 0x81;
        buffer[pgc + 0x21] = 2;
        buffer[pgc + 0x22] = 3;
        buffer[pgc + 0x23] = 2;

        // palette: black, white, rest grey
        for (var i = 0; i < 16; i++)
        {
            var index = pgc + 0xA4 + i * 4;
            buffer[index + 1] = (byte)(i == 0 ? 16 : i == 1 ? 235 : 128);
            buffer[index + 2] = 128;
            buffer[index + 3] = 128;
        }

        const int programMap = 0xEC;
        const int playbackInfo = 0xF0;
        var positionInfo = playbackInfo + cellCount * 24;
        WriteUInt16(buffer, pgc + 0xE6, programMap);
        WriteUInt16(buffer, pgc + 0xE8, playbackInfo);
        WriteUInt16(buffer, pgc + 0xEA, positionInfo);
        programEntryCells.CopyTo(buffer, pgc + programMap);
        for (var i = 0; i < cellCount; i++)
        {
            var play = pgc + playbackInfo + i * 24;
            buffer[play] = cellCategories[i];
            buffer[play + 6] = 0x10; // 10 seconds
            buffer[play + 7] = 0b01_000000;
            WriteUInt32(buffer, play + 8, (uint)(i * 1000));
            WriteUInt32(buffer, play + 20, (uint)(i * 1000 + 999));
            var position = pgc + positionInfo + i * 4;
            buffer[position + 1] = 1;
            buffer[position + 3] = (byte)(i + 1);
        }

        return buffer;
    }

    private static void WriteUInt16(byte[] buffer, int index, int value)
    {
        buffer[index] = (byte)(value >> 8);
        buffer[index + 1] = (byte)value;
    }

    private static void WriteUInt32(byte[] buffer, int index, uint value)
    {
        buffer[index] = (byte)(value >> 24);
        buffer[index + 1] = (byte)(value >> 16);
        buffer[index + 2] = (byte)(value >> 8);
        buffer[index + 3] = (byte)value;
    }
}
