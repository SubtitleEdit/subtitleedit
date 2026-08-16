using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Round-trip regression tests from the modern broadcast formats sweep (IMSC 1.1 family).
/// </summary>
public class ModernBroadcastFormatsSweepTest
{
    private static Subtitle SaveAndReload(SubtitleFormat format, Subtitle subtitle)
    {
        var text = format.ToText(subtitle, "test");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), null);
        return loaded;
    }

    // Literal angle brackets made the paragraph XML parse fail, and the fallback stripped
    // every '<' and '>' from the text ("3 < 5 > 2" became "3  5  2").
    [Fact]
    public void Imsc11KeepsLiteralAngleBrackets()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Math: 3 < 5 > 2.", 1000, 3000));

        var loaded = SaveAndReload(new TimedTextImsc11(), subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Math: 3 < 5 > 2.", loaded.Paragraphs[0].Text);
    }

    // The italic style carried tts:fontFamily="default", which the reader surfaced as a
    // spurious <font face="default"> tag around every italic run.
    [Fact]
    public void Imsc11ItalicRoundTripsWithoutFontTag()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Partial <i>italic word</i> here.", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("<i>Whole line italic.</i>", 4000, 6000));

        var loaded = SaveAndReload(new TimedTextImsc11(), subtitle);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Partial <i>italic word</i> here.", loaded.Paragraphs[0].Text);
        Assert.Equal("<i>Whole line italic.</i>", loaded.Paragraphs[1].Text);
    }

    // Italics were expressed only via tts:shear, invisible to spec-compliant readers -
    // the italic style must carry tts:fontStyle="italic".
    [Fact]
    public void Imsc11ItalicStyleUsesFontStyle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>Italic.</i>", 1000, 3000));

        var text = new TimedTextImsc11().ToText(subtitle, "test");

        Assert.Contains("tts:fontStyle=\"italic\"", text);
    }

    // Rosetta parses each line as its own XML fragment; an <i> spanning the line break
    // made the parse fail and the markup ended up as literal "i" text.
    [Fact]
    public void RosettaTwoLineItalicRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>Two italic lines" + Environment.NewLine + "both of them.</i>", 1000, 3000));

        var loaded = SaveAndReload(new TimedTextImscRosetta(), subtitle);

        Assert.Single(loaded.Paragraphs);
        var normalized = loaded.Paragraphs[0].Text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
        Assert.Equal("<i>Two italic lines" + Environment.NewLine + "both of them.</i>", normalized);
    }

    [Fact]
    public void RosettaKeepsLiteralAngleBrackets()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Math: 3 < 5 > 2.", 1000, 3000));

        var loaded = SaveAndReload(new TimedTextImscRosetta(), subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Math: 3 < 5 > 2.", loaded.Paragraphs[0].Text);
    }
}
