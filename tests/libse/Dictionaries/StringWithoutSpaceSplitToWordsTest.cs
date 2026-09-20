using Nikse.SubtitleEdit.Core.Dictionaries;

namespace LibSETests.Dictionaries;

public class StringWithoutSpaceSplitToWordsTest
{
    [Fact]
    public void DictionariesValidXml()
    {
        var words = "we the people of the united states in order to form a more perfect union establish justice in sure domestic tranquility provide for the common defence promote the general welfare and secure the blessings of liberty to ourselves and our posterity do ordain and establish this constitution for the united states of america".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).OrderByDescending(p => p.Length).ToArray();
        var input = "wethepeopleoftheunitedstatesinordertoformamoreperfectunionestablishjusticeinsuredomestictranquilityprovideforthecommondefencepromotethegeneralwelfareandsecuretheblessingsoflibertytoourselvesandourposteritydoordainandestablishthisconstitutionfortheunitedstatesofamerica";
        var result = StringWithoutSpaceSplitToWords.SplitWord(words, input, "eng");
        Assert.Equal("we the people of the united states in order to form a more perfect union establish justice in sure domestic tranquility provide for the common defence promote the general welfare and secure the blessings of liberty to ourselves and our posterity do ordain and establish this constitution for the united states of america", result);
    }

    [Fact]
    public void SplitWord_LongestWordBlocksOnlySplit_IsStillSplit()
    {
        // Taking the longest word first masks "brother" and "together", stranding an "e" after
        // each. Ignoring one of the two at a time (the old retry) still leaves the other trap.
        var words = new[] { "brother", "together", "there", "here", "bro", "get", "in", "to" }.OrderByDescending(p => p.Length).ToArray();

        Assert.Equal("bro there in to get here", StringWithoutSpaceSplitToWords.SplitWord(words, "brothereintogethere", "eng"));
    }

    [Fact]
    public void SplitWord_PrefersFewestWords()
    {
        var words = new[] { "some", "thing", "something", "is", "here", "he", "re" }.OrderByDescending(p => p.Length).ToArray();

        Assert.Equal("something is here", StringWithoutSpaceSplitToWords.SplitWord(words, "somethingishere", "eng"));
    }

    [Fact]
    public void SplitWord_NoFullSplit_ReturnsInput()
    {
        var words = new[] { "some", "thing" };

        Assert.Equal("somethingx", StringWithoutSpaceSplitToWords.SplitWord(words, "somethingx", "eng"));
    }

    [Fact]
    public void SplitWord_InputIsListWord_ReturnsInput()
    {
        var words = new[] { "starbase", "star", "base" };

        Assert.Equal("starbase", StringWithoutSpaceSplitToWords.SplitWord(words, "starbase", "eng"));
    }
}