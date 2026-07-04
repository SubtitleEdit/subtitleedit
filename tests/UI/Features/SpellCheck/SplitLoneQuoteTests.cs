using Nikse.SubtitleEdit.Features.SpellCheck;
using System.Linq;

namespace UITests.Features.SpellCheck;

// #12143: a subtitle wrapped in single quotes (e.g. 'Met de waarheid\nverover ik het universum.')
// made the tokenizer emit the trailing quote as its own word ("'"), which spell check then flagged
// as unknown. Split must not emit tokens that consist solely of apostrophe/quote characters, while
// keeping quote-wrapped words and contractions intact.
public class SplitLoneQuoteTests
{
    [Fact]
    public void Split_ShouldNotEmitTrailingLoneQuote()
    {
        var words = SpellCheckWordLists.Split("verover ik het universum.'");

        Assert.DoesNotContain(words, w => w.Text == "'");
        Assert.Equal(new[] { "verover", "ik", "het", "universum" }, words.Select(w => w.Text));
    }

    [Fact]
    public void Split_ShouldKeepQuoteWrappedWord()
    {
        // The leading quote stays attached to the word so the checker's Trim('\'') retry can validate it.
        var words = SpellCheckWordLists.Split("'Met de waarheid");

        Assert.Equal(new[] { "'Met", "de", "waarheid" }, words.Select(w => w.Text));
    }

    [Fact]
    public void Split_ShouldKeepContraction()
    {
        var words = SpellCheckWordLists.Split("'n beetje don't");

        Assert.Equal(new[] { "'n", "beetje", "don't" }, words.Select(w => w.Text));
    }

    [Fact]
    public void Split_ShouldDropStandaloneQuoteToken()
    {
        // A lone quote surrounded by spaces (or typographic single quotes) carries no word content.
        var words = SpellCheckWordLists.Split("hallo ' ’ wereld");

        Assert.Equal(new[] { "hallo", "wereld" }, words.Select(w => w.Text));
    }

    [Fact]
    public void Split_ShouldPreserveIndexOfKeptWords()
    {
        var words = SpellCheckWordLists.Split("universum.'");

        var word = Assert.Single(words);
        Assert.Equal("universum", word.Text);
        Assert.Equal(0, word.Index);
    }
}
