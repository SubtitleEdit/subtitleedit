using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// The SMPTE preview stretch used to be two copies of the same loop in the mpv and VLC
/// reloaders, and the secondary subtitle got none of it, so it drifted 0.1 % against the
/// main subtitle in SMPTE mode. One helper now serves all three.
/// </summary>
public class SmptePreviewStretchTests
{
    [Fact]
    public void Apply_StretchesStartAndEndOfEveryParagraph()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("a", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("b", 10000, 20000));

        SmptePreviewStretch.Apply(subtitle);

        Assert.Equal(1001, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(2002, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal(10010, subtitle.Paragraphs[1].StartTime.TotalMilliseconds, 3);
        Assert.Equal(20020, subtitle.Paragraphs[1].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void Stretched_LeavesTheSourceParagraphUntouched()
    {
        var source = new Paragraph("shared", 1000, 2000) { Extra = "Secondary" };

        var copy = SmptePreviewStretch.Stretched(source);

        Assert.NotSame(source, copy);
        Assert.Equal(1000, source.StartTime.TotalMilliseconds);
        Assert.Equal(2000, source.EndTime.TotalMilliseconds);
        Assert.Equal(1001, copy.StartTime.TotalMilliseconds, 3);
        Assert.Equal(2002, copy.EndTime.TotalMilliseconds, 3);
        Assert.Equal("Secondary", copy.Extra);
        Assert.Equal("shared", copy.Text);
    }

    [Fact]
    public void AddSecondarySubtitle_InSmpteMode_StretchesACopyAndKeepsTheSharedSecondaryIntact()
    {
        var main = new Subtitle();
        var secondary = new Subtitle();
        secondary.Paragraphs.Add(new Paragraph("second", 5000, 7000));

        SecondarySubtitleMerger.AddSecondarySubtitle(main, secondary, smpteMode: true);

        var added = Assert.Single(main.Paragraphs);
        Assert.NotSame(secondary.Paragraphs[0], added);
        Assert.Equal(5005, added.StartTime.TotalMilliseconds, 3);
        Assert.Equal(7007, added.EndTime.TotalMilliseconds, 3);
        Assert.Equal(5000, secondary.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(7000, secondary.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void AddSecondarySubtitle_WithoutSmpte_AddsTheParagraphsByReference()
    {
        var main = new Subtitle();
        var secondary = new Subtitle();
        secondary.Paragraphs.Add(new Paragraph("second", 5000, 7000));

        SecondarySubtitleMerger.AddSecondarySubtitle(main, secondary, smpteMode: false);

        var added = Assert.Single(main.Paragraphs);
        Assert.Same(secondary.Paragraphs[0], added);
    }
}
