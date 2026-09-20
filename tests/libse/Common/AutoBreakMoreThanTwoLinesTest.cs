using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class AutoBreakMoreThanTwoLinesTest
{
    private static List<string> Break(string text, int maximumLength)
    {
        return Utilities.AutoBreakLineMoreThanTwoLines(text, maximumLength, 25, "en").SplitToLines();
    }

    [Fact]
    public void LinesAreBalanced_NotLeftOverInTheLastLine()
    {
        // Filling line by line gave 26/26/28/28/32.
        var lines = Break("We hold these truths to be self-evident, that all men are created equal, that they are endowed by their Creator with certain unalienable Rights.", 37);

        Assert.Equal(new List<string>
        {
            "We hold these truths to be",
            "self-evident, that all men are",
            "created equal, that they are",
            "endowed by their Creator with",
            "certain unalienable Rights.",
        }, lines);
    }

    [Fact]
    public void UsesTheFewestLinesThatFit()
    {
        // 118 characters fit in four lines of 30 - this used to take five.
        var lines = Break("The quick brown fox jumps over the lazy dog while the farmer sleeps quietly under the old oak tree near the river bank.", 30);

        Assert.Equal(4, lines.Count);
        Assert.All(lines, line => Assert.True(line.Length <= 30));
    }

    [Fact]
    public void ShortTextOverTheMaximum_TakesTwoLinesNotThree()
    {
        // Under "merge lines shorter than", so the two line break leaves it alone - was "Short/line/that fits."
        var lines = Break("Short line that fits.", 20);

        Assert.Equal(new List<string> { "Short line", "that fits." }, lines);
    }

    [Fact]
    public void WordLongerThanMaximum_GetsItsOwnLine_AndTheRestIsStillBroken()
    {
        // One unbreakable word used to make every attempt fail and the text come back unbroken.
        var lines = Break("Supercalifragilisticexpialidocious is a word that Mr. Banks never wanted to hear in his house again, not even once, not ever.", 30);

        Assert.Equal("Supercalifragilisticexpialidocious", lines[0]);
        Assert.All(lines.Skip(1), line => Assert.True(line.Length <= 30));
        Assert.DoesNotContain(lines, line => line.EndsWith("Mr."));
    }

    [Fact]
    public void FewerWordsThanTheLengthAsksFor_OneLinePerWord()
    {
        // 80 characters at max 20 asks for four lines, but there are only two words.
        var lines = Break("Pneumonoultramicroscopicsilicovolcanoconiosis Supercalifragilisticexpialidocious", 20);

        Assert.Equal(new List<string> { "Pneumonoultramicroscopicsilicovolcanoconiosis", "Supercalifragilisticexpialidocious" }, lines);
    }

    [Fact]
    public void PrefersBreakingAfterPunctuation()
    {
        // Balance alone gives "It was the best of times, it" / "was the worst of times, it" / ...
        var lines = Break("It was the best of times, it was the worst of times, it was the age of wisdom, it was the age of foolishness.", 37);

        Assert.Equal(new List<string>
        {
            "It was the best of times,",
            "it was the worst of times,",
            "it was the age of wisdom,",
            "it was the age of foolishness.",
        }, lines);
    }

    [Fact]
    public void DialogDashStartsALine()
    {
        var lines = Break("- Did you see Mr. Smith at the station yesterday? - No, I was with Dr. Jones the whole afternoon at the clinic.", 37);

        Assert.Equal(4, lines.Count);
        Assert.Contains(lines, line => line.StartsWith("- No, I was"));
        Assert.DoesNotContain(lines, line => line.EndsWith("Mr.") || line.EndsWith("Dr.") || line.EndsWith("-"));
        Assert.All(lines, line => Assert.True(line.Length <= 37));
    }

    [Fact]
    public void NoEmptyLines()
    {
        var lines = Break("Supercalifragilisticexpialidocious is a word that Mr. Banks never wanted to hear in his house again, not even once, not ever.", 37);

        Assert.DoesNotContain(lines, string.IsNullOrWhiteSpace);
    }

    [Fact]
    public void HtmlTagsStayOnTheirWords()
    {
        var lines = Break("<i>The quick brown fox jumps over the lazy dog while the farmer sleeps quietly</i> under the old oak tree near the river bank.", 30);

        Assert.StartsWith("<i>The quick", lines[0]);
        Assert.Contains(lines, line => line.StartsWith("sleeps quietly</i> under"));
        Assert.All(lines, line => Assert.True(HtmlUtil.RemoveHtmlTags(line, true).Length <= 30));
    }
}
