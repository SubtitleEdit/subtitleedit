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

    // --- Colon: the bug from issue #13681 ---

    [Theory]
    [InlineData("-UNA: I have it.|-M'BENGA: Good.|Get out of there.")]
    [InlineData("- UNA: I have it.|- M'BENGA: Good.|Get out of there.")]
    [InlineData("UNA: I have it.|M'BENGA: Good.|Get out of there.")]
    public void Colon_ThreeLinesTwoSpeakers_KeepsDialogDashes(string input)
    {
        // The second speaker continues on the third line, so the text is not
        // recognized as a dialog - but two names were removed from the first
        // two lines, so both lines need a dialog dash.
        var remover = MakeRemover();
        remover.Settings.RemoveTextBeforeColon = true;

        var text = input.Replace("|", Environment.NewLine);
        var expected = "- I have it." + Environment.NewLine + "- Good." + Environment.NewLine + "Get out of there.";

        Assert.Equal(expected, remover.RemoveTextFromHearImpaired(text, "en"));
    }

    [Theory]
    [InlineData("UNA:|First officer's log.|Stardate 2122.4.", "First officer's log.|Stardate 2122.4.")]
    [InlineData("MAN ON RADIO:|Come in.|Do you read me?", "Come in.|Do you read me?")]
    [InlineData("<i>UNA:|First officer's log.|Stardate 2122.4.</i>", "<i>First officer's log.|Stardate 2122.4.</i>")]
    public void Colon_ThreeLinesNameOnOwnLine_KeepsSingleSpeaker(string input, string expected)
    {
        // The name takes up the whole first line, so removing it leaves two lines
        // from one and the same speaker - no dialog dashes wanted.
        var remover = MakeRemover();
        remover.Settings.RemoveTextBeforeColon = true;

        var text = input.Replace("|", Environment.NewLine);

        Assert.Equal(expected.Replace("|", Environment.NewLine), remover.RemoveTextFromHearImpaired(text, "en"));
    }

    [Theory]
    [InlineData("UNA:|I have it.|M'BENGA: Good.", "- I have it.|- Good.")]
    [InlineData("I have it.|M'BENGA: Good.", "- I have it.|- Good.")]
    public void Colon_NameOnOwnLine_StillDashesRealDialog(string input, string expected)
    {
        // A second name was removed from a line that survived, so there really
        // are two speakers left.
        var remover = MakeRemover();
        remover.Settings.RemoveTextBeforeColon = true;

        var text = input.Replace("|", Environment.NewLine);

        Assert.Equal(expected.Replace("|", Environment.NewLine), remover.RemoveTextFromHearImpaired(text, "en"));
    }
}
