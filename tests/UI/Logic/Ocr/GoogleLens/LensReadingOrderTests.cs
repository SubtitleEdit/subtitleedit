using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

namespace UITests.Logic.Ocr.GoogleLens;

// #12149: Google Lens returns the text chunks of a multi-line subtitle interleaved between the two
// visual rows, scrambling the sentence (e.g. "maar hebben toen ze die Koran ze hem geëxecuteerd.
// Vonden,"). LensCore.OrderSegmentsIntoReadingLines uses each chunk's bounding box to restore
// reading order. The boxes below are the real values captured from the Lens response for the
// reporter's sample image (image1024.png, 849x148).
public class LensReadingOrderTests
{
    private static Segment Seg(string text, double cx, double cy, double w, double h)
        => new(text, new BoundingBox(new[] { cx, cy, w, h }, new[] { 849, 148 }));

    [Fact]
    public void OrderSegments_InterleavedTwoLineSubtitle_RestoresReadingOrder()
    {
        // Exactly the order and geometry Google returned: top/bottom/top/bottom/top/bottom.
        var scrambled = new List<Segment>
        {
            Seg("maar", 0.0789, 0.1960, 0.1578, 0.3784),
            Seg("hebben", 0.1372, 0.7972, 0.2297, 0.3919),
            Seg("toen ze die", 0.3498, 0.1960, 0.3463, 0.3784),
            Seg("ze hem", 0.3899, 0.7938, 0.2285, 0.3851),
            Seg("Koran vonden,", 0.7715, 0.1960, 0.4499, 0.3784),
            Seg("geëxecuteerd.", 0.7479, 0.7938, 0.4405, 0.3851),
        };

        var ordered = LensCore.OrderSegmentsIntoReadingLines(scrambled);

        Assert.Equal(2, ordered.Count);
        Assert.Equal("maar toen ze die Koran vonden,", ordered[0].Text);
        Assert.Equal("hebben ze hem geëxecuteerd.", ordered[1].Text);
    }

    [Fact]
    public void OrderSegments_AlreadyInOrder_IsUnchanged()
    {
        var segments = new List<Segment>
        {
            Seg("Line one here", 0.5, 0.25, 0.6, 0.3),
            Seg("Line two here", 0.5, 0.75, 0.6, 0.3),
        };

        var ordered = LensCore.OrderSegmentsIntoReadingLines(segments);

        Assert.Equal(new[] { "Line one here", "Line two here" }, ordered.Select(s => s.Text));
    }

    [Fact]
    public void OrderSegments_SingleSegment_IsReturnedAsIs()
    {
        var segments = new List<Segment> { Seg("Just one line", 0.5, 0.5, 0.8, 0.3) };

        var ordered = LensCore.OrderSegmentsIntoReadingLines(segments);

        Assert.Single(ordered);
        Assert.Equal("Just one line", ordered[0].Text);
    }

    [Fact]
    public void OrderSegments_DefaultBoxes_KeepsSegmentsAndOrderUnchanged()
    {
        // When geometry is missing every chunk gets the same default centre box (0.5, 0.5); with
        // nothing to sort by, the segments (and their line breaks) must be left exactly as-is - not
        // shuffled and not merged into a single line.
        var segments = new List<Segment>
        {
            Seg("alpha", 0.5, 0.5, 1, 1),
            Seg("beta", 0.5, 0.5, 1, 1),
            Seg("gamma", 0.5, 0.5, 1, 1),
        };

        var ordered = LensCore.OrderSegmentsIntoReadingLines(segments);

        Assert.Equal(new[] { "alpha", "beta", "gamma" }, ordered.Select(s => s.Text));
    }
}
