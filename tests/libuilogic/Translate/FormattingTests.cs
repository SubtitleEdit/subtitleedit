using Nikse.SubtitleEdit.UiLogic.Translate;

namespace LibUiLogicTests.Translate;

/// <summary>
/// Formatting takes tags off a line before it is sent to a translation engine and puts them back
/// around the answer. Leading ASSA override blocks were always handled; trailing ones were not,
/// so they travelled to the engine and came back "normalized" (#13927).
/// </summary>
public class FormattingTests
{
    [Fact]
    public void TrailingOverrideBlock_IsNotSentAndComesBack()
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed(@"Overboard{\fad(200,200)}", "en");

        Assert.Equal("Overboard", text);
        Assert.Equal(@"Par-dessus bord{\fad(200,200)}", formatting.ReAddFormatting("Par-dessus bord"));
    }

    [Fact]
    public void LeadingAndTrailingBlocks_BothSurvive()
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed(@"{\pos(946.5,250.8)\fs54}Overboard{\fad(200,200)}", "en");

        Assert.Equal("Overboard", text);
        Assert.Equal(@"{\pos(946.5,250.8)\fs54}Par-dessus bord{\fad(200,200)}", formatting.ReAddFormatting("Par-dessus bord"));
    }

    [Fact]
    public void MultipleTrailingBlocks_KeepTheirOrder()
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed(@"Hi{\i1}{\fad(1,2)}", "en");

        Assert.Equal("Hi", text);
        Assert.Equal(@"Salut{\i1}{\fad(1,2)}", formatting.ReAddFormatting("Salut"));
    }

    [Fact]
    public void TrailingBlock_LetsTheItalicCheckSeeTheRealEndOfTheLine()
    {
        var formatting = new Formatting();

        // Before the trailing block was taken off, EndsWith("</i>") was false here, so the italic
        // tags went to the engine as text instead of being restored around the answer.
        var text = formatting.SetTagsAndReturnTrimmed(@"<i>Overboard</i>{\fad(1,2)}", "en");

        Assert.Equal("Overboard", text);
        Assert.Equal(@"<i>Par-dessus bord</i>{\fad(1,2)}", formatting.ReAddFormatting("Par-dessus bord"));
    }

    [Fact]
    public void ClosingBraceThatIsNotAnOverrideBlock_IsLeftAlone()
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed("Say {this}", "en");

        Assert.Equal("Say {this}", text);
        Assert.Equal("Dis {this}", formatting.ReAddFormatting("Dis {this}"));
    }

    [Fact]
    public void BraceBelongingToAnEarlierBlock_IsNotTakenAsTrailing()
    {
        var formatting = new Formatting();

        // The '}' at the end closes nothing - the only "{\" opens a block that closes mid-line.
        var text = formatting.SetTagsAndReturnTrimmed(@"a{\b1}b}", "en");

        Assert.Equal(@"a{\b1}b}", text);
        Assert.Equal(@"a{\b1}b}", formatting.ReAddFormatting(@"a{\b1}b}"));
    }

    [Fact]
    public void PlainLine_IsUnchanged()
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed("Overboard", "en");

        Assert.Equal("Overboard", text);
        Assert.Equal("Par-dessus bord", formatting.ReAddFormatting("Par-dessus bord"));
    }
    // #14424: a vector drawing's "text" is shape commands - it must never reach the engine.
    [Theory]
    [InlineData(@"{\p1}m 0 0 l 100 0 100 100 0 100{\p0}")]
    [InlineData(@"{\an7\pos(10,10)\p2}m 0 0 l 10 10")]
    [InlineData(@"{\an8}{\p1}m 0 0 l 10 10")]
    public void VectorDrawing_IsKeptVerbatim(string input)
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed(input, "en");

        Assert.DoesNotContain("m 0 0", text);
        Assert.Equal(input, formatting.ReAddFormatting("whatever the engine answered"));
    }

    [Fact]
    public void PosTag_IsNotMistakenForDrawingMode()
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed(@"{\pos(10,20)}Hello", "en");

        Assert.Equal("Hello", text);
    }

    // #14424: blocks in the middle whose tags apply to the whole line are moved to the front.
    [Theory]
    [InlineData(@"Hello{\pos(10,20)} world", "Hello world", @"{\pos(10,20)}Hej verden")]
    [InlineData(@"{\blur1}Hello {\an8\fad(200,200)}world", "Hello world", @"{\blur1}{\an8\fad(200,200)}Hej verden")]
    [InlineData(@"Hello {\move(1,2,3,4)}{\clip(0,0,10,10)}world", "Hello world", @"{\move(1,2,3,4)}{\clip(0,0,10,10)}Hej verden")]
    public void LineGlobalBlockInTheMiddle_IsTakenOffAndRestoredInFront(string input, string expectedSent, string expectedBack)
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed(input, "en");

        Assert.Equal(expectedSent, text);
        Assert.Equal(expectedBack, formatting.ReAddFormatting("Hej verden"));
    }

    // Moving a position-dependent tag would change what it styles, so such blocks stay put.
    [Theory]
    [InlineData(@"Hello {\i1}world{\i0}!")]
    [InlineData(@"Hello {\alpha&H80&}world")]
    [InlineData(@"Hello {\pos(1,2)\c&H0000FF&}world")]
    [InlineData(@"Ka{\k30}ra{\k25}o{\k40}ke")]
    [InlineData(@"Hello {\t(\frz10)}world")]
    public void PositionDependentBlockInTheMiddle_StaysInPlace(string input)
    {
        var formatting = new Formatting();

        var text = formatting.SetTagsAndReturnTrimmed(input, "en");

        Assert.Equal(input, text);
    }
}
