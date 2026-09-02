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
}
