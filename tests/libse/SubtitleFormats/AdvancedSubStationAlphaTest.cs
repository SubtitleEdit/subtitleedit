using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class AdvancedSubStationAlphaTest
{
    private const string StandardEventsFormatLine = "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";

    private static Subtitle MakeSubtitle(string header)
    {
        var subtitle = new Subtitle { Header = header };
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 2000));
        return subtitle;
    }

    private static string HeaderWithEventsFormat(string formatLine)
    {
        return @"[Script Info]
ScriptType: v4.00+

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1

[Events]
" + formatLine;
    }

    [Fact]
    public void ToText_HeaderWithStandardEventsFormatLine_DoesNotDuplicateIt()
    {
        var subtitle = MakeSubtitle(HeaderWithEventsFormat(StandardEventsFormatLine));

        var text = new AdvancedSubStationAlpha().ToText(subtitle, string.Empty);

        var formatCount = text.SplitToLines().Count(l => l.StartsWith("Format: Layer", System.StringComparison.Ordinal));
        Assert.Equal(1, formatCount);
    }

    [Fact]
    public void ToText_HeaderWithNonStandardEventsFormatLine_AppendsStandardLineAfterIt()
    {
        // An MKV CodecPrivate header is kept verbatim, so its events Format line can use a
        // nonstandard field order. Dialogue lines are always written in the standard order, so
        // the standard Format line must still be appended (last Format line wins when parsing) -
        // otherwise every parser maps the fields to the wrong comma slots.
        var subtitle = MakeSubtitle(HeaderWithEventsFormat("Format: Start, End, Style, Text"));

        var text = new AdvancedSubStationAlpha().ToText(subtitle, string.Empty);

        var lines = text.SplitToLines();
        var nonStandardIndex = lines.IndexOf("Format: Start, End, Style, Text");
        var standardIndex = lines.IndexOf(StandardEventsFormatLine);
        Assert.True(nonStandardIndex >= 0);
        Assert.True(standardIndex > nonStandardIndex);

        // And the output must round-trip: SE's own parser reads the text back correctly.
        var reloaded = new Subtitle();
        new AdvancedSubStationAlpha().LoadSubtitle(reloaded, lines, "test.ass");
        Assert.Single(reloaded.Paragraphs);
        Assert.Equal("Hello", reloaded.Paragraphs[0].Text);
    }
}
