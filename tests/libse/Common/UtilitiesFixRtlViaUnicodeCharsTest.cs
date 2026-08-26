using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class UtilitiesFixRtlViaUnicodeCharsTest
{
    private const string Rle = "‫"; // right-to-left embedding
    private const string Pdf = "‬"; // pop directional formatting

    private const string Arabic = "هذا صحيح.";

    [Fact]
    public void PlainLineIsEmbeddedAndClosed()
    {
        Assert.Equal(Rle + Arabic + Pdf, Utilities.FixRtlViaUnicodeChars(Arabic));
    }

    [Fact]
    public void EmbeddingStartsAfterALeadingAssaBlock()
    {
        // A U+202B in front of "{\i1}" moves the tag itself to the other end instead of the text
        // it marks up - the case reported in issue #14150.
        Assert.Equal("{\\i1}" + Rle + Arabic + Pdf, Utilities.FixRtlViaUnicodeChars("{\\i1}" + Arabic));
    }

    [Fact]
    public void EmbeddingStartsAfterEveryLeadingAssaBlock()
    {
        Assert.Equal(
            "{\\an8}{\\pos(10,20)}" + Rle + Arabic + Pdf,
            Utilities.FixRtlViaUnicodeChars("{\\an8}{\\pos(10,20)}" + Arabic));
    }

    [Fact]
    public void HtmlTagsStayOutsideTheEmbedding()
    {
        Assert.Equal("<i>" + Rle + Arabic + Pdf + "</i>", Utilities.FixRtlViaUnicodeChars("<i>" + Arabic + "</i>"));
    }

    [Fact]
    public void MarkupInsideTheLineIsLeftWhereItIs()
    {
        var text = "{\\i1}" + Arabic + "{\\i0} " + Arabic;
        Assert.Equal("{\\i1}" + Rle + Arabic + "{\\i0} " + Arabic + Pdf, Utilities.FixRtlViaUnicodeChars(text));
    }

    [Fact]
    public void EveryLineGetsItsOwnEmbedding()
    {
        var text = "{\\i1}" + Arabic + Environment.NewLine + Arabic;
        Assert.Equal(
            "{\\i1}" + Rle + Arabic + Pdf + Environment.NewLine + Rle + Arabic + Pdf,
            Utilities.FixRtlViaUnicodeChars(text));
    }

    [Fact]
    public void ALineOfNothingButMarkupIsLeftAlone()
    {
        Assert.Equal("{\\an8}", Utilities.FixRtlViaUnicodeChars("{\\an8}"));
    }

    [Theory]
    [InlineData("{\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}")]
    [InlineData("{\\an8\\p4}m 0 0 l 10 10")]
    public void ADrawingIsLeftAlone(string drawing)
    {
        // Coordinates are not a sentence, and a directional mark among the drawing commands is
        // something libass has to parse as one of them.
        Assert.Equal(drawing, Utilities.FixRtlViaUnicodeChars(drawing));
    }

    [Theory]
    [InlineData("{\\pos(10,20)}")]
    [InlineData("{\\pbo3}")]
    [InlineData("{\\fsp4}")]
    [InlineData("{\\p0}")]
    public void TagsThatOnlyLookLikeADrawingStillGetTheEmbedding(string tag)
    {
        Assert.Equal(tag + Rle + Arabic + Pdf, Utilities.FixRtlViaUnicodeChars(tag + Arabic));
    }

    [Fact]
    public void RunningItTwiceGivesTheSameText()
    {
        var once = Utilities.FixRtlViaUnicodeChars("{\\i1}" + Arabic);
        Assert.Equal(once, Utilities.FixRtlViaUnicodeChars(once));
    }

    [Fact]
    public void MarksLeftByTheOldOnePassVersionAreReplaced()
    {
        // Files fixed by an earlier Subtitle Edit carry an opening character in front of the tag
        // and no closing one.
        Assert.Equal("{\\i1}" + Rle + Arabic + Pdf, Utilities.FixRtlViaUnicodeChars(Rle + "{\\i1}" + Arabic));
    }

    [Fact]
    public void EmptyTextIsUnchanged()
    {
        Assert.Equal(string.Empty, Utilities.FixRtlViaUnicodeChars(string.Empty));
    }

    [Fact]
    public void AnUnclosedTagIsTreatedAsText()
    {
        Assert.Equal(Rle + "{not a block " + Arabic + Pdf, Utilities.FixRtlViaUnicodeChars("{not a block " + Arabic));
    }
}
