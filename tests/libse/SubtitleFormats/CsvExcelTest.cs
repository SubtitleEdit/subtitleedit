using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class CsvExcelTest
{
    private static Subtitle SaveAndReload(Subtitle subtitle)
    {
        var format = new CsvExcel();
        var text = format.ToText(subtitle, "test");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), "test.csv");
        return loaded;
    }

    [Fact]
    public void TimeCodesKeepMilliseconds()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1234, 5678));

        var loaded = SaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal(1234, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(5678, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void ActorAndForcedRoundTrip()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Forced line", 1000, 3000) { Actor = "Anna", Forced = true });
        subtitle.Paragraphs.Add(new Paragraph("Normal line", 4000, 6000) { Actor = "Bob" });

        var loaded = SaveAndReload(subtitle);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Anna", loaded.Paragraphs[0].Actor);
        Assert.True(loaded.Paragraphs[0].Forced);
        Assert.Equal("Bob", loaded.Paragraphs[1].Actor);
        Assert.False(loaded.Paragraphs[1].Forced);
    }

    [Fact]
    public void QuotesCommasAndLineBreaksRoundTrip()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Two lines," + Environment.NewLine + "and a \"quote\".", 1000, 3000));

        var loaded = SaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Two lines," + Environment.NewLine + "and a \"quote\".", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void HeaderRowIsNotImportedAsSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 3000));

        var text = new CsvExcel().ToText(subtitle, "test");

        Assert.StartsWith("\"Number\",\"Start time\",\"End time\",\"Text\",\"Actor\",\"Forced\"", text);
        Assert.Single(SaveAndReload(subtitle).Paragraphs);
    }

    [Fact]
    public void IsMineRequiresTheHeader()
    {
        var format = new CsvExcel();
        var withHeader = format.ToText(NewSubtitle(), "test").SplitToLines();
        Assert.True(format.IsMine(withHeader, "test.csv"));

        var withoutHeader = withHeader.Skip(1).ToList();
        Assert.False(format.IsMine(withoutHeader, "test.csv"));
    }

    // Other csv dialects must not be claimed - and this format must not be claimed by them.
    [Fact]
    public void OtherCsvDialectsAreNotMine()
    {
        var nuendo = new CsvNuendo().ToText(NewSubtitle(), "test").SplitToLines();
        Assert.False(new CsvExcel().IsMine(nuendo, "test.csv"));

        var daVinci = new CsvDaVinci().ToText(NewSubtitle(), "test").SplitToLines();
        Assert.False(new CsvExcel().IsMine(daVinci, "test.csv"));
    }

    [Fact]
    public void AutoDetectPicksCsvExcel()
    {
        var lines = new CsvExcel().ToText(NewSubtitle(), "test").SplitToLines();
        var format = SubtitleFormat.AllSubtitleFormats.First(f => f.IsMine(lines, "test.csv"));
        Assert.Equal(new CsvExcel().Name, format.Name);
    }

    [Fact]
    public void FramesTimeCodesAreAccepted()
    {
        var lines = new List<string>
        {
            "\"Number\",\"Start time\",\"End time\",\"Text\",\"Actor\",\"Forced\"",
            "\"1\",\"00:00:01:00\",\"00:00:02:00\",\"Hello\",\"\",\"False\"",
        };

        var loaded = new Subtitle();
        new CsvExcel().LoadSubtitle(loaded, lines, "test.csv");

        Assert.Single(loaded.Paragraphs);
        Assert.Equal(1000, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    // A spreadsheet may write "00:00:01.5" instead of the three digits the writer emits.
    [Fact]
    public void ShortFractionIsReadAsTenths()
    {
        var lines = new List<string>
        {
            "\"Number\",\"Start time\",\"End time\",\"Text\",\"Actor\",\"Forced\"",
            "\"1\",\"00:00:01.5\",\"00:00:02.05\",\"Hello\",\"\",\"False\"",
        };

        var loaded = new Subtitle();
        new CsvExcel().LoadSubtitle(loaded, lines, "test.csv");

        Assert.Single(loaded.Paragraphs);
        Assert.Equal(1500, loaded.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(2050, loaded.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    private static Subtitle NewSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 3000) { Actor = "Anna" });
        subtitle.Paragraphs.Add(new Paragraph("World", 4000, 6000));
        return subtitle;
    }
}
