using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// macOS "Look up" (#14277). The text comes straight out of the subtitle text box, so it can be a
/// two-line selection, it can hold anything a subtitle holds, and the menu header must stay short.
/// </summary>
public class MacDictionaryLookupTests
{
    [Fact]
    public void BuildsADictUrlForTheWord()
    {
        Assert.Equal("dict://%C3%A9vanoui", MacDictionaryLookup.BuildUrl("évanoui"));
    }

    [Fact]
    public void LooksUpATwoLineSelectionAsOnePhrase()
    {
        Assert.Equal("dict://in%20the%20dark", MacDictionaryLookup.BuildUrl("in the\r\ndark"));
    }

    [Fact]
    public void FoldsTabsAndRepeatedSpaces()
    {
        Assert.Equal("hi there", MacDictionaryLookup.Normalize("  hi \t  there  "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    public void HasNothingToLookUp(string? text)
    {
        Assert.Null(MacDictionaryLookup.Normalize(text));
        Assert.Null(MacDictionaryLookup.BuildUrl(text));
    }

    [Fact]
    public void PutsTheWordInTheMenuHeader()
    {
        Assert.Equal("Look up \"évanoui\"", MacDictionaryLookup.BuildHeader("Look up \"{0}\"", "évanoui"));
    }

    [Fact]
    public void ElidesALongSelectionInTheMenuHeader()
    {
        var header = MacDictionaryLookup.BuildHeader("Look up \"{0}\"", new string('a', 100));

        Assert.Equal("Look up \"" + new string('a', MacDictionaryLookup.MaxHeaderTextLength - 1) + "…\"", header);
    }

    [Fact]
    public void DoesNotThrowOnAStrayBraceInTheTranslation()
    {
        var header = MacDictionaryLookup.BuildHeader("Slå op \"{0}\" {1", "ord");

        Assert.Equal("Slå op \"ord\" {1", header);
    }
}
