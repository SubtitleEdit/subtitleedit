using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// "Selection to Sentence case" (#13093) - the "Normal casing" fix applied to a text box
/// selection instead of whole lines.
/// </summary>
public class SentenceCaserTests
{
    [Fact]
    public void SentenceCaseUppercaseSelection()
    {
        var res = SentenceCaser.SentenceCase(string.Empty, "HOW ARE YOU?", "en");
        Assert.Equal("How are you?", res);
    }

    [Fact]
    public void SentenceCaseKeepsSentenceStartsInsideSelection()
    {
        var res = SentenceCaser.SentenceCase(string.Empty, "HOW ARE YOU? I AM FINE.", "en");
        Assert.Equal("How are you? I am fine.", res);
    }

    [Fact]
    public void SentenceCaseDoesNotCapitalizeMidSentenceSelection()
    {
        var res = SentenceCaser.SentenceCase("How ", "ARE YOU?", "en");
        Assert.Equal("are you?", res);
    }

    [Fact]
    public void SentenceCaseCapitalizesAfterSentenceEnd()
    {
        var res = SentenceCaser.SentenceCase("I am fine. ", "HOW ARE YOU?", "en");
        Assert.Equal("How are you?", res);
    }

    [Fact]
    public void SentenceCaseKeepsSurroundingWhiteSpace()
    {
        var res = SentenceCaser.SentenceCase(string.Empty, " HOW ARE YOU? ", "en");
        Assert.Equal(" How are you? ", res);
    }

    [Fact]
    public void SentenceCaseKeepsTags()
    {
        var res = SentenceCaser.SentenceCase(string.Empty, "<i>HOW ARE YOU?</i>", "en");
        Assert.Equal("<i>How are you?</i>", res);
    }

    [Fact]
    public void SentenceCaseWhiteSpaceOnlySelectionIsUnchanged()
    {
        var res = SentenceCaser.SentenceCase(string.Empty, "   ", "en");
        Assert.Equal("   ", res);
    }
}
