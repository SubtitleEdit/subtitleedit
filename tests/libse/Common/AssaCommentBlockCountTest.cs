using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;

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
}
