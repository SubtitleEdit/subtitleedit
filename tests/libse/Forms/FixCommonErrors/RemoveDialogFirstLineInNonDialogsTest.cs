using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class RemoveDialogFirstLineInNonDialogsTest
{
    private static string Fix(string input)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(input, 0, 2000));
        subtitle.Renumber();
        new RemoveDialogFirstLineInNonDialogs().Fix(subtitle, new EmptyFixCallback());
        return subtitle.Paragraphs[0].Text;
    }

    [Theory]
    [InlineData("-")] // Hyphen-Minus (U+002D)
    [InlineData("‐")] // Hyphen (U+2010)
    [InlineData("‑")] // Non-breaking hyphen (U+2011)
    [InlineData("‒")] // Figure dash (U+2012)
    [InlineData("–")] // En dash (U+2013)
    [InlineData("—")] // Em dash (U+2014)
    [InlineData("―")] // Horizontal bar (U+2015)
    [InlineData("−")] // Minus sign (U+2212)
    public void RemovesStartDash_ForAllDashCharacters(string dash)
    {
        // issue #12748 - the dash is more than ASCII 45
        Assert.Equal("Hi there!", Fix(dash + " Hi there!"));
        Assert.Equal("Hi there!", Fix(dash + "Hi there!"));
    }

    [Theory]
    [InlineData("-")]
    [InlineData("–")]
    [InlineData("—")]
    public void KeepsStartDash_WhenTwoLineDialog(string dash)
    {
        var input = dash + " Hi there!" + Environment.NewLine + dash + " Hello!";
        Assert.Equal(input, Fix(input));
    }

    [Theory]
    [InlineData("-")]
    [InlineData("–")]
    [InlineData("—")]
    public void KeepsStartDash_WhenOneLineDialog(string dash)
    {
        var input = dash + " Hi there! " + dash + " Hello!";
        Assert.Equal(input, Fix(input));
    }

    [Theory]
    [InlineData("- [phone rings] - Hello!")]
    [InlineData("- (sighs) - Who's there?")]
    public void KeepsStartDash_WhenDialogFollowsBracketOrParenthesis(string input)
    {
        Assert.Equal(input, Fix(input));
    }

    [Fact]
    public void KeepsStartDash_WhenSecondLineUsesAnotherDashCharacter()
    {
        var input = "- Hi there!" + Environment.NewLine + "– Hello!";
        Assert.Equal(input, Fix(input));
    }

    [Fact]
    public void RemovesStartDash_WhenTextContainsHyphenatedWord()
    {
        Assert.Equal("It's a well-known fact.", Fix("– It's a well-known fact."));
    }

    [Fact]
    public void RemovesStartDash_InsideItalicTag()
    {
        Assert.Equal("<i>Hi there!</i>", Fix("<i>– Hi there!</i>"));
    }

    [Fact]
    public void KeepsText_WhenNoStartDash()
    {
        Assert.Equal("Hi there – or not!", Fix("Hi there – or not!"));
    }
}
