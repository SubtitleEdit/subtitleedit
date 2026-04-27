using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class FixCommonErrorsRunnerTest
{
    [Fact]
    public void RunAll_OnEmptySubtitle_NoThrow()
    {
        var sub = new Subtitle();
        FixCommonErrorsRunner.RunAll(sub);
        Assert.Empty(sub.Paragraphs);
    }

    [Fact]
    public void RunAll_FixesMissingSpaceAfterComma()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("hello,world.", 0, 1000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        // FixMissingSpaces adds a space after the comma; FixStartWithUppercaseLetter
        // capitalises the leading 'h'. End result: "Hello, world."
        Assert.Equal("Hello, world.", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void RunAll_FixesUnneededSpaceBeforePeriod()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hello world .", 0, 2000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        Assert.DoesNotContain(" .", sub.Paragraphs[0].Text);
        Assert.EndsWith(".", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void RunAll_RemovesEmptyLines()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hello world.", 0, 2000));
        sub.Paragraphs.Add(new Paragraph(string.Empty, 3000, 5000));
        sub.Paragraphs.Add(new Paragraph("Goodbye.", 6000, 8000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        // FixEmptyLines drops the empty paragraph
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal("Hello world.", sub.Paragraphs[0].Text);
        Assert.Equal("Goodbye.", sub.Paragraphs[1].Text);
    }

    [Fact]
    public void RunAll_FixesAloneLowercaseI_InEnglish()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Yes, i went to the store.", 0, 2000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        // FixAloneLowercaseIToUppercaseI fixes lowercase 'i' as a standalone word
        Assert.Contains(" I ", sub.Paragraphs[0].Text);
    }
}
