using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class DutchCasingTest
{
    // --- IJ digraph (CapitalizeFirstLetter) ---

    [Theory]
    [InlineData("ijsland", "IJsland")]
    [InlineData("ijs", "IJs")]
    [InlineData("ijzer", "IJzer")]
    [InlineData("Ijsland", "IJsland")]   // wrongly single-capitalized -> corrected
    [InlineData("IJsland", "IJsland")]   // already correct -> idempotent
    [InlineData("morgens", "Morgens")]   // ordinary word
    [InlineData("a", "A")]
    [InlineData("", "")]
    public void CapitalizeFirstLetter_HandlesIjDigraph(string input, string expected)
    {
        Assert.Equal(expected, DutchCasing.CapitalizeFirstLetter(input));
    }

    // --- Sentence start: apostrophe contractions + IJ (FixSentenceStart) ---

    [Theory]
    [InlineData("'s morgens is te laat", "'s Morgens is te laat")] // the issue's "Not Good" case
    [InlineData("'S morgens is te laat", "'s Morgens is te laat")] // the issue's "WRONG" case
    [InlineData("'s Morgens is te laat", "'s Morgens is te laat")] // the issue's "Good" case -> unchanged
    [InlineData("'t kind", "'t Kind")]
    [InlineData("'n appel", "'n Appel")]
    [InlineData("'k weet het", "'k Weet het")]
    [InlineData("'m geven", "'m Geven")]
    [InlineData("'t ijs smelt", "'t IJs smelt")]  // contraction + IJ digraph on next word
    public void FixSentenceStart_HandlesContractions(string input, string expected)
    {
        Assert.Equal(expected, DutchCasing.FixSentenceStart(input));
    }

    [Theory]
    [InlineData("ijsland is koud", "IJsland is koud")] // plain sentence start with IJ
    [InlineData("morgen is het laat", "Morgen is het laat")]
    public void FixSentenceStart_CapitalizesFirstWord(string input, string expected)
    {
        Assert.Equal(expected, DutchCasing.FixSentenceStart(input));
    }

    [Fact]
    public void FixSentenceStart_LeavesNonContractionApostropheAlone()
    {
        // A leading apostrophe that is not a Dutch contraction (e.g. an opening quote) must not be
        // treated as one - we don't move/force casing there (the quote/contraction ambiguity).
        Assert.Equal("'Hallo,' zei hij", DutchCasing.FixSentenceStart("'Hallo,' zei hij"));
    }

    [Theory]
    [InlineData("nl", true)]
    [InlineData("NL", true)]
    [InlineData("en", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsDutch(string? language, bool expected)
    {
        Assert.Equal(expected, DutchCasing.IsDutch(language!));
    }

    // --- StartsWithContraction ---

    [Theory]
    [InlineData("'s morgens", true)]
    [InlineData("'S morgens", true)]   // wrongly capitalized contraction is still a contraction
    [InlineData("'t kind", true)]
    [InlineData("'n appel", true)]
    [InlineData("'k weet", true)]
    [InlineData("'m geven", true)]
    [InlineData("'r naam", true)]
    [InlineData("'x morgens", false)]  // not a contraction letter
    [InlineData("'smorgens", false)]   // no space after the contraction
    [InlineData("'Hallo' zei hij", false)] // opening quote, not a contraction
    [InlineData("morgens", false)]
    [InlineData("'s", false)]          // too short
    [InlineData("", false)]
    [InlineData(null, false)]
    public void StartsWithContraction(string? input, bool expected)
    {
        Assert.Equal(expected, DutchCasing.StartsWithContraction(input!));
    }
}
