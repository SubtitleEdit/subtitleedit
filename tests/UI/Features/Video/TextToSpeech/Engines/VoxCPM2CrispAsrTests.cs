using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

public class VoxCPM2CrispAsrTests
{
    // #14480: VoxCPM2 stops right after the last phoneme when the line has no sentence ending,
    // which is heard as the last word being cut off. Measured with fixed seeds: 8-9 of 20 chopped
    // endings without terminal punctuation / with a comma / with an ellipsis, 2 of 20 with a full
    // stop. The full stop is added for the synthesis text only.
    [Theory]
    [InlineData("Non mi aspettavo che tornasse", "Non mi aspettavo che tornasse.")]
    [InlineData("Non mi aspettavo che tornasse,", "Non mi aspettavo che tornasse.")]
    [InlineData("Non mi aspettavo che tornasse...", "Non mi aspettavo che tornasse.")]
    [InlineData("Non mi aspettavo che tornasse\u2026", "Non mi aspettavo che tornasse.")]
    [InlineData("Non mi aspettavo che tornasse -", "Non mi aspettavo che tornasse.")]
    [InlineData("Non mi aspettavo che tornasse\u2014", "Non mi aspettavo che tornasse.")]
    [InlineData("Non mi aspettavo che tornasse:", "Non mi aspettavo che tornasse.")]
    [InlineData("Non mi aspettavo che tornasse  ", "Non mi aspettavo che tornasse.")]
    [InlineData("彼が戻るとは思わなかった", "彼が戻るとは思わなかった。")]
    [InlineData("彼が戻るとは思わなかった、", "彼が戻るとは思わなかった。")]
    public void EnsureSentenceEnding_GivesAFullStopToALineWithoutASentenceEnding(string text, string expected)
    {
        Assert.Equal(expected, VoxCPM2CrispAsr.EnsureSentenceEnding(text));
    }

    [Theory]
    [InlineData("Perché hai spento le luci?")]
    [InlineData("Vattene!")]
    [InlineData("Il treno parte alle sette.")]
    [InlineData("これは何ですか？")]
    [InlineData("行こう。")]
    [InlineData("ماذا تريد؟")]
    public void EnsureSentenceEnding_KeepsALineThatAlreadyEndsASentence(string text)
    {
        Assert.Equal(text, VoxCPM2CrispAsr.EnsureSentenceEnding(text));
    }

    [Theory]
    [InlineData("Perché hai spento le luci?...", "Perché hai spento le luci?")]
    [InlineData("Vattene!,", "Vattene!")]
    public void EnsureSentenceEnding_DropsASoftEndingAfterARealOne(string text, string expected)
    {
        Assert.Equal(expected, VoxCPM2CrispAsr.EnsureSentenceEnding(text));
    }

    [Theory]
    [InlineData("\"Non mi aspettavo che tornasse\"", "\"Non mi aspettavo che tornasse.\"")]
    [InlineData("\"Non mi aspettavo che tornasse,\"", "\"Non mi aspettavo che tornasse.\"")]
    [InlineData("(non mi aspettavo che tornasse)", "(non mi aspettavo che tornasse.)")]
    [InlineData("\"Perché?\"", "\"Perché?\"")]
    [InlineData("\u00ABNon mi aspettavo che tornasse\u00BB", "\u00ABNon mi aspettavo che tornasse.\u00BB")]
    public void EnsureSentenceEnding_KeepsAClosingQuoteOrBracketAfterTheFullStop(string text, string expected)
    {
        Assert.Equal(expected, VoxCPM2CrispAsr.EnsureSentenceEnding(text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("...")]
    [InlineData("\"\"")]
    public void EnsureSentenceEnding_LeavesALineWithNoWordsAlone(string text)
    {
        Assert.Equal(text, VoxCPM2CrispAsr.EnsureSentenceEnding(text));
    }

    [Fact]
    public void BuildSpeakPayload_ForAPerLineClone_SendsTheStagedFileNameAsVoice()
    {
        var payload = VoxCPM2CrispAsr.BuildSpeakPayload("hello.", 1.0, "se-per-line-line-0007.wav");

        Assert.Equal("se-per-line-line-0007.wav", payload["voice"]);
        Assert.Equal("hello.", payload["input"]);
    }
}
