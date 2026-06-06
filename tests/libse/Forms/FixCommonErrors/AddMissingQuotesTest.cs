using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Forms.FixCommonErrors;

public class AddMissingQuotesTest
{
    private static Subtitle Fix(params Paragraph[] paragraphs)
    {
        var subtitle = new Subtitle();
        foreach (var p in paragraphs)
        {
            subtitle.Paragraphs.Add(p);
        }

        subtitle.Renumber();
        new AddMissingQuotes().Fix(subtitle, new EmptyFixCallback());
        return subtitle;
    }

    private static Paragraph P(string text, double startMs, double endMs) => new(text, startMs, endMs);

    // ---------------------------------------------------------------------
    // A line that is already balanced must never be touched, no matter how
    // many quote pairs it contains. This is the user's concern: a line can
    // legitimately quote more than one word, e.g. I like "this" and "that".
    // ---------------------------------------------------------------------

    [Fact]
    public void SingleBalancedPair_OnOneLine_IsUnchanged()
    {
        var sub = Fix(P("\"Hello world.\"", 0, 2000));

        Assert.Equal("\"Hello world.\"", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void TwoBalancedPairs_OnOneLine_IsUnchanged()
    {
        var sub = Fix(P("I like \"this\" and \"that\".", 0, 2000));

        Assert.Equal("I like \"this\" and \"that\".", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void TwoBalancedPairs_StartingAndEndingWithQuote_IsUnchanged()
    {
        var sub = Fix(P("\"this\" and \"that\"", 0, 2000));

        Assert.Equal("\"this\" and \"that\"", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void ThreeBalancedPairs_OnOneLine_IsUnchanged()
    {
        var sub = Fix(P("\"a\", \"b\" and \"c\".", 0, 2000));

        Assert.Equal("\"a\", \"b\" and \"c\".", sub.Paragraphs[0].Text);
    }

    // ---------------------------------------------------------------------
    // The branch the PR changes: the current line has a single (unbalanced)
    // quote and the next line starts and ends with a quote. The next line is
    // only "already valid" when its quote count is even.
    // ---------------------------------------------------------------------

    [Fact]
    public void NextLineHasTwoPairs_IsTreatedAsValid_CurrentLineQuoteClosedOnItsOwnLine()
    {
        // Before the fix, the next line's two quote pairs (4 quotes) were not
        // recognised as valid, so the open quote on line 1 was wrongly assumed
        // to span into line 2. It should instead be closed on line 1.
        var sub = Fix(
            P("She said \"good morning", 0, 2000),
            P("\"Yes\" and \"no.\"", 2500, 4500));

        Assert.Equal("She said \"good morning\"", sub.Paragraphs[0].Text);
        Assert.Equal("\"Yes\" and \"no.\"", sub.Paragraphs[1].Text);
    }

    [Fact]
    public void NextLineHasOnePair_IsTreatedAsValid_CurrentLineQuoteClosedOnItsOwnLine()
    {
        // The original (== 2) behaviour, which the even-count check preserves.
        var sub = Fix(
            P("She said \"good morning", 0, 2000),
            P("\"Hello there.\"", 2500, 4500));

        Assert.Equal("She said \"good morning\"", sub.Paragraphs[0].Text);
        Assert.Equal("\"Hello there.\"", sub.Paragraphs[1].Text);
    }

    // ---------------------------------------------------------------------
    // A genuine quote spanning two lines (one quote opening, one closing)
    // must be left untouched - nothing is missing.
    // ---------------------------------------------------------------------

    [Fact]
    public void QuoteSpanningTwoLines_IsLeftUnchanged()
    {
        var sub = Fix(
            P("He said, \"I will go to the store", 0, 2000),
            P("and buy some milk.\"", 2500, 4500));

        Assert.Equal("He said, \"I will go to the store", sub.Paragraphs[0].Text);
        Assert.Equal("and buy some milk.\"", sub.Paragraphs[1].Text);
    }

    // ---------------------------------------------------------------------
    // The previous-line branch is the mirror image of the next-line branch
    // and uses the same even-count check, so a previous line that already
    // contains valid quote pairs is recognised as self-contained regardless
    // of how many pairs it has.
    // ---------------------------------------------------------------------

    [Fact]
    public void PrevLineHasOnePair_DanglingCloseQuoteGetsOpened()
    {
        var sub = Fix(
            P("\"Hello.\"", 0, 2000),
            P("good morning\"", 2500, 4500));

        Assert.Equal("\"Hello.\"", sub.Paragraphs[0].Text);
        Assert.Equal("\"good morning\"", sub.Paragraphs[1].Text);
    }

    [Fact]
    public void PrevLineHasTwoPairs_DanglingCloseQuoteGetsOpened()
    {
        // Mirror of NextLineHasTwoPairs...: the previous line's two quote pairs
        // are recognised as self-contained, so the second line's dangling close
        // quote is opened on its own line instead of being assumed to span.
        var sub = Fix(
            P("\"Hi\" and \"bye.\"", 0, 2000),
            P("good morning\"", 2500, 4500));

        Assert.Equal("\"Hi\" and \"bye.\"", sub.Paragraphs[0].Text);
        Assert.Equal("\"good morning\"", sub.Paragraphs[1].Text);
    }
}
