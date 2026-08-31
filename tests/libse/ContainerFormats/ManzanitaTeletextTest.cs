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
    public void ItalicIsDroppedButAccentsSurvive()
    {
        var subtitle = MakeSubtitle(new Paragraph("<i>Voilà</i>", 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal("Voilà", paragraphs[0].Text);
    }

    [Theory]
    // The G2 supplementary set, reached through an X/26 triplet with mode 0x0f.
    [InlineData("♪ La la la ♪")]
    [InlineData("Il coûte 5 €")]
    [InlineData("© Nikse, ® and ™")]
    [InlineData("«Bonjour» ¿Qué? ¡Vaya!")]
    [InlineData("Œuvre, œuf, Ærø, ø and ß")]
    // A G0 letter plus a diacritical mark, X/26 modes 0x11-0x1f.
    [InlineData("Voilà, très élégant")]
    [InlineData("Größe, Übermäßig, schön")]
    [InlineData("Zażółć gęślą jaźń")]
    [InlineData("Příliš žluťoučký kůň")]
    // Teletext gives these codes to the national option sub-sets, so they need the G0 lookup.
    [InlineData("# is not £, and @ is @")]
    public void RoundTripKeepsNonAsciiCharacters(string text)
    {
        var subtitle = MakeSubtitle(new Paragraph(text, 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal(text, paragraphs[0].Text);
    }

    [Fact]
    public void EnhancedCharactersSurviveOnEveryRow()
    {
        var subtitle = MakeSubtitle(new Paragraph(
            "♪ Hey, es klingt ein bisschen weirdo" + Environment.NewLine +
            "Doch ich soll deine Nummer klären. ♪", 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal("♪ Hey, es klingt ein bisschen weirdo" + Environment.NewLine +
                     "Doch ich soll deine Nummer klären. ♪", paragraphs[0].Text);
    }

    [Fact]
    public void MoreEnhancementsThanOnePacketHoldsAreAllWritten()
    {
        // Thirteen triplets fit in a packet, and every row spends one on its active position.
        var text = string.Concat(Enumerable.Repeat("é", 30));
        var subtitle = MakeSubtitle(new Paragraph(text, 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal(text, paragraphs[0].Text);
    }

    [Fact]
    public void X26EnhancementsAreRead()
    {
        // The fixture holds "mail*se.org # note" in the row, with an X/26 packet putting "@" over
        // the star (mode 0x10, a G0 character without a diacritical mark) and the music note over
        // the hash (mode 0x0f, G2 code 0x55) - the shapes ZDF and arte transmit.
        var parser = new ManzanitaTransportStreamParser();
        parser.Parse(Path.Combine("Files", "teletext_x26_enhancements.dvbttx"));

        var paragraphs = parser.GetTeletext()[888];

        Assert.Equal("mail@se.org ♪ note", paragraphs[0].Text);
    }

    [Fact]
    public void X28NationalOptionIsRead()
    {
        // The fixture declares the French sub-set in an X/28/0 packet and then sends the codes
        // 0x23 and 0x40, which French fills with "é" and "à" - what canal+ transmits. The three
        // national option bits run the other way round in that packet than in the page header,
        // and reading them the header's way lands on German instead ("d#j§ all#s").
        var parser = new ManzanitaTransportStreamParser();
        parser.Parse(Path.Combine("Files", "teletext_x28_national_option.dvbttx"));

        var paragraphs = parser.GetTeletext()[888];

        Assert.Equal("On est déjà allés", paragraphs[0].Text);
    }

    [Fact]
    public void X28ColourMapAndX26ForegroundColourAreRead()
    {
        // The fixture carries ZDF's own X/28/0 colour map, where colour map entry 17 - CLUT 2,
        // the first table a broadcaster may redefine - is the orange #ff8822 from their page 100,
        // and an X/26 foreground colour triplet that paints the row with it. Level 1 can only
        // name the first eight entries, so nothing but the enhancement can reach this colour.
        var parser = new ManzanitaTransportStreamParser();
        parser.Parse(Path.Combine("Files", "teletext_x28_colour_map.dvbttx"));

        var paragraphs = parser.GetTeletext()[888];

        Assert.Equal("<font color=\"#ff8822\">Level 2.5 orange</font>", paragraphs[0].Text);
    }

    [Fact]
    public void UnsupportedCharactersAreStillFolded()
    {
        var subtitle = MakeSubtitle(new Paragraph("[Привет] and 日本", 1000, 3000));

        var paragraphs = WriteAndRead(subtitle)[888];

        // No teletext code and nothing to fold to, so the brackets are the closest stand-in.
        Assert.Equal("(??????) and ??", paragraphs[0].Text);
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

    [Fact]
    public void AFeatureLengthFileIsReadBackWhole()
    {
        // The XML preamble holds one <packet /> line per teletext packet - two per subtitle here,
        // one to show it and one to erase it - so it grows past the 200 KB the reader used to
        // look at somewhere around 1650 subtitles. Everything beyond that read back as an empty
        // file, with no error: the end tag was never found, so the packet index came out empty.
        var subtitle = new Subtitle();
        for (var i = 0; i < 2000; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph("Line " + i, i * 3000, i * 3000 + 2000));
        }

        var paragraphs = WriteAndRead(subtitle)[888];

        Assert.Equal(2000, paragraphs.Count);
        Assert.Equal("Line 0", paragraphs[0].Text);
        Assert.Equal("Line 1999", paragraphs[1999].Text);
    }

    [Fact]
    public void AFileWithNoEndTagIsNotRead()
    {
        var parser = new ManzanitaTransportStreamParser();
        using var ms = new MemoryStream(new byte[500_000]);

        parser.Parse(ms);

        Assert.Empty(parser.GetTeletext());
    }
}
