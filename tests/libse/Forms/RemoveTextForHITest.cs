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
    [InlineData("First officer's log.|Stardate 2122.4.|UNA:", "First officer's log.|Stardate 2122.4.")]
    [InlineData("First officer's log.|Stardate 2122.4.|<i>UNA:</i>", "First officer's log.|Stardate 2122.4.")]
    [InlineData("First officer's log.|Stardate 2122.4.|- UNA:", "First officer's log.|Stardate 2122.4.")]
    [InlineData("First officer's log.|Stardate 2122.4.|OLD MAN:", "First officer's log.|Stardate 2122.4.")]
    [InlineData("First officer's log.|UNA:", "First officer's log.")]
    public void Colon_TrailingNameOnOwnLine_IsRemoved(string input, string expected)
    {
        // The name of the next speaker on a line of its own after the text - the
        // whole text was left alone before, as its only colon is the last character.
        var remover = MakeRemover();
        remover.Settings.RemoveTextBeforeColon = true;

        var text = input.Replace("|", Environment.NewLine);

        Assert.Equal(expected.Replace("|", Environment.NewLine), remover.RemoveTextFromHearImpaired(text, "en"));
    }

    [Theory]
    [InlineData("First officer's log.|Stardate 2122.4.|MAN ON RADIO:", "First officer's log.|Stardate 2122.4.")]
    [InlineData("First officer's log.|Stardate 2122.4.|<i>MAN ON RADIO:</i>", "First officer's log.|Stardate 2122.4.")]
    [InlineData("Come in.|Do you read me?|WOMAN ON TV:", "Come in.|Do you read me?")]
    [InlineData("Come in.|MAN ON RADIO:", "Come in.")]
    public void Colon_TrailingMultiWordLabel_IsRemoved(string input, string expected)
    {
        // A label of more than two words is only taken when the line before it is a
        // finished sentence - see Colon_TrailingMultiWordLabel_KeptWhenLineContinues.
        var remover = MakeRemover();
        remover.Settings.RemoveTextBeforeColon = true;

        var text = input.Replace("|", Environment.NewLine);

        Assert.Equal(expected.Replace("|", Environment.NewLine), remover.RemoveTextFromHearImpaired(text, "en"));
    }

    [Theory]
    [InlineData("And she would like you to do|three things:")]
    [InlineData("It was quite a long day|and then he said|something:")]
    [InlineData("Here is the thing|I wanted to say:")]
    // the last line continues the line before it, so it is a sentence, not a label
    [InlineData("I could hear the|MAN ON RADIO:")]
    [InlineData("I HAVE A LIST OF|THINGS TO DO:")]
    // uppercase sentences after a finished sentence - kept by the narrator check
    [InlineData("HE SAID THIS TO ME.|HERE IS THE LIST:")]
    [InlineData("IT WAS A LONG DAY.|AND THEN HE SAID THIS:")]
    public void Colon_SentenceEndingInColon_IsKept(string input)
    {
        // A sentence that just happens to end in a colon is not a speaker name.
        var remover = MakeRemover();
        remover.Settings.RemoveTextBeforeColon = true;

        var text = input.Replace("|", Environment.NewLine);

        Assert.Equal(text, remover.RemoveTextFromHearImpaired(text, "en"));
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
