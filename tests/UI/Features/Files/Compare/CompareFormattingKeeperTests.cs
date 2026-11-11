using Nikse.SubtitleEdit.Features.Files.Compare;

namespace UITests.Features.Files.Compare;

/// <summary>"Take text, keep formatting" moves only the tags that wrap the current line (#16823).</summary>
public class CompareFormattingKeeperTests
{
    [Theory]
    [InlineData("{\\i1}von derUniversityofSouthern California.{\\i0}", "von der University of Southern California.", "{\\i1}von der University of Southern California.{\\i0}")]
    [InlineData("{\\i1}Wieder ein enttäuschen der A uftritt\nfür die Ohio State.{\\i0}", "Wieder ein enttäuschender Auftritt\nfür die Ohio State.", "{\\i1}Wieder ein enttäuschender Auftritt\nfür die Ohio State.{\\i0}")]
    [InlineData("{\\an8}{\\i1}m it Gitarre", "mit Gitarre", "{\\an8}{\\i1}mit Gitarre")]
    [InlineData("{\\an8}*schweres Atmen *", "{\\an8}* schweres Atmen *", "{\\an8}* schweres Atmen *")]
    [InlineData("<i>Nach 2Jahren</i>", "Nach 2 Jahren", "<i>Nach 2 Jahren</i>")]
    [InlineData("<i><b>Fam ilie</b></i>", "Familie", "<i><b>Familie</b></i>")]
    [InlineData("<i>Line one</i>\n<i>Line too</i>", "Line one\nLine two", "<i>Line one\nLine two</i>")]
    [InlineData("<i>Line one</i>\r\n<i>Line too</i>", "Line one\r\nLine two", "<i>Line one\r\nLine two</i>")]
    [InlineData("<font color=\"#ff0000\">im mer</font>", "immer", "<font color=\"#ff0000\">immer</font>")]
    public void KeepsTheWrappingTags(string current, string reference, string expected)
    {
        Assert.Equal(expected, CompareFormattingKeeper.TakeText(current, reference));
    }

    [Theory]
    [InlineData("Ich war im mer da.", "Ich war immer da.", "Ich war immer da.")]
    [InlineData("Ich war im mer da.", "<i>Ich war immer da.</i>", "Ich war immer da.")]
    [InlineData("Ich war im mer da.", "{\\i1}Ich war immer da.{\\i0}", "Ich war immer da.")]
    public void TakesThePlainReferenceText_WhenTheCurrentLineHasNoFormatting(string current, string reference, string expected)
    {
        Assert.Equal(expected, CompareFormattingKeeper.TakeText(current, reference));
    }

    [Theory]
    [InlineData("<i>One</i> two <i>three</i>", "One two three", "One two three")]
    [InlineData("<i>One</i> two", "One two", "One two")]
    [InlineData("One <i>two</i>", "One two", "One two")]
    [InlineData("<i>One <b>two</b> three</i>", "One two three", "<i>One two three</i>")]
    public void DropsTagsThatDoNotWrapTheWholeLine(string current, string reference, string expected)
    {
        Assert.Equal(expected, CompareFormattingKeeper.TakeText(current, reference));
    }

    [Fact]
    public void KeepsAnUnclosedAssaTag()
    {
        Assert.Equal("{\\i1}Snyder als Trainer", CompareFormattingKeeper.TakeText("{\\i1}Snyder alsTrainer", "Snyder als Trainer"));
    }
}
