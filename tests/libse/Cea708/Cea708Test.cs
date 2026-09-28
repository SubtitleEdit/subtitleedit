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

    private const byte Hide = 0x8A;
    private static readonly byte[] SpaItalic = { 0x90, 0x05, 0x80 };
    private static readonly byte[] SpaPlain = { 0x90, 0x05, 0x00 };
    private static readonly byte[] HideAll = { Hide, 0xFF };
    private static readonly byte[] DefineWindow0 = { 0x98, 0x38, 0x00, 0x00, 0x01, 0x1F, 0x09 }; // window style 1, pen style 1

    private static byte[] Bytes(params object[] parts)
    {
        var result = new List<byte>();
        foreach (var part in parts)
        {
            if (part is string text)
            {
                result.AddRange(System.Text.Encoding.ASCII.GetBytes(text));
            }
            else
            {
                result.AddRange((byte[])part);
            }
        }

        return result.ToArray();
    }

    [Fact]
    public void ItalicPenWrapsTextInItalicTags()
    {
        var text = Cea708Decoder.Decode(0, Bytes(SpaItalic, "Hi", HideAll), new CommandState(), false);
        Assert.Equal("<i>Hi</i>", text);
    }

    [Fact]
    public void ItalicRunInsideLineClosesBeforeTheSpace()
    {
        var text = Cea708Decoder.Decode(0, Bytes("Hi ", SpaItalic, "yo ", SpaPlain, "ok", HideAll), new CommandState(), false);
        Assert.Equal("Hi <i>yo</i> ok", text);
    }

    [Fact]
    public void ItalicPenSpansLineBreaks()
    {
        var text = Cea708Decoder.Decode(0, Bytes(new byte[] { 0x92, 0, 0 }, SpaItalic, "Hi", new byte[] { 0x92, 1, 0 }, "yo", HideAll), new CommandState(), false);
        Assert.Equal("<i>Hi" + Environment.NewLine + "yo</i>", text);
    }

    // A new DefineWindow (or Reset) re-applies a predefined pen style, so a later caption with no
    // SetPenAttributes of its own must not inherit the italics of the one before.
    [Fact]
    public void DefineWindowWithPenStyleResetsItalics()
    {
        var state = new CommandState();
        Assert.Equal("<i>Hi</i>", Cea708Decoder.Decode(0, Bytes(DefineWindow0, SpaItalic, "Hi", HideAll), state, false));
        Assert.Equal("yo", Cea708Decoder.Decode(1, Bytes(DefineWindow0, "yo", HideAll), state, false));
    }

    [Fact]
    public void ItalicPenCarriesOverToTheNextCaption()
    {
        var state = new CommandState();
        Assert.Equal("<i>Hi</i>", Cea708Decoder.Decode(0, Bytes(SpaItalic, "Hi", HideAll), state, false));
        Assert.Equal("<i>yo</i>", Cea708Decoder.Decode(1, Bytes("yo", HideAll), state, false));
    }
}
