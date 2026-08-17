using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;

namespace UITests.Features.Video.TextToSpeech.AutoCast;

/// <summary>
/// Which of a speaker's lines their voice is cloned from. Clone quality follows this choice more
/// than anything else in the pipeline.
/// </summary>
public class SpeakerReferenceBuilderTests
{
    private static List<Paragraph> Lines(params (double Start, double End)[] times) =>
        times.Select(t => new Paragraph("text", t.Start * 1000, t.End * 1000)).ToList();

    [Fact]
    public void TheLongestLinesAreUsedFirst()
    {
        var lines = Lines((0, 1.5), (10, 20), (30, 35));

        var picked = SpeakerReferenceBuilder.PickReferenceLines(lines);

        // 10 s + 5 s already passes the target, so the 1.5 s line is not needed.
        Assert.Equal(2, picked.Count);
        Assert.DoesNotContain(picked, p => Math.Abs(p.Duration.TotalSeconds - 1.5) < 0.01);
    }

    [Fact]
    public void ThePickedLinesComeBackInTimeOrder()
    {
        // The joined audio is cut in this order and the transcript is written in this order; if
        // they disagree, every engine that uses ref-text clones against the wrong words.
        var lines = Lines((30, 40), (10, 14), (50, 56));

        var picked = SpeakerReferenceBuilder.PickReferenceLines(lines);

        Assert.Equal(picked.OrderBy(p => p.StartTime.TotalMilliseconds).ToList(), picked);
    }

    [Fact]
    public void EnoughAudioStopsTheSearch()
    {
        var lines = Lines((0, 20), (30, 50), (60, 80));

        var picked = SpeakerReferenceBuilder.PickReferenceLines(lines);

        Assert.Single(picked);
    }

    [Fact]
    public void AChatteringSpeakerIsCappedAtMaxParts()
    {
        // Forty one-second lines would otherwise mean forty ffmpeg cuts and a stitched clip that
        // is mostly line beginnings.
        var lines = Lines(Enumerable.Range(0, 40).Select(i => (i * 2.0, i * 2.0 + 1.05)).ToArray());

        var picked = SpeakerReferenceBuilder.PickReferenceLines(lines);

        Assert.True(picked.Count <= SpeakerReferenceBuilder.MaxParts, $"picked {picked.Count} parts");
    }

    [Fact]
    public void VeryShortLinesAreSkipped()
    {
        var lines = Lines((0, 0.4), (5, 0.4 + 5), (10, 14));

        var picked = SpeakerReferenceBuilder.PickReferenceLines(lines);

        Assert.Single(picked);
        Assert.Equal(4, picked[0].Duration.TotalSeconds, 1);
    }

    [Fact]
    public void ASpeakerWithOnlyShortLinesStillGetsAReference()
    {
        // A poor reference clones something; no reference clones nothing, and this speaker would
        // otherwise silently drop out of the cast.
        var lines = Lines((0, 0.4), (5, 5.6), (10, 10.3));

        var picked = SpeakerReferenceBuilder.PickReferenceLines(lines);

        Assert.Single(picked);
        Assert.Equal(0.6, picked[0].Duration.TotalSeconds, 1);
    }

    [Fact]
    public void LinesWithNoTextAreNotUsed()
    {
        // An empty line has no words for the reference transcript, so it only adds silence.
        var lines = Lines((0, 5), (10, 20));
        lines[1].Text = "   ";

        var picked = SpeakerReferenceBuilder.PickReferenceLines(lines);

        Assert.Single(picked);
        Assert.Equal(5, picked[0].Duration.TotalSeconds, 1);
    }
}
