using Nikse.SubtitleEdit.Core.Cea708;
using Cea708Decoder = Nikse.SubtitleEdit.Core.Cea708.Cea708;

namespace LibSETests.Cea708;

public class Cea708Test
{
    // The one-byte-argument window/Delay commands (0x88-0x8D) used to read bytes[i + 1]
    // without checking the remaining length, throwing on a buffer ending with the command id.
    [Theory]
    [InlineData(0x88)] // ClearWindows
    [InlineData(0x89)] // DisplayWindows
    [InlineData(0x8A)] // HideWindows
    [InlineData(0x8B)] // ToggleWindows
    [InlineData(0x8C)] // DeleteWindows
    [InlineData(0x8D)] // Delay
    public void DecodeTruncatedOneByteArgumentCommandDoesNotThrow(byte commandId)
    {
        var result = Cea708Decoder.Decode(0, new[] { commandId }, new CommandState(), true);
        Assert.NotNull(result);
    }
}
