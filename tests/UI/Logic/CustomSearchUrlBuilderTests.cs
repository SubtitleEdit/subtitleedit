using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// The "search via" slots (Options > Shortcuts) hold a URL template the user types themselves, so
/// the builder has to survive a template with no placeholder, stray braces, and anything that is
/// not a web address at all.
/// </summary>
public class CustomSearchUrlBuilderTests
{
    [Fact]
    public void PutsTheEncodedTextWhereThePlaceholderIs()
    {
        var url = CustomSearchUrlBuilder.Build("https://duckduckgo.com/?q={0}", "a & b");

        Assert.Equal("https://duckduckgo.com/?q=a%20%26%20b", url);
    }

    [Fact]
    public void AppendsTheTextWhenTheTemplateHasNoPlaceholder()
    {
        var url = CustomSearchUrlBuilder.Build("https://www.thefreedictionary.com/", "word");

        Assert.Equal("https://www.thefreedictionary.com/word", url);
    }

    [Fact]
    public void SearchesATwoLineSubtitleAsOnePhrase()
    {
        var url = CustomSearchUrlBuilder.Build("https://example.com/?q={0}", "first line\r\nsecond line");

        Assert.Equal("https://example.com/?q=first%20line%20second%20line", url);
    }

    [Fact]
    public void DoesNotThrowOnAStrayBraceInTheTemplate()
    {
        var url = CustomSearchUrlBuilder.Build("https://example.com/?q={0}&x={1", "hi");

        Assert.Equal("https://example.com/?q=hi&x={1", url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    // Not a URL at all, or a scheme that would hand the shell something other than a web page.
    [InlineData("thefreedictionary.com/{0}")]
    [InlineData("file:///etc/passwd?q={0}")]
    [InlineData("javascript:alert({0})")]
    public void ReturnsNullForAnythingThatIsNotAWebAddress(string? template)
    {
        Assert.Null(CustomSearchUrlBuilder.Build(template, "word"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  \r\n ")]
    [InlineData(null)]
    public void ReturnsNullWithNothingToSearchFor(string? text)
    {
        Assert.Null(CustomSearchUrlBuilder.Build("https://example.com/?q={0}", text));
    }
}
