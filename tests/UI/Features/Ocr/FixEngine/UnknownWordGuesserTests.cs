using System.Linq;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;

namespace UITests.Features.Ocr.FixEngine;

public class UnknownWordGuesserTests
{
    // Regression test for https://github.com/SubtitleEdit/subtitleedit/issues/12156
    // "Fix common OCR errors" inserted a space right after the hyphen in Dutch hyphenated
    // words (e.g. "vaudeville-veteraan" -> "vaudeville- veteraan"), because the smart-split
    // heuristic treated '-' as a splittable consonant. A split must never land next to a
    // non-letter.
    [Theory]
    [InlineData("vaudeville-veteraan")]
    [InlineData("Bailey-gebied")]
    [InlineData("zwart-wit-foto")]
    [InlineData("moeder-overste")]
    public void CreateGuessesFromLetters_NeverSplitsNextToHyphen(string word)
    {
        var guesses = UnknownWordGuesser.CreateGuessesFromLetters(word, "nld").ToList();

        foreach (var guess in guesses)
        {
            var spaceIndex = guess.IndexOf(' ');
            while (spaceIndex >= 0)
            {
                Assert.True(spaceIndex > 0 && guess[spaceIndex - 1] != '-',
                    $"'{word}' produced guess '{guess}' with a space right after a hyphen");
                Assert.True(spaceIndex < guess.Length - 1 && guess[spaceIndex + 1] != '-',
                    $"'{word}' produced guess '{guess}' with a space right before a hyphen");
                spaceIndex = guess.IndexOf(' ', spaceIndex + 1);
            }
        }
    }

    // A hyphenated word must not be turned into a split whose only difference from the input
    // is a space adjacent to the hyphen (the exact symptom reported in #12156).
    [Fact]
    public void CreateGuessesFromLetters_HyphenatedWord_DoesNotOnlyAddSpaceAtHyphen()
    {
        var guesses = UnknownWordGuesser.CreateGuessesFromLetters("vaudeville-veteraan", "nld").ToList();

        Assert.DoesNotContain("vaudeville- veteraan", guesses);
        Assert.DoesNotContain("vaudeville -veteraan", guesses);
    }

    // Ordinary (non-hyphenated) words are still split, so the fix does not disable the feature.
    [Fact]
    public void CreateGuessesFromLetters_PlainWord_StillSplitsBetweenConsonants()
    {
        var guesses = UnknownWordGuesser.CreateGuessesFromLetters("werkplek", "nld").ToList();

        Assert.Contains("werk plek", guesses);
    }
}
