using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// The .dvbttx toolbar format - a thin <see cref="SubtitleFormat"/> shell around the Manzanita
/// teletext writer and parser, with the page number and language riding on the subtitle header
/// the way an EBU STL subtitle keeps its GSI block there.
/// </summary>
public class DvbTeletextTest
{
    private static string WriteTempFile(Subtitle subtitle, DvbTeletext format)
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dvbttx");
        using (var ms = new MemoryStream())
        {
            Assert.True(format.Save(fileName, ms, subtitle, batchMode: true));
            File.WriteAllBytes(fileName, ms.ToArray());
        }

        return fileName;
    }

    [Fact]
    public void SaveAndLoadRoundTripsTextColorsAndPage()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world!", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("<font color=\"#ff8822\">Level 2.5 orange</font>", 5000, 8000));
        subtitle.Header = DvbTeletext.CreateHeader(771, "ger");

        var format = new DvbTeletext();
        var fileName = WriteTempFile(subtitle, format);
        try
        {
            Assert.True(format.IsMine(null, fileName));

            var loaded = new Subtitle();
            new DvbTeletext().LoadSubtitle(loaded, null, fileName);

            Assert.Equal(2, loaded.Paragraphs.Count);
            Assert.Equal("Hello world!", loaded.Paragraphs[0].Text);
            Assert.Equal("<font color=\"#ff8822\">Level 2.5 orange</font>", loaded.Paragraphs[1].Text);

            Assert.True(DvbTeletext.TryParseHeader(loaded.Header, out var page, out var language));
            Assert.Equal(771, page);
            Assert.Equal("ger", language);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void SaveWithoutHeaderUsesTheFormatDefaults()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 3000));

        var format = new DvbTeletext();
        var fileName = WriteTempFile(subtitle, format);
        try
        {
            var loaded = new Subtitle();
            new DvbTeletext().LoadSubtitle(loaded, null, fileName);

            Assert.True(DvbTeletext.TryParseHeader(loaded.Header, out var page, out var language));
            Assert.Equal(888, page);
            Assert.Equal("eng", language);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<dvbteletext page=\"1200\" language=\"eng\" />")] // out of page range
    [InlineData("some 1024 byte STL header")]
    public void HeadersOfOtherShapesAreNotDvbTeletextHeaders(string? header)
    {
        Assert.False(DvbTeletext.IsDvbTeletextHeader(header));
    }

    [Fact]
    public void IsMineRejectsATextSubtitle()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dvbttx");
        File.WriteAllText(fileName, "1\n00:00:01,000 --> 00:00:03,000\nHello\n");
        try
        {
            Assert.False(new DvbTeletext().IsMine(null, fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
