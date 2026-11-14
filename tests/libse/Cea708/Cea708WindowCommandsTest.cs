using Nikse.SubtitleEdit.Core.Cea708.Commands;

namespace LibSETests.Cea708;

public class Cea708WindowCommandsTest
{
    [Theory]
    [InlineData(0b00000000)]
    [InlineData(0b00000001)]
    [InlineData(0b10000000)]
    [InlineData(0b10100101)]
    [InlineData(0b11111111)]
    public void WindowBitmapRoundTrips(int bitmap)
    {
        var bytes = new byte[] { 0, (byte)bitmap };

        AssertRoundTrip(new ClearWindows(0, bytes, 1).Flags, new ClearWindows(0, bytes, 1).GetBytes(), ClearWindows.Id, bitmap);
        AssertRoundTrip(new DisplayWindows(0, bytes, 1).Flags, new DisplayWindows(0, bytes, 1).GetBytes(), DisplayWindows.Id, bitmap);
        AssertRoundTrip(new HideWindows(0, bytes, 1).Flags, new HideWindows(0, bytes, 1).GetBytes(), HideWindows.Id, bitmap);
        AssertRoundTrip(new ToggleWindows(0, bytes, 1).Flags, new ToggleWindows(0, bytes, 1).GetBytes(), ToggleWindows.Id, bitmap);
        AssertRoundTrip(new DeleteWindows(0, bytes, 1).Flags, new DeleteWindows(0, bytes, 1).GetBytes(), DeleteWindows.Id, bitmap);
    }

    [Fact]
    public void AllConstructorsSetEveryWindow()
    {
        Assert.Equal(new byte[] { (byte)HideWindows.Id, 0xFF }, new HideWindows(true).GetBytes());
        Assert.Equal(new byte[] { (byte)ToggleWindows.Id, 0xFF }, new ToggleWindows(true).GetBytes());
        Assert.Equal(new byte[] { (byte)DeleteWindows.Id, 0x00 }, new DeleteWindows(false).GetBytes());
    }

    private static void AssertRoundTrip(bool[] flags, byte[] encoded, int id, int bitmap)
    {
        Assert.Equal(8, flags.Length);
        for (var i = 0; i < 8; i++)
        {
            Assert.Equal((bitmap & (1 << i)) != 0, flags[i]);
        }

        Assert.Equal(new[] { (byte)id, (byte)bitmap }, encoded);
    }
}
