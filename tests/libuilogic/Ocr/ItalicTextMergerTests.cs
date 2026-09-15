using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// ItalicTextMerger turns the matched glyphs' Italic flags into italic tags, word by word.
/// Punctuation must not vote: a hyphen trained from italic text matches an upright hyphen with no
/// wrong pixels, so dialog dashes came out as "&lt;i&gt;-&lt;/i&gt;" and ". . ." as italic (#14886).
/// </summary>
public class ItalicTextMergerTests
{
    /// <summary>One glyph per character of <paramref name="text"/>; an 'i' in <paramref name="italicMask"/> marks that glyph italic.</summary>
    private static List<NOcrChar> Chars(string text, string italicMask)
    {
        Assert.Equal(text.Length, italicMask.Length);
        var list = new List<NOcrChar>(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            list.Add(new NOcrChar { Text = text[i].ToString(), Italic = italicMask[i] == 'i' });
        }

        return list;
    }

    [Fact]
    public void ItalicHyphenInUprightDialogLineIsNotTagged()
    {
        var chars = Chars("- Where are you?", "i...............");

        Assert.Equal("- Where are you?", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void UprightHyphenInItalicLineStaysInsideTag()
    {
        var chars = Chars("- Hello?", "..iiiiii");

        Assert.Equal("<i>- Hello?</i>", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void ItalicDotsBetweenUprightWordsAreNotTagged()
    {
        var chars = Chars("Link. . . Here", "......i.i.....");

        Assert.Equal("Link. . . Here", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void TrailingItalicPunctuationFollowsPreviousWord()
    {
        var chars = Chars("Rule one. . .", "..........i.i");

        Assert.Equal("Rule one. . .", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void PunctuationDoesNotFollowWordOnNextLine()
    {
        var chars = Chars("Wait ...\nthere", ".....iii.iiiii");

        Assert.Equal("Wait ...\n<i>there</i>", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void UnknownGlyphsDoNotOutvoteItalicLetters()
    {
        // "*" is an unmatched glyph and never italic.
        var chars = Chars("go wh***", "ii.ii...");

        Assert.Equal("<i>go wh***</i>", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void TiedWordFollowsUprightContext()
    {
        // "on" has one italic and one upright glyph.
        var chars = Chars("Go on home", "...i......");

        Assert.Equal("Go on home", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void TiedWordFollowsItalicContext()
    {
        var chars = Chars("Go on home", "iii.iiiiii");

        Assert.Equal("<i>Go on home</i>", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void MixedLineKeepsItalicWord()
    {
        var chars = Chars("Hello world", "......iiiii");

        Assert.Equal("Hello <i>world</i>", ItalicTextMerger.MergeWithItalicTags(chars));
    }

    [Fact]
    public void BinaryOcrMatchesUseSameRules()
    {
        var chars = new List<BinaryOcrMatcher.CompareMatch>
        {
            new("-", true, 0, null),
            new(" ", false, 0, null),
            new("H", false, 0, null),
            new("i", false, 0, null),
            new(".", true, 0, null),
        };

        Assert.Equal("- Hi.", ItalicTextMerger.MergeWithItalicTags(chars));
    }
}
