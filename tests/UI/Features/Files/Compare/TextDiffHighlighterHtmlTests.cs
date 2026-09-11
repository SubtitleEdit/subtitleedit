using Nikse.SubtitleEdit.Features.Files.Compare;

namespace UITests.Features.Files.Compare;

// The compare window's HTML export used to carry only the cell backgrounds; the word-level
// red/green marking the window shows was lost. CompareToHtml renders the same runs as spans.
public class TextDiffHighlighterHtmlTests
{
    [Fact]
    public void CompareToHtml_MarksOnlyTheDifferingRun()
    {
        var (left, right) = TextDiffHighlighter.CompareToHtml("It is what it is.", "It IS what it is.", false, false);

        Assert.Contains("<span style=\"color:#B71C1C;background-color:#FFEBEE\">is</span>", left);
        Assert.Contains("<span style=\"color:#B71C1C;background-color:#FFEBEE\">IS</span>", right);
        Assert.Contains("<span style=\"background-color:#E6FFED\">It&nbsp;</span>", left);
        Assert.Equal("It&nbsp;is&nbsp;what&nbsp;it&nbsp;is.", StripTags(left));
        Assert.Equal("It&nbsp;IS&nbsp;what&nbsp;it&nbsp;is.", StripTags(right));
    }

    [Fact]
    public void CompareToHtml_EqualTexts_AreNotMarked()
    {
        var (left, right) = TextDiffHighlighter.CompareToHtml("Same text", "Same text", false, false);

        // Spaces come out as &nbsp; like the rest of the export (HtmlUtil.EncodeNamed).
        Assert.Equal("Same&nbsp;text", left);
        Assert.Equal("Same&nbsp;text", right);
    }

    [Fact]
    public void CompareToHtml_IgnoredWhitespaceDifference_IsNotMarked()
    {
        var (left, right) = TextDiffHighlighter.CompareToHtml("Hello world", "Hello  world", true, false);

        Assert.DoesNotContain("<span", left);
        Assert.DoesNotContain("<span", right);
    }

    [Fact]
    public void CompareToHtml_OneSideEmpty_MarksTheOtherWhole()
    {
        var (left, right) = TextDiffHighlighter.CompareToHtml(string.Empty, "Only here", false, false);

        Assert.Equal(string.Empty, left);
        Assert.Equal("<span style=\"color:#B71C1C;background-color:#FFEBEE\">Only&nbsp;here</span>", right);
    }

    [Fact]
    public void CompareToHtml_EncodesHtmlAndLineBreaks()
    {
        var (left, _) = TextDiffHighlighter.CompareToHtml("<i>a & b</i>\r\nline two", "<i>a & c</i>\r\nline two", false, false);

        Assert.Contains("&lt;i&gt;", left);
        Assert.Contains("&amp;", left);
        Assert.Contains("<br />", left);
        Assert.DoesNotContain("\r", left);
    }

    private static string StripTags(string html)
        => System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
}
