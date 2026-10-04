using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class FixDifferentQuotesTest
{
    private static string Fix(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 2000));
        new FixDifferentQuotes().Fix(subtitle, new EmptyFixCallback());
        return subtitle.Paragraphs[0].Text;
    }

    [Theory]
    [InlineData("\"Hello”", "\"Hello\"")]
    [InlineData("“Hello\"", "\"Hello\"")]
    [InlineData("He said \"yes”.", "He said \"yes\".")]
    [InlineData("<i>“Hello\"</i>", "<i>\"Hello\"</i>")]
    public void Fixes(string input, string expected)
    {
        Assert.Equal(expected, Fix(input));
    }

    [Theory]
    [InlineData("\"Hello\"")]
    [InlineData("“Hello”")]
    [InlineData("„Hallo\"")]
    [InlineData("„Hallo“")]
    [InlineData("\"a\" and ”b")]
    [InlineData("\"Hello” and ”bye")]
    [InlineData("Hello")]
    [InlineData("")]
    public void Unchanged(string input)
    {
        Assert.Equal(input, Fix(input));
    }

    [Fact]
    public void RunsBeforeAddMissingQuotes()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("\"Hello”", 0, 2000));
        new FixDifferentQuotes().Fix(subtitle, new EmptyFixCallback());
        new AddMissingQuotes().Fix(subtitle, new EmptyFixCallback());

        Assert.Equal("\"Hello\"", subtitle.Paragraphs[0].Text);
    }
}
