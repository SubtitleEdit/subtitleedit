using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;

namespace LibSETests.ContainerFormats;

public class TeletextTablesTest
{
    // RemapG0Charset patches the Latin G0 row in place when a stream selects a national
    // subset, so a later decode of a different file must be able to start from the
    // original table or the previous file's national characters leak into it.
    [Fact]
    public void ResetLatinG0_RestoresMutatedNationalSubsetPositions()
    {
        var latin = (int)TeletextTables.G0CharsetsT.Latin;
        var position = TeletextTables.G0LatinNationalSubsetsPositions[0];
        var original = TeletextTables.G0[latin, position];

        TeletextTables.G0[latin, position] = 0x0141; // Ł - pretend a Polish stream was decoded
        TeletextTables.ResetLatinG0();

        Assert.Equal(original, TeletextTables.G0[latin, position]);
    }

    // The X/28 and M/29 charset designation field is 7 bits wide (0..127), but the subset
    // map only defines the first 56 ids - the decoder must treat the rest as unmapped
    // rather than index out of range.
    [Fact]
    public void G0LatinNationalSubsetsMap_CoversOnlyDefinedDesignations()
    {
        Assert.Equal(56, TeletextTables.G0LatinNationalSubsetsMap.Length);
    }
}
