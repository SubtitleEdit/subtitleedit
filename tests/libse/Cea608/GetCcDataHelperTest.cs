using Nikse.SubtitleEdit.Core.Cea608;

namespace LibSETests.Cea608;

public class GetCcDataHelperTest
{
    // endPos <= startPos used to underflow in ulong arithmetic and attempt a huge allocation.
    [Theory]
    [InlineData(10ul, 10ul)]
    [InlineData(10ul, 2ul)]
    public void GetSeiDataWithNonPositiveRangeReturnsEmpty(ulong startPos, ulong endPos)
    {
        using var ms = new MemoryStream(new byte[16]);
        Assert.Empty(GetCcDataHelper.GetSeiData(ms, startPos, endPos));
    }

    [Fact]
    public void ParseCcDataFromSeiAllPayloadTypeContinuationsDoesNotThrow()
    {
        // 0xFF keeps the payload-type accumulation loop going; it used to run off the buffer.
        var fieldData = new List<CcData>();
        GetCcDataHelper.ParseCcDataFromSei(new byte[] { 0xFF, 0xFF, 0xFF }, fieldData);
        Assert.Empty(fieldData);
    }

    [Fact]
    public void ParseCcDataFromSeiMagicAtEndOfBufferDoesNotThrow()
    {
        // payload type 4, size 0, then exactly the two cc-data magic words ending the buffer.
        // The header check must also account for the cc-count byte read at pos + 8, which
        // lies one past the magic words - this input used to throw IndexOutOfRangeException.
        var buffer = new byte[]
        {
            0x04, 0x00,
            0xB5, 0x00, 0x31, 0x47, // 3036688711
            0x41, 0x39, 0x34, 0x03, // 1094267907
        };

        var fieldData = new List<CcData>();
        GetCcDataHelper.ParseCcDataFromSei(buffer, fieldData);
        Assert.Empty(fieldData);
    }
}
