using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

/// <summary>
/// Documents the ToParagraph/ctor round-trip invariant behind the ASSA style-loss family
/// (PRs #13352/#13353 and the waveform guess-time-codes fix): ToParagraph only fills
/// Paragraph.Extra when it knows the format, the ASSA writer reads the style column from
/// Extra, and the format-aware ctor reads the style back from Extra. Any round trip through
/// a bare ToParagraph() therefore wipes the style as soon as an ASSA format re-enters.
/// </summary>
public class SubtitleLineViewModelStyleRoundTripTests
{
    private static readonly AdvancedSubStationAlpha Assa = new();

    private static SubtitleLineViewModel MakeStyledLine()
    {
        return new SubtitleLineViewModel(new Paragraph("Hello", 0, 1000) { Extra = "Big" }, Assa);
    }

    [Fact]
    public void FormatAwareRoundTrip_KeepsStyle()
    {
        var line = MakeStyledLine();
        Assert.Equal("Big", line.Style);

        var roundTripped = new SubtitleLineViewModel(line.ToParagraph(Assa), Assa);

        Assert.Equal("Big", roundTripped.Style);
    }

    [Fact]
    public void BareToParagraph_LeavesExtraEmpty()
    {
        var line = MakeStyledLine();

        var p = line.ToParagraph();

        // This is the trap: the ASSA writer and the format-aware ctor both read Extra, so a
        // bare ToParagraph drops the style even though Paragraph.Style still carries it.
        Assert.True(string.IsNullOrEmpty(p.Extra));
        Assert.Equal("Big", p.Style);
    }
}
