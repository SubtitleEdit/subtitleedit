using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class Cavena890Test
{
    private static Subtitle SaveAndReload(Subtitle subtitle, int languageId)
    {
        var oldLanguageId = Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId;
        Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = languageId;
        var path = Path.GetTempFileName();
        try
        {
            new Cavena890().Save(path, subtitle, batchMode: true);
            var loaded = new Subtitle();
            new Cavena890().LoadSubtitle(loaded, new List<string>(), path);
            return loaded;
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = oldLanguageId;
            File.Delete(path);
        }
    }

    // The Chinese text field is UTF-16BE. The writer used to emit only the high byte of
    // each character (and lone italic-marker bytes), so no Chinese text could round-trip.
    [Fact]
    public void ChineseTextRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("早上好", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph("你好世界" + Environment.NewLine + "第二行", 3000, 5000));

        var loaded = SaveAndReload(subtitle, Cavena890.LanguageIdChineseSimplified);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("早上好", loaded.Paragraphs[0].Text);
        Assert.Equal("你好世界" + Environment.NewLine + "第二行", loaded.Paragraphs[1].Text);
    }

    [Fact]
    public void ChineseItalicRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>你好世界</i>", 0, 2000));

        var loaded = SaveAndReload(subtitle, Cavena890.LanguageIdChineseSimplified);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("<i>你好世界</i>", loaded.Paragraphs[0].Text);
    }
}
