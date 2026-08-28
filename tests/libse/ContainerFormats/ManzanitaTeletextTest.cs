using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;

namespace LibSETests.ContainerFormats;

public class ManzanitaTeletextTest
{
    private static Dictionary<int, List<Paragraph>> WriteAndRead(Subtitle subtitle, int pageNumber = 888)
    {
        var writer = new ManzanitaTeletextWriter { PageNumber = pageNumber, Date = new DateTime(2026, 1, 1) };
        var parser = new ManzanitaTransportStreamParser();
        using var ms = new MemoryStream(writer.GetBytes(subtitle));
        parser.Parse(ms);
        return parser.GetTeletext();
    }

    private static Subtitle MakeSubtitle(params Paragraph[] paragraphs)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.AddRange(paragraphs);
        return subtitle;
    }

    [Fact]
    public void RoundTripKeepsTextAndTimeCodes()
    {
        var subtitle = MakeSubtitle(
            new Paragraph("Hello world!", 1000, 3000),
            new Paragraph("Two lines" + Environment.NewLine + "of text.", 5000, 8500));

        var pages = WriteAndRead(subtitle);

        var paragraphs = Assert.Contains(888, (IDictionary<int, List<Paragraph>>)pages);
        Assert.Equal(2, paragraphs.Count);
        Assert.Equal("Hello world!", paragraphs[0].Text);
        Assert.Equal(1000, paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Two lines" + Environment.NewLine + "of text.", paragraphs[1].Text);
        Assert.Equal(5000, paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(8500, paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void RoundTripKeepsTeletextColors()
    {
        var subtitle = MakeSubtitle(new Paragraph("<font color=\"#00ff00\">Green line</font>", 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal("<font color=\"#00ff00\">Green line</font>", paragraphs[0].Text);
    }

    [Fact]
    public void RoundTripKeepsTopAlignment()
    {
        var subtitle = MakeSubtitle(new Paragraph("{\\an8}Up here", 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal("{\\an8}Up here", paragraphs[0].Text);
    }

    [Fact]
    public void WritesTheRequestedPage()
    {
        var subtitle = MakeSubtitle(new Paragraph("On page 801", 1000, 3000));

        var pages = WriteAndRead(subtitle, 801);

        Assert.Equal(new[] { 801 }, pages.Keys.ToArray());
    }

    [Fact]
    public void SubtitlesFollowingEachOtherKeepTheirOwnEndTime()
    {
        // No room for an erase packet between the two - the second page header ends the first.
        var subtitle = MakeSubtitle(
            new Paragraph("First", 1000, 2000),
            new Paragraph("Second", 2040, 4000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal(2, paragraphs.Count);
        Assert.Equal(2000, paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(2040, paragraphs[1].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void ItalicAndUnsupportedCharactersAreFolded()
    {
        var subtitle = MakeSubtitle(new Paragraph("<i>Voilà</i>", 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal("Voila", paragraphs[0].Text);
    }

    [Fact]
    public void AFullWidthLineIsNotClipped()
    {
        // 36 characters plus the double height and box attributes need all 40 cells of the row.
        var subtitle = MakeSubtitle(new Paragraph("Dave's lying was a terrible burden...", 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal("Dave's lying was a terrible burden...", paragraphs[0].Text);
    }

    [Fact]
    public void WrittenFileIsRecognizedAsManzanita()
    {
        var writer = new ManzanitaTeletextWriter { Date = new DateTime(2026, 1, 1) };
        var bytes = writer.GetBytes(MakeSubtitle(new Paragraph("Hello", 1000, 3000)));

        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dvbttx");
        try
        {
            File.WriteAllBytes(fileName, bytes);
            Assert.True(FileUtil.IsManzanita(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void EmptySubtitleWritesNoPackets()
    {
        var pages = WriteAndRead(new Subtitle());

        Assert.Empty(pages);
    }
}
