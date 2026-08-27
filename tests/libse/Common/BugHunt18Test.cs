using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;

namespace LibSETests.Common;

/// <summary>
/// Guard tests for the 2026-08-28 bug hunt (sweep 18): a tolerance probe that overrode an exact
/// match, an off-by-one that dropped the character before a bracket, and a guard testing a
/// lowercase letter the enclosing condition had already ruled out.
/// </summary>
public class BugHunt18Test
{
    [Fact]
    public void FirstOnOrAfter_ExactMatch_ReturnsItNotTheEarlierNeighbour()
    {
        // A cue snapped onto a shot change hits exactly. The tolerance probe used to run anyway
        // and returned the PREVIOUS entry - "the next shot change" came back 20 ms earlier.
        var shotChanges = new List<double> { 9.98, 10.0 };

        Assert.Equal(10.0, shotChanges.FirstOnOrAfter(10.0, 0.039, -1));
    }

    [Fact]
    public void FirstOnOrBefore_ExactMatch_ReturnsItNotTheLaterNeighbour()
    {
        var shotChanges = new List<double> { 10.0, 10.02 };

        Assert.Equal(10.0, shotChanges.FirstOnOrBefore(10.0, 0.039, -1));
    }

    [Fact]
    public void FirstOnOrAfter_NoExactMatch_StillUsesTheTolerance()
    {
        // The near-miss behaviour the tolerance exists for must survive the fix.
        var shotChanges = new List<double> { 9.98, 10.5 };

        Assert.Equal(9.98, shotChanges.FirstOnOrAfter(10.0, 0.039, -1));
    }

    [Fact]
    public void SplitWord_UppercaseAStart_IsSplit()
    {
        // The guard tested lowercase 'a', which the enclosing "starts with its own uppercase"
        // condition had already excluded - so the exception never applied and "Acat" came back
        // unsplit while the 'I' sibling ("Iam") worked.
        var words = new[] { "a", "A", "I", "am", "cat" };

        Assert.Equal("A cat", StringWithoutSpaceSplitToWords.SplitWord(words, "Acat", "eng"));
    }

    [Fact]
    public void SplitWord_UppercaseIStart_StillSplit()
    {
        var words = new[] { "a", "A", "I", "am", "cat" };

        Assert.Equal("I am", StringWithoutSpaceSplitToWords.SplitWord(words, "Iam", "eng"));
    }
}
