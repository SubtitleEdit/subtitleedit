using Nikse.SubtitleEdit.Features.Files.ImportPlainText;

namespace UITests.Features.Files.ImportPlainText;

public class ForcedAlignPlannerTests
{
    private static readonly ForcedAlignPlanner.Options Defaults = new();

    private static List<string> Lines(int count, int length = 40)
        => Enumerable.Range(0, count).Select(_ => new string('x', length)).ToList();

    [Fact]
    public void LinesForWindow_OvershootsOnlyModestly()
    {
        // 40 chars per line, 10 chars/second => 4 seconds of speech per line.
        // A 240 s window holds 60 lines; at 1.15x overshoot we feed 69.
        var take = ForcedAlignPlanner.LinesForWindow(Lines(500), 240, 10, Defaults, isLastWindow: false);

        Assert.InRange(take, 66, 72);
    }

    [Fact]
    public void LinesForWindow_LastWindowTakesEverythingLeft()
    {
        // Nothing may be dropped off the end just because the estimate says it won't fit.
        var take = ForcedAlignPlanner.LinesForWindow(Lines(500), 240, 10, Defaults, isLastWindow: true);

        Assert.Equal(500, take);
    }

    [Fact]
    public void LinesForWindow_AlwaysTakesAtLeastOneLine()
    {
        // A very long line against a tiny window must not plan zero work forever.
        var take = ForcedAlignPlanner.LinesForWindow(Lines(3, 10000), 1, 10, Defaults, isLastWindow: false);

        Assert.Equal(1, take);
    }

    [Fact]
    public void AcceptCount_DiscardsCuesNearTheWindowEdge()
    {
        // The overshoot text is crammed into the tail, so cues there are not trustworthy.
        var cues = new List<ForcedAlignPlanner.Cue>
        {
            new(0, 50),
            new(50, 100),
            new(100, 150),   // 150 <= 180 (75% of 240) - kept
            new(150, 200),   // past the accept limit - dropped
            new(200, 239),
        };

        var accepted = ForcedAlignPlanner.AcceptCount(cues, 240, Defaults, isLastWindow: false);

        Assert.Equal(3, accepted);
    }

    [Fact]
    public void AcceptCount_LastWindowKeepsEverything()
    {
        var cues = new List<ForcedAlignPlanner.Cue> { new(0, 100), new(100, 230), new(230, 239) };

        Assert.Equal(3, ForcedAlignPlanner.AcceptCount(cues, 240, Defaults, isLastWindow: true));
    }

    [Fact]
    public void AcceptCount_NeverReturnsZeroWhenCuesExist()
    {
        // Every cue sits past the accept limit. Returning 0 would re-plan the same window
        // forever, so some progress has to be forced.
        var cues = new List<ForcedAlignPlanner.Cue> { new(200, 210), new(210, 220), new(220, 230) };

        var accepted = ForcedAlignPlanner.AcceptCount(cues, 240, Defaults, isLastWindow: false);

        Assert.True(accepted >= 1);
    }

    [Fact]
    public void AcceptCount_RespectsTailGuardOnShortWindows()
    {
        // With a 4 s window the guard (1 s) binds before the 75% fraction (3 s).
        var options = new ForcedAlignPlanner.Options { AcceptFraction = 0.95, TailGuardSeconds = 1.0 };
        var cues = new List<ForcedAlignPlanner.Cue> { new(0, 2.9), new(2.9, 3.5) };

        Assert.Equal(1, ForcedAlignPlanner.AcceptCount(cues, 4, options, isLastWindow: false));
    }

    [Fact]
    public void TrimImplausible_CutsAtASustainedRunOfSqueezedCues()
    {
        // Ratios measured against ground truth past a 90 s passage missing from the
        // script: correctly placed lines ran at 0.84-0.89 of reading time, lines crammed
        // onto the missing passage's audio at 0.36-0.48.
        var reading = new List<double> { 5, 5, 5, 5, 5, 5 };
        var cues = new List<ForcedAlignPlanner.Cue>
        {
            new(0, 4.3),      // 0.86
            new(4.3, 8.6),    // 0.86
            new(8.6, 12.8),   // 0.84
            new(12.8, 14.8),  // 0.40 - squeezed
            new(14.8, 16.7),  // 0.38 - squeezed
            new(16.7, 18.5),  // 0.36 - squeezed
        };

        Assert.Equal(3, ForcedAlignPlanner.TrimImplausible(cues, reading, 6));
    }

    [Fact]
    public void TrimImplausible_IgnoresAnIsolatedShortCue()
    {
        // One quickly-delivered line is not evidence that the script has drifted off the
        // audio; only a sustained run is.
        var reading = new List<double> { 5, 5, 5, 5 };
        var cues = new List<ForcedAlignPlanner.Cue>
        {
            new(0, 4.3),
            new(4.3, 6.3),    // 0.40 - one short line
            new(6.3, 10.6),
            new(10.6, 14.9),
        };

        Assert.Equal(4, ForcedAlignPlanner.TrimImplausible(cues, reading, 4));
    }

    [Fact]
    public void TrimImplausible_LeavesAWellAlignedWindowAlone()
    {
        var reading = new List<double> { 4, 4, 4 };
        var cues = new List<ForcedAlignPlanner.Cue> { new(0, 3.6), new(3.6, 7.2), new(7.2, 10.8) };

        Assert.Equal(3, ForcedAlignPlanner.TrimImplausible(cues, reading, 3));
    }

    [Fact]
    public void TrimImplausible_CanRejectAWholeWindow()
    {
        // Everything squeezed from the start - the caller steps the audio cursor on
        // instead of consuming any script.
        var reading = new List<double> { 5, 5, 5 };
        var cues = new List<ForcedAlignPlanner.Cue> { new(0, 1.8), new(1.8, 3.5), new(3.5, 5.2) };

        Assert.Equal(0, ForcedAlignPlanner.TrimImplausible(cues, reading, 3));
    }

    [Fact]
    public void ParseCues_ReadsAlignerSrtOutput()
    {
        var srt = "1\n00:00:00,160 --> 00:00:03,360\nThe engineer repaired the radio\n\n"
                + "2\n00:00:03,520 --> 00:00:07,040\nA stubborn cat watched\n\n";

        var cues = ForcedAligner.ParseCues(srt);

        Assert.Equal(2, cues.Count);
        Assert.Equal(0.16, cues[0].StartSeconds, 3);
        Assert.Equal(3.36, cues[0].EndSeconds, 3);
        Assert.Equal(3.52, cues[1].StartSeconds, 3);
        Assert.Equal(7.04, cues[1].EndSeconds, 3);
    }

    [Fact]
    public void ParseCues_EmptyOutputIsNotAnError()
    {
        Assert.Empty(ForcedAligner.ParseCues(string.Empty));
        Assert.Empty(ForcedAligner.ParseCues("   "));
    }
}
