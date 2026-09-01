using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;

namespace LibSETests.Common;

/// <summary>
/// Guard tests for the 2026-08-27 bug hunt (sweep 14): binary-parser strides and bounds, and
/// guards whose comparison was off by one.
/// </summary>
public class BugHunt14Test
{
    /// <summary>
    /// Builds the payload of a PAT section carrying <paramref name="programNumbers"/> entries,
    /// laid out exactly as ISO/IEC 13818-1 2.4.4.3 describes.
    /// </summary>
    private static byte[] MakePatPacket(params int[] programNumbers)
    {
        var sectionLength = 5 + (programNumbers.Length * 4) + 4; // header after length + N*4 + CRC32
        var buffer = new List<byte>
        {
            0x00, // pointer field
            0x00, // table id
            (byte)(0xB0 | ((sectionLength >> 8) & 0x03)),
            (byte)(sectionLength & 0xFF),
            0x00, 0x01, // transport stream id
            0xC1,       // version / current-next
            0x00,       // section number
            0x00,       // last section number
        };

        foreach (var programNumber in programNumbers)
        {
            buffer.Add((byte)(programNumber >> 8));
            buffer.Add((byte)(programNumber & 0xFF));
            buffer.Add((byte)(0xE0 | ((0x100 + programNumber) >> 8)));  // reserved + PID high
            buffer.Add((byte)((0x100 + programNumber) & 0xFF));         // PID low
        }

        buffer.AddRange(new byte[] { 0, 0, 0, 0 }); // CRC32
        return buffer.ToArray();
    }

    [Fact]
    public void ProgramAssociationTable_ReadsEveryProgram_NotEveryOther()
    {
        // Entries are 4 bytes, but the loop stepped 8 and counted (SectionLength - 5) / 8 - right
        // by accident for the single-program case, and silently dropping programs otherwise.
        var pat = new ProgramAssociationTable(MakePatPacket(1, 2, 3), 0);

        Assert.Equal(new List<int> { 1, 2, 3 }, pat.ProgramNumbers);
        Assert.Equal(new List<int> { 0x101, 0x102, 0x103 }, pat.ProgramIds);
    }

    [Fact]
    public void ProgramAssociationTable_SingleProgram_StillWorks()
    {
        var pat = new ProgramAssociationTable(MakePatPacket(1), 0);

        Assert.Equal(new List<int> { 1 }, pat.ProgramNumbers);
    }
}
