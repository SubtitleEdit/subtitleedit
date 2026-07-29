using Nikse.SubtitleEdit.Features.Files.ImportPlainText;

namespace UITests.Features.Files.ImportPlainText;

public class ForcedAlignPlannerTests
{
    private static readonly ForcedAlignPlanner.Options Defaults = new();

    private static List<string> Lines(int count, int length = 40)
        => Enumerable.Range(0, count).Select(_ => new string('x', length)).ToList();

    [Fact]
    public void ChunkSize_LeavesTheWindowMostlyEmpty()
    {
        // The whole approach depends on the window holding far more audio than the chunk's
        // text needs. 40-char lines at 15 chars/second are ~2.7 s each; a 120 s window
        // takes 35% of that in text, so ~15 lines' worth - capped to 12 by line count.
        var take = ForcedAlignPlanner.ChunkSize(Lines(500), 120, 15, Defaults);

        Assert.Equal(12, take);
    }

    [Fact]
    public void ChunkSize_CapsOnReadingTimeForLongLines()
    {
        // Long lines hit the reading-time budget before the line count, which is what keeps
        // the slack that lets the aligner skip audio the script does not cover.
        var take = ForcedAlignPlanner.ChunkSize(Lines(500, 400), 120, 15, Defaults);

        Assert.InRange(take, 1, 2);
    }

    [Fact]
    public void ChunkSize_AlwaysTakesAtLeastOneLine()
    {
        Assert.Equal(1, ForcedAlignPlanner.ChunkSize(Lines(3, 10000), 10, 15, Defaults));
        Assert.Equal(0, ForcedAlignPlanner.ChunkSize(new List<string>(), 120, 15, Defaults));
    }

    [Fact]
    public void AcceptChunk_StopsWhenTheAlignerStartsFillingSpace()
    {
        // Measured shape: the first cues track real speech back to back, then one opens
        // far after the previous closed and the rest run away down the window.
        var reading = new List<double> { 3, 3, 3, 3, 3 };
        var cues = new List<ForcedAlignPlanner.Cue>
        {
            new(0, 2.8),
            new(3.0, 5.9),
            new(6.1, 8.9),
            new(63.0, 65.9),   // 54 s gap - the aligner has stopped tracking
            new(66.1, 69.0),
        };

        Assert.Equal(3, ForcedAlignPlanner.AcceptChunk(cues, reading));
    }

    [Fact]
    public void AcceptChunk_StopsOnACueFarLongerThanItsText()
    {
        var reading = new List<double> { 3, 3, 3 };
        var cues = new List<ForcedAlignPlanner.Cue>
        {
            new(0, 2.8),
            new(3.0, 5.9),
            new(6.1, 30.0),   // 24 s for 3 s of text
        };

        Assert.Equal(2, ForcedAlignPlanner.AcceptChunk(cues, reading));
    }

    [Fact]
    public void AcceptChunk_ToleratesTheFirstCueAbsorbingLeadingAudio()
    {
        // The first cue soaks up whatever leading audio the script says nothing about, so
        // its length proves nothing and must not veto the whole chunk.
        var reading = new List<double> { 3, 3, 3 };
        var cues = new List<ForcedAlignPlanner.Cue>
        {
            new(0, 110.0),      // 110 s for 3 s of text - expected, not a failure
            new(110.2, 113.0),
            new(113.2, 116.0),
        };

        Assert.Equal(3, ForcedAlignPlanner.AcceptChunk(cues, reading));
    }

    [Fact]
    public void AcceptChunk_AcceptsACleanChunkWhole()
    {
        var reading = new List<double> { 3, 3, 3, 3 };
        var cues = new List<ForcedAlignPlanner.Cue>
        {
            new(0, 2.8), new(3.0, 5.9), new(6.1, 8.9), new(9.1, 11.9),
        };

        Assert.Equal(4, ForcedAlignPlanner.AcceptChunk(cues, reading));
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
