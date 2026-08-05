using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using System.Text;

namespace LibSETests.ContainerFormats;

public class Mp4TextSampleTest
{
    private static byte[] MakeBox(string name, params byte[][] content)
    {
        var payload = content.SelectMany(b => b).ToArray();
        var size = 8 + payload.Length;
        var bytes = new List<byte>
        {
            (byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size,
        };
        bytes.AddRange(Encoding.ASCII.GetBytes(name));
        bytes.AddRange(payload);
        return bytes.ToArray();
    }

    private static byte[] UInt32(uint value) => new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

    private static byte[] UInt16(int value) => new[] { (byte)(value >> 8), (byte)value };

    /// <summary>
    /// tx3g sample: 16-bit text length, the UTF-8 text, then the modifier boxes.
    /// </summary>
    private static byte[] Tx3gSample(string text, params byte[][] modifierBoxes)
    {
        var textBytes = Encoding.UTF8.GetBytes(text);
        return UInt16(textBytes.Length).Concat(textBytes).Concat(modifierBoxes.SelectMany(b => b)).ToArray();
    }

    private static byte[] StylBox(params byte[][] records) => MakeBox("styl", UInt16(records.Length), records.SelectMany(r => r).ToArray());

    private static byte[] StylRecord(int startChar, int endChar, byte faceStyle, uint rgba = 0xFFFFFFFF)
    {
        return UInt16(startChar).Concat(UInt16(endChar))
            .Concat(UInt16(1))                     // font id
            .Concat(new[] { faceStyle, (byte)18 }) // face style flags + font size
            .Concat(UInt32(rgba)).ToArray();
    }

    // An "stxt"/"sbtt" sample is the text itself. Reading it as a tx3g sample ate the first
    // two characters as a length and then ran past the end of the sample.
    [Fact]
    public void SimpleTextSampleIsTheWholeSample()
    {
        var sample = Encoding.UTF8.GetBytes("Hello world");

        Assert.Equal("Hello world", Mp4TextSampleHelper.ReadSimpleTextSample(sample));
    }

    [Fact]
    public void SimpleTextSampleKeepsMarkupAndSkipsPadding()
    {
        var sample = Encoding.UTF8.GetBytes("Hello <i>world</i>").Concat(new byte[] { 0, 0 }).ToArray();

        Assert.Equal("Hello <i>world</i>", Mp4TextSampleHelper.ReadSimpleTextSample(sample));
    }

    [Theory]
    [InlineData("stxt", true)]
    [InlineData("sbtt", true)]
    [InlineData("tx3g", false)]
    [InlineData("wvtt", false)]
    [InlineData(null, false)]
    public void SimpleTextCodecsAreRecognized(string? codec, bool expected)
    {
        Assert.Equal(expected, Mp4TextSampleHelper.IsSimpleTextCodec(codec));
    }

    [Fact]
    public void Tx3gSampleWithoutStyleIsPlainText()
    {
        Assert.Equal("Hello world", Mp4TextSampleHelper.ReadTx3gSampleText(Tx3gSample("Hello world")));
    }

    [Fact]
    public void Tx3gSampleLongerThanTheTextIsRejected()
    {
        var sample = new byte[] { 0xFF, 0xFF, (byte)'a', (byte)'b' }; // declares 65535 bytes of text

        Assert.Null(Mp4TextSampleHelper.ReadTx3gSampleText(sample));
    }

    // styl offsets count characters, not bytes, so non-ASCII text must not shift them.
    [Fact]
    public void Tx3gStyleRecordMakesItalic()
    {
        var sample = Tx3gSample("Héllö wörld end", StylBox(StylRecord(6, 11, 2)));

        Assert.Equal("Héllö <i>wörld</i> end", Mp4TextSampleHelper.ReadTx3gSampleText(sample));
    }

    [Fact]
    public void Tx3gStyleRecordsCombine()
    {
        var sample = Tx3gSample("bold and italic", StylBox(StylRecord(0, 4, 1), StylRecord(9, 15, 2 | 4)));

        Assert.Equal("<b>bold</b> and <i><u>italic</u></i>", Mp4TextSampleHelper.ReadTx3gSampleText(sample));
    }

    [Fact]
    public void Tx3gStyleRecordWithColor()
    {
        var sample = Tx3gSample("red text", StylBox(StylRecord(0, 3, 0, 0xFF0000FF)));

        Assert.Equal("<font color=\"#ff0000\">red</font> text", Mp4TextSampleHelper.ReadTx3gSampleText(sample));
    }

    // White is the tx3g default color - tagging every cue with it would be noise.
    [Fact]
    public void Tx3gDefaultWhiteStyleRecordAddsNoTags()
    {
        var sample = Tx3gSample("plain text", StylBox(StylRecord(0, 5, 0)));

        Assert.Equal("plain text", Mp4TextSampleHelper.ReadTx3gSampleText(sample));
    }

    // QuickTime text tracks use "styl" too, but with 14-byte records - misreading those as
    // tx3g records would style arbitrary parts of the text.
    [Fact]
    public void QuickTimeStyleRecordsAreIgnored()
    {
        var quickTimeRecord = UInt16(0).Concat(UInt16(4)).Concat(UInt16(1))
            .Concat(new byte[] { 2, 18 })
            .Concat(new byte[] { 0, 0, 0, 0, 0, 0 }) // QuickTime uses a 6 byte color
            .ToArray();
        var sample = Tx3gSample("Hello world", MakeBox("styl", UInt16(1), quickTimeRecord));

        Assert.Equal("Hello world", Mp4TextSampleHelper.ReadTx3gSampleText(sample));
    }

    // The full stbl path: an "stxt" track with one sample, its text stored after the boxes.
    [Fact]
    public void StblReadsSimpleTextTrack()
    {
        var text = Encoding.UTF8.GetBytes("Hello world");
        var boxes = new[]
        {
            MakeBox("stsd", UInt32(0), UInt32(1), MakeBox("stxt", new byte[6], UInt16(1), new byte[] { 0, 0 })),
            MakeBox("stts", UInt32(0), UInt32(1), UInt32(1), UInt32(1000)),
            MakeBox("stsc", UInt32(0), UInt32(1), UInt32(1), UInt32(1), UInt32(1)),
            MakeBox("stsz", UInt32(0), UInt32(0), UInt32(1), UInt32((uint)text.Length)),
        }.SelectMany(b => b).ToList();
        const int stcoBoxSize = 8 + 4 + 4 + 4; // the text starts right after the stco box
        boxes.AddRange(MakeBox("stco", UInt32(0), UInt32(1), UInt32((uint)(boxes.Count + stcoBoxSize))));

        using var ms = new MemoryStream(boxes.Concat(text).ToArray());
        var stbl = new Stbl(ms, (ulong)boxes.Count, 1000, "text", null);

        var paragraphs = stbl.GetParagraphs();
        Assert.Single(paragraphs);
        Assert.Equal("Hello world", paragraphs[0].Text);
        Assert.Equal(0, paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(1000, paragraphs[0].EndTime.TotalMilliseconds);
    }

    // MP4Box stores the idx palette in the esds of the "mp4s" sample entry, as the DVD
    // stores it: 4 bytes per entry, (0, Y, Cr, Cb).
    [Fact]
    public void VobSubPaletteIsReadFromSampleEntry()
    {
        var dsi = new List<byte>();
        dsi.AddRange(new byte[] { 0x00, 0x10, 0x80, 0x80 }); // black
        dsi.AddRange(new byte[] { 0x00, 0xde, 0x80, 0x80 }); // near white
        while (dsi.Count < 64)
        {
            dsi.Add(0);
        }

        var esds = MakeBox("esds",
            UInt32(0),                                         // version + flags
            new byte[] { 0x03, (byte)(3 + 2 + 15 + 64) },      // ES_Descriptor
            new byte[] { 0x00, 0x00, 0x00 },                   // ES_ID + flags
            new byte[] { 0x04, (byte)(13 + 2 + 64) },          // DecoderConfigDescriptor
            new byte[13],                                      // object type, stream type, buffer size, bitrates
            new byte[] { 0x05, 64 },                           // DecoderSpecificInfo
            dsi.ToArray());
        var sampleEntryPayload = new byte[6].Concat(UInt16(1)).Concat(esds).ToArray();

        var palette = Mp4VobSubPalette.FromMp4sSampleEntry(sampleEntryPayload);

        Assert.NotNull(palette);
        Assert.Equal(16, palette!.Count);
        Assert.Equal(0x00, palette[0].Red);
        Assert.Equal(0x00, palette[0].Green);
        Assert.Equal(0x00, palette[0].Blue);
        Assert.InRange(palette[1].Red, 0xee, 0xf2);
        Assert.InRange(palette[1].Green, 0xee, 0xf2);
        Assert.InRange(palette[1].Blue, 0xee, 0xf2);
    }

    [Fact]
    public void VobSubPaletteIsNullWithoutEsds()
    {
        Assert.Null(Mp4VobSubPalette.FromMp4sSampleEntry(new byte[6].Concat(UInt16(1)).ToArray()));
        Assert.Null(Mp4VobSubPalette.FromMp4sSampleEntry(null));
    }
}
