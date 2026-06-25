using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;

namespace LibSETests.Forms;

public class RemoveTextForHITest
{
    private static RemoveTextForHI MakeRemover(string customStart = "", string customEnd = "")
    {
        var settings = new RemoveTextForHISettings(new Subtitle())
        {
            OnlyIfInSeparateLine = false,
            RemoveTextBetweenSquares = false,
            RemoveTextBetweenBrackets = false,
            RemoveTextBetweenParentheses = false,
            RemoveTextBetweenQuestionMarks = false,
            RemoveTextBetweenCustomTags = false,
            RemoveInterjections = false,
            RemoveIfAllUppercase = false,
            RemoveTextBeforeColon = false,
            RemoveWhereContains = false,
            RemoveIfOnlyMusicSymbols = false,
            CustomStart = customStart,
            CustomEnd = customEnd,
        };

        return new RemoveTextForHI(settings);
    }

    // --- Custom tags: the bug from issue #11850 ---

    [Fact]
    public void Custom_SingleChar_RemovesBetween()
    {
        var remover = MakeRemover("(", ")");
        remover.Settings.RemoveTextBetweenCustomTags = true;

        Assert.Equal("Hello", remover.RemoveHearImpairedTags("(noise) Hello"));
    }

    [Fact]
    public void Custom_MultiChar_RemovesWholeEndTag()
    {
        // Before the fix only the first char of the end tag (">") was removed,
        // leaving "> Hello".
        var remover = MakeRemover("<<", ">>");
        remover.Settings.RemoveTextBetweenCustomTags = true;

        Assert.Equal("Hello", remover.RemoveHearImpairedTags("<<noise>> Hello"));
    }

    [Fact]
    public void Custom_MultiChar_HtmlLikeTags()
    {
        var remover = MakeRemover("<i>", "</i>");
        remover.Settings.RemoveTextBetweenCustomTags = true;

        Assert.Equal("Hello", remover.RemoveHearImpairedTags("<i>noise</i> Hello"));
    }

    [Fact]
    public void Custom_MultiChar_InMiddle()
    {
        var remover = MakeRemover("[[", "]]");
        remover.Settings.RemoveTextBetweenCustomTags = true;

        Assert.Equal("Hello there", remover.RemoveHearImpairedTags("Hello [[noise]] there"));
    }

    // --- Predefined tags: regression guards (must keep working after the fix) ---

    [Fact]
    public void Squares_RemovesBetween()
    {
        var remover = MakeRemover();
        remover.Settings.RemoveTextBetweenSquares = true;

        Assert.Equal("Hello", remover.RemoveHearImpairedTags("[noise] Hello"));
    }

    [Fact]
    public void Squares_WithColon_RemovesNameAndColon()
    {
        var remover = MakeRemover();
        remover.Settings.RemoveTextBetweenSquares = true;

        Assert.Equal("Hello", remover.RemoveHearImpairedTags("[MAN]: Hello"));
    }

    [Fact]
    public void Parentheses_RemovesBetween()
    {
        var remover = MakeRemover();
        remover.Settings.RemoveTextBetweenParentheses = true;

        Assert.Equal("Bye", remover.RemoveHearImpairedTags("(SIGH) Bye"));
    }

    [Fact]
    public void Parentheses_WithColon_RemovesNameAndColon()
    {
        var remover = MakeRemover();
        remover.Settings.RemoveTextBetweenParentheses = true;

        Assert.Equal("Hello", remover.RemoveHearImpairedTags("(MAN): Hello"));
    }

    [Fact]
    public void CurlyBrackets_RemovesBetween()
    {
        var remover = MakeRemover();
        remover.Settings.RemoveTextBetweenBrackets = true;

        Assert.Equal("Hello", remover.RemoveHearImpairedTags("{noise} Hello"));
    }
}
