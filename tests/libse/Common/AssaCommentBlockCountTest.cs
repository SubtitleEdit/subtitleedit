using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Tests.Common;

/// <summary>
/// ASSA renderers never draw a {...} block, so fansubbers use {comment} for notes; the
/// character counters skip those blocks for ASSA/SSA only (#15584).
/// </summary>
public class AssaCommentBlockCountTest
{
    [Theory]
    [InlineData("Hello {a note} there", "Hello  there")]
    [InlineData("{note}Hello", "Hello")]
    [InlineData("Hello{}", "Hello")]
    [InlineData("{note \\i1}Hello", "Hello")]
    [InlineData("{\\i1}Hello{\\i0}", "Hello")]
    [InlineData("Hello { there", "Hello { there")]   // unclosed brace is drawn as text
    [InlineData("Hello } there", "Hello } there")]
    public void RemoveSsaTags_RemoveCommentBlocks(string input, string expected)
    {
        Assert.Equal(expected, Utilities.RemoveSsaTags(input, removeCommentBlocks: true));
    }

    [Fact]
    public void RemoveSsaTags_KeepsCommentBlocksByDefault()
    {
        Assert.Equal("Hello {a note} there", Utilities.RemoveSsaTags("Hello {a note} there{\\i1}"));
    }

    [Fact]
    public void CountCharacters_IgnoresCommentBlocksOnlyWhenEnabled()
    {
        const string text = "{hello I am a bracket}Hi!";
        var calc = new CalcAll();
        var old = CalcFactory.IgnoreAssaCommentBlocks;
        try
        {
            CalcFactory.IgnoreAssaCommentBlocks = false;
            Assert.Equal(25, calc.CountCharacters(text, true));

            CalcFactory.IgnoreAssaCommentBlocks = true;
            Assert.Equal(3, calc.CountCharacters(text, true));
            Assert.Equal(3, new CalcNoSpace().CountCharacters(text, true));
            Assert.Equal("Hi!", CalcFactory.RemoveTags(text));
        }
        finally
        {
            CalcFactory.IgnoreAssaCommentBlocks = old;
        }
    }

    [Theory]
    [InlineData("Hello {a note} there", "Hello  there")]
    [InlineData("{note}Hello", "Hello")]
    [InlineData("Hello{}", "Hello")]
    [InlineData("{note\\i1}Hello", "{\\i1}Hello")]          // tags of a block are kept
    [InlineData("{\\an8}{\\i1}Hello{\\i0}", "{\\an8}{\\i1}Hello{\\i0}")]
    [InlineData("Hello { there", "Hello { there")]            // unclosed brace is drawn as text
    [InlineData("a {x} b { c", "a  b { c")]
    [InlineData("Line 1\\N{tl note}Line 2", "Line 1\\NLine 2")]
    [InlineData("", "")]
    public void RemoveCommentBlocks(string input, string expected)
    {
        Assert.Equal(expected, AdvancedSubStationAlpha.RemoveCommentBlocks(input));
    }

    [Theory]
    [InlineData("{TL note}", true)]
    [InlineData("{TL note}{\\i1}", true)]
    [InlineData("{TL note} ", true)]
    [InlineData("{a}\\N{b}", true)]
    [InlineData("{TL note}Hello", false)]
    [InlineData("Hello", false)]
    [InlineData("", false)]                // already empty: not made empty by the comment strip
    [InlineData("{\\i1}", false)]
    [InlineData("Hello { there", false)]
    public void IsCommentOnly(string input, bool expected)
    {
        Assert.Equal(expected, AdvancedSubStationAlpha.IsCommentOnly(input));
    }

    [Fact]
    public void RemoveCommentBlocks_Paragraphs_DropsLinesThatOnlyHeldAComment()
    {
        var paragraphs = new List<Paragraph>
        {
            new Paragraph("{note}Hello", 0, 1000),
            new Paragraph("{TL note: a pun}", 1000, 2000),
            new Paragraph("", 2000, 3000),
            new Paragraph("{\\i1}Bye", 3000, 4000),
        };

        var result = AdvancedSubStationAlpha.RemoveCommentBlocks(paragraphs);

        Assert.Equal(new[] { "Hello", "", "{\\i1}Bye" }, result.Select(p => p.Text));
        Assert.Equal(paragraphs[0].Id, result[0].Id);
        Assert.Equal("{note}Hello", paragraphs[0].Text); // input untouched
    }
}
