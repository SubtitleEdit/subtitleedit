using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

/// <summary>
/// <see cref="NikseBitmap.IsEqualTo"/> compares the raw pixel buffers, and the
/// (width, height, byte[]) constructor takes an externally supplied buffer, so equal dimensions
/// do not by themselves guarantee equal buffer lengths. CDG rendering calls this once per packet.
/// </summary>
public class NikseBitmapEqualityTest
{
    private static byte[] MakeBuffer(int length, byte seed = 0)
    {
        var buffer = new byte[length];
        for (var i = 0; i < length; i++)
        {
            buffer[i] = (byte)(i * 7 + seed);
        }

        return buffer;
    }

    [Fact]
    public void IsEqualToMatchesIdenticalBuffers()
    {
        var one = new NikseBitmap(4, 3, MakeBuffer(4 * 3 * 4));
        var other = new NikseBitmap(4, 3, MakeBuffer(4 * 3 * 4));

        Assert.True(one.IsEqualTo(other));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(23)]
    [InlineData(4 * 3 * 4 - 1)] // the very last byte
    public void IsEqualToRejectsASingleDifferingByte(int index)
    {
        var buffer = MakeBuffer(4 * 3 * 4);
        var changed = MakeBuffer(4 * 3 * 4);
        changed[index] = (byte)(changed[index] ^ 0xFF);

        Assert.False(new NikseBitmap(4, 3, buffer).IsEqualTo(new NikseBitmap(4, 3, changed)));
    }

    [Fact]
    public void IsEqualToRejectsDifferentDimensions()
    {
        var one = new NikseBitmap(4, 3, MakeBuffer(4 * 3 * 4));
        var other = new NikseBitmap(3, 4, MakeBuffer(4 * 3 * 4));

        Assert.False(one.IsEqualTo(other));
    }

    [Fact]
    public void IsEqualToMatchesTwoEmptyBitmaps()
    {
        Assert.True(new NikseBitmap(0, 0, Array.Empty<byte>()).IsEqualTo(new NikseBitmap(0, 0, Array.Empty<byte>())));
    }

    // Same dimensions but a shorter buffer on the left used to compare only the shared prefix and
    // report a match; a shorter buffer on the right threw IndexOutOfRangeException.
    [Theory]
    [InlineData(4 * 3 * 4, 4 * 3 * 4 - 4)]
    [InlineData(4 * 3 * 4 - 4, 4 * 3 * 4)]
    public void IsEqualToRejectsMismatchedBufferLengths(int leftLength, int rightLength)
    {
        var one = new NikseBitmap(4, 3, MakeBuffer(leftLength));
        var other = new NikseBitmap(4, 3, MakeBuffer(rightLength));

        Assert.False(one.IsEqualTo(other));
    }
}
