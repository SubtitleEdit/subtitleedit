using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

// #17098: the transcript text language overrules the configured language for the file name
// suffix only when it clearly contradicts it - another script, or strong word evidence for a
// language the detector doesn't mix up with the configured one.
public class SpeechToTextFileNameLanguageTests
{
    private const int Strong = SpeechToTextFileNameLanguage.MinimumWordScore;

    [Theory]
    [InlineData("ja", "id", Strong)] // configured Japanese, Indonesian text
    [InlineData("es", "ja", 0)] // different script wins even without word hits
    [InlineData("en", "ko", 0)]
    [InlineData("ru", "en", 0)]
    [InlineData("zh", "ja", 0)]
    [InlineData("en", "sr-latn", Strong)] // Latin Serbian vs English: not confusable, strong
    public void TextLanguageWinsOnClearContradiction(string configured, string detected, int score)
    {
        Assert.Equal(detected, SpeechToTextFileNameLanguage.Resolve(false, configured, detected, score));
    }

    [Theory]
    [InlineData("de", "en", Strong)]
    [InlineData("fr", "it", 50)]
    public void SameScriptStrongWordEvidenceWins(string configured, string detected, int score)
    {
        Assert.Equal(detected, SpeechToTextFileNameLanguage.Resolve(false, configured, detected, score));
    }

    [Theory]
    [InlineData("no", "da", 500)] // close languages: a guess never overrules the user
    [InlineData("id", "ms", 500)]
    [InlineData("pt", "es", 500)]
    [InlineData("hr", "sr-latn", 500)]
    [InlineData("de", "en", Strong - 1)] // too little evidence (short clip)
    [InlineData("de", "en", 0)] // detected from letters only
    [InlineData("sr", "ru", 500)] // Serbian is written in both scripts
    [InlineData("xx", "ja", 0)] // unknown configured code
    [InlineData("pt-br", "pt", 0)] // same language keeps the regional spelling
    public void ConfiguredLanguageIsKept(string configured, string detected, int score)
    {
        Assert.Equal(configured, SpeechToTextFileNameLanguage.Resolve(false, configured, detected, score));
    }

    [Fact]
    public void TranslateToEnglishAlwaysGivesEnglish()
    {
        Assert.Equal("en", SpeechToTextFileNameLanguage.Resolve(true, "ja", "ja", 100));
    }

    [Fact]
    public void FallsBackToConfiguredWhenDetectionInconclusive()
    {
        Assert.Equal("ja", SpeechToTextFileNameLanguage.Resolve(false, "ja", null, 0));
    }

    [Fact]
    public void UsesDetectedWhenNothingConfigured()
    {
        Assert.Equal("id", SpeechToTextFileNameLanguage.Resolve(false, null, "id", 0));
    }

    [Fact]
    public void NoSuffixWhenNothingKnown()
    {
        Assert.Null(SpeechToTextFileNameLanguage.Resolve(false, null, null, 0));
    }
}
