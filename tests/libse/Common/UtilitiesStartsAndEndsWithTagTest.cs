using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class UtilitiesStartsAndEndsWithTagTest
{
    // Cases the old hard-coded prefix/suffix list already accepted - must stay true.
    [Theory]
    [InlineData("<i>Hello there</i>")]
    [InlineData("- <i>Hello there</i>")]
    [InlineData("-<i>Hello there</i>")]
    [InlineData("- ...<i>Hello there</i>")]
    [InlineData("<i>Hello there</i>.")]
    [InlineData("<i>Hello there</i>!")]
    [InlineData("<i>Hello there</i>?")]
    [InlineData("<i>Hello there</i>...")]
    [InlineData("<i>Hello there</i>-")]
    [InlineData("- <i>Hello there</i>...")]
    public void MatchesLikeOldImplementation(string text)
    {
        Assert.True(Utilities.StartsAndEndsWithTag(text, "<i>", "</i>"));
    }

    // Variants the character-skip approach additionally accepts (leading dots/spaces without
    // a dash, runs of trailing punctuation) - pinned deliberately: merging italics across
    // these prefixes/suffixes is the desired behavior in FixInvalidItalicTags.
    [Theory]
    [InlineData("...<i>I was going</i>")]
    [InlineData(". <i>Hello there</i>")]
    [InlineData(" <i>Hello there</i>")]
    [InlineData("<i>Hello there</i>?!")]
    [InlineData("<i>Hello there</i>!!")]
    [InlineData("<i>Hello there</i> -")]
    [InlineData("<i>Hello there</i> ")]
    [InlineData("-- <i>Hello there</i>")]
    public void MatchesNewlyAcceptedVariants(string text)
    {
        Assert.True(Utilities.StartsAndEndsWithTag(text, "<i>", "</i>"));
    }

    [Theory]
    [InlineData("He said <i>hello</i> to me.")]
    [InlineData("Hello <i>there</i>")]
    [InlineData("<i>Hello there")]
    [InlineData("Hello there</i>")]
    [InlineData("Hello there")]
    [InlineData("")]
    [InlineData("   ")]
    public void DoesNotMatch(string text)
    {
        Assert.False(Utilities.StartsAndEndsWithTag(text, "<i>", "</i>"));
    }

    [Fact]
    public void MatchesFontTag()
    {
        Assert.True(Utilities.StartsAndEndsWithTag("- <font color=\"red\">Hello</font>...", "<font", "</font>"));
    }

    [Fact]
    public void StartTagOnlyOverloadStillWorks()
    {
        // HtmlUtil.FixInvalidItalicTags calls this with startTag == endTag for dangling-tag lines.
        Assert.True(Utilities.StartsAndEndsWithTag("<i>", "<i>", "<i>"));
        Assert.False(Utilities.StartsAndEndsWithTag("<i>Hello", "<i>", "</i>"));
    }
}
