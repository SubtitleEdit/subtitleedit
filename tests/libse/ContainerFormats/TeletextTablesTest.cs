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

    // ETS 300 706, chapter 8.3 - a triplet written by ManzanitaTeletextWriter has to survive
    // the decoder, including its single bit error correction.
    [Theory]
    [InlineData(0x00000)]
    [InlineData(0x3ffff)]
    [InlineData(0x05937)] // set active position, row 15 column 11
    [InlineData(0x2ABC3)] // a G2 character
    public void Hamming2418_RoundTrips(int value)
    {
        var encoded = TeletextHamming.Hamming2418Encode(value);

        Assert.Equal((uint)value, TeletextHamming.UnHamming2418(encoded));
        for (var bit = 0; bit < 24; bit++)
        {
            Assert.Equal((uint)value, TeletextHamming.UnHamming2418(encoded ^ (1 << bit)));
        }
    }

    // Teletext is not ASCII: the Latin G0 set keeps "£" at 0x23 and puts "#" at 0x5f, and it has
    // no code at all for "[", "]" or "{" to "~".
    [Fact]
    public void TryGetLatinG0Code_UsesTheTeletextCodes()
    {
        Assert.True(TeletextTables.TryGetLatinG0Code('#', out var hash));
        Assert.Equal(0x5f, hash);
        Assert.True(TeletextTables.TryGetLatinG0Code('\u00a3', out var pound));
        Assert.Equal(0x23, pound);
        Assert.False(TeletextTables.TryGetLatinG0Code('[', out _));
        Assert.False(TeletextTables.TryGetLatinG0Code('~', out _));
    }

    [Fact]
    public void TryGetG2Replacement_FindsG2CharactersAndDiacriticalMarks()
    {
        Assert.True(TeletextTables.TryGetG2Replacement('\u266a', out var note));
        Assert.Equal(TeletextTables.G2Mode, note.Mode);
        Assert.Equal(0x55, note.Data);

        Assert.True(TeletextTables.TryGetG2Replacement('\u00e4', out var aUmlaut));
        Assert.Equal(0x18, aUmlaut.Mode);
        Assert.Equal((byte)'a', aUmlaut.Data);

        // A plain space is in the G2 table three times over, and needs no enhancement.
        Assert.False(TeletextTables.TryGetG2Replacement(' ', out _));
    }
}
