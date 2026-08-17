using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class CsvDaVinciTest
{
    private static Subtitle SaveAndReload(Subtitle subtitle)
    {
        var format = new CsvDaVinci();
        var text = format.ToText(subtitle, "test");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), null);
        return loaded;
    }

    // The reader parsed the Play flag out of the text column instead of the play column,
    // so an exported "True" always read back as "False".
    [Fact]
    public void PlayFlagRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Plays audio.", 1000, 3000) { Effect = "True" });
        subtitle.Paragraphs.Add(new Paragraph("Does not.", 4000, 6000));

        var loaded = SaveAndReload(subtitle);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("True", loaded.Paragraphs[0].Effect);
        Assert.Equal("False", loaded.Paragraphs[1].Effect);
    }

    [Fact]
    public void ActorMultiLineAndQuotesRoundTrip()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Two lines here" + Environment.NewLine + "second line.", 1000, 3000) { Actor = "Anna" });
        subtitle.Paragraphs.Add(new Paragraph("Quote \"inside\" text, and, commas.", 4000, 6000) { Actor = "Bob" });

        var loaded = SaveAndReload(subtitle);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Two lines here" + Environment.NewLine + "second line.", loaded.Paragraphs[0].Text);
        Assert.Equal("Anna", loaded.Paragraphs[0].Actor);
        Assert.Equal("Quote \"inside\" text, and, commas.", loaded.Paragraphs[1].Text);
        Assert.Equal("Bob", loaded.Paragraphs[1].Actor);
    }
}
