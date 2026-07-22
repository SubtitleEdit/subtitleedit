using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class Tx3GTextOnlyTest
{
    private static Subtitle Load(byte[] bytes)
    {
        // LoadSubtitle only does anything when the file name carries the format's own extension.
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".tx3g");
        try
        {
            File.WriteAllBytes(path, bytes);
            var subtitle = new Subtitle();
            new Tx3GTextOnly().LoadSubtitle(subtitle, new List<string>(), path);
            return subtitle;
        }
        finally
        {
            File.Delete(path);
        }
    }

    // GetUInt is declared as int, so any box length whose leading byte is >= 0x80 comes back
    // negative. That used to pass the guard and throw ArgumentOutOfRangeException out of
    // LoadSubtitle ("Non-negative number required. (Parameter 'count')").
    [Theory]
    [InlineData(0xFF, 0xFF, 0xFF, 0xFF)] // -1
    [InlineData(0x80, 0x00, 0x00, 0x00)] // int.MinValue
    [InlineData(0xFF, 0xFF, 0xFF, 0xF8)] // -8
    public void NegativeBoxLengthDoesNotThrow(byte b0, byte b1, byte b2, byte b3)
    {
        var subtitle = Load(new byte[] { b0, b1, b2, b3, 65, 66, 67, 68, 69, 70 });

        Assert.Empty(subtitle.Paragraphs);
    }

    // A zero length is legal - timed text uses empty samples to clear the screen between cues.
    // It used to append an empty paragraph for every 4 bytes; it should be skipped instead.
    [Fact]
    public void ZeroBoxLengthIsSkippedNotTreatedAsText()
    {
        var subtitle = Load(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        Assert.Empty(subtitle.Paragraphs);
    }

    // Parsing must continue past a clear-screen sample rather than stopping at it.
    [Fact]
    public void ZeroBoxLengthDoesNotStopParsing()
    {
        var subtitle = Load(new byte[]
        {
            0x00, 0x00, 0x00, 0x02, 65, 66, // "AB"
            0x00, 0x00, 0x00, 0x00, // clear screen
            0x00, 0x00, 0x00, 0x03, 67, 68, 69, // "CDE"
        });

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("AB", subtitle.Paragraphs[0].Text);
        Assert.Equal("CDE", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void ReadsTextBoxes()
    {
        var subtitle = Load(new byte[]
        {
            0x00, 0x00, 0x00, 0x02, 65, 66, // "AB"
            0x00, 0x00, 0x00, 0x03, 67, 68, 69, // "CDE"
        });

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("AB", subtitle.Paragraphs[0].Text);
        Assert.Equal("CDE", subtitle.Paragraphs[1].Text);
    }

    // A length running past the end of the buffer must stop the loop rather than over-read.
    [Fact]
    public void BoxLengthPastEndOfBufferStops()
    {
        var subtitle = Load(new byte[] { 0x00, 0x00, 0x00, 0x64, 65, 66, 67 });

        Assert.Empty(subtitle.Paragraphs);
    }
}
