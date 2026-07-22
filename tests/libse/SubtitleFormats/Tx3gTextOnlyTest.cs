using System.Text;
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

    // A minimal tx3g sample description: 30 fixed bytes, then a font table with one "Arial" entry.
    private static byte[] Header()
    {
        var header = new byte[30 + 2 + 3 + 5];
        header[25] = 0x24; // font size 36, just so the record is not entirely zero
        header[31] = 0x01; // font table entry count = 1
        header[33] = 0x01; // font id = 1
        header[34] = 0x05; // name length
        Encoding.ASCII.GetBytes("Arial").CopyTo(header, 35);
        return header;
    }

    private static byte[] Build(params byte[][] parts)
    {
        var all = new List<byte>(Header());
        foreach (var part in parts)
        {
            all.AddRange(part);
        }

        return all.ToArray();
    }

    private static byte[] TextSample(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var sample = new byte[2 + bytes.Length];
        sample[0] = (byte)(bytes.Length >> 8);
        sample[1] = (byte)(bytes.Length & 0xFF);
        bytes.CopyTo(sample, 2);
        return sample;
    }

    private static byte[] ClearScreenSample() => new byte[] { 0x00, 0x00 };

    private static byte[] StylBox() => new byte[]
    {
        0x00, 0x00, 0x00, 0x16, 0x73, 0x74, 0x79, 0x6C, // size 22, "styl"
        0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0x00, 0x24, 0xFF, 0xFF, 0xFF, 0xFF,
    };

    [Fact]
    public void ReadsTextSamples()
    {
        var subtitle = Load(Build(TextSample("Hello"), TextSample("World")));

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal("World", subtitle.Paragraphs[1].Text);
    }

    // Timed text blanks the screen between cues with an empty sample; it is not a terminator.
    [Fact]
    public void SkipsClearScreenSamplesAndKeepsGoing()
    {
        var subtitle = Load(Build(
            TextSample("first"), ClearScreenSample(),
            TextSample("second"), ClearScreenSample(), ClearScreenSample(),
            TextSample("third")));

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(new[] { "first", "second", "third" }, subtitle.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void SkipsStylModifierBoxes()
    {
        var subtitle = Load(Build(
            TextSample("styled"), StylBox(),
            TextSample("plain")));

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("styled", subtitle.Paragraphs[0].Text);
        Assert.Equal("plain", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void ReadsUtf16TextWithByteOrderMark()
    {
        var utf16 = new List<byte> { 0xFE, 0xFF };
        utf16.AddRange(Encoding.BigEndianUnicode.GetBytes("hej"));
        var sample = new List<byte> { 0x00, (byte)utf16.Count };
        sample.AddRange(utf16);

        var subtitle = Load(Build(sample.ToArray()));

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("hej", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void ConvertsLineFeedsToNewLines()
    {
        var subtitle = Load(Build(TextSample("line one\nline two")));

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("line one" + Environment.NewLine + "line two", subtitle.Paragraphs[0].Text);
    }

    // A truncated file must stop cleanly rather than over-read or throw.
    [Fact]
    public void TruncatedSampleStopsWithoutThrowing()
    {
        var subtitle = Load(Build(TextSample("ok"), new byte[] { 0x00, 0x64, 65, 66 }));

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("ok", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void FileShorterThanSampleDescriptionIsRejected()
    {
        var subtitle = Load(new byte[] { 0x00, 0x01, 0x02 });

        Assert.Empty(subtitle.Paragraphs);
    }

    // Real MP4Box output: "MP4Box -raw 3 sample_MP4.mp4" over the tx3g track in sample_MP4.mp4.
    // The expected text was cross-checked against "ffmpeg -i sample_MP4.mp4 -c:s srt", which
    // yields the same 808 cues in the same order.
    [Fact]
    public void ReadsRealMp4BoxExport()
    {
        var subtitle = new Subtitle();
        new Tx3GTextOnly().LoadSubtitle(subtitle, new List<string>(), "Files/sample_tx3g.tx3g");

        Assert.Equal(808, subtitle.Paragraphs.Count);
        Assert.Equal("(Alan) 'Chapter one. Sorrow.", subtitle.Paragraphs[0].Text);
        Assert.Equal("'There were many who" + Environment.NewLine + "considered Atticus pund", subtitle.Paragraphs[1].Text);
        Assert.Equal("(♪ Theme music)", subtitle.Paragraphs[807].Text);
        Assert.DoesNotContain(subtitle.Paragraphs, p => string.IsNullOrEmpty(p.Text));
    }

    [Fact]
    public void RealMp4BoxExportIsDetectedByFormatDetection()
    {
        var format = SubtitleFormat.AllSubtitleFormats
            .First(f => f.IsMine(new List<string>(), "Files/sample_tx3g.tx3g"));

        Assert.Equal("tx3g", format.Name);
    }

    // tx3g is binary, so it must also be offered to the binary load paths - batch convert, OCR,
    // point sync, join subtitles and seconv all iterate GetBinaryFormats rather than
    // AllSubtitleFormats.
    [Fact]
    public void IsRegisteredAsABinaryFormat()
    {
        Assert.Contains(SubtitleFormat.GetBinaryFormats(false), f => f.Name == "tx3g");
    }

    // The binary load path calls IsMine(null, fileName) - a null line list must not throw.
    [Fact]
    public void IsMineAcceptsNullLines()
    {
        var format = SubtitleFormat.GetBinaryFormats(false).First(f => f.Name == "tx3g");

        Assert.True(format.IsMine(null, "Files/sample_tx3g.tx3g"));
        Assert.False(format.IsMine(null, "Files/auto_detect_Danish.srt"));
    }
}
