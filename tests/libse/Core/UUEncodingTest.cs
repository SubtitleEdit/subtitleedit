using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class UUEncodingTest
{
    [Fact]
    public void ForwardAndBackAgain()
    {
        var byteArray = new byte[byte.MaxValue];
        for (int i = byte.MinValue; i < byte.MaxValue; i++)
        {
            byteArray[i] = (byte)i;
        }

        var text = UUEncoding.UUEncode(byteArray);
        var newBytes = UUEncoding.UUDecode(text);

        Assert.Equal(byteArray.Length, newBytes.Length);
        for (int i = byte.MinValue; i < byte.MaxValue; i++)
        {
            Assert.Equal(byteArray[i], newBytes[i]);
        }
    }
}
