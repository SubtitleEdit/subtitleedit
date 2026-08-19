using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

public class SpeechToTextPostProcessorTests
{
    [Fact]
    public void IsNonStandardLineTerminationLanguage_WhisperJapanese_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("ja"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_WhisperChinese_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("zh"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_WhisperCantonese_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("yue"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_VoskCodes_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("jp"));
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("cn"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_OtherLanguages_ReturnsFalse()
    {
        Assert.False(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("en"));
        Assert.False(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("da"));
    }

    // The merge step used to only know the Vosk codes, so Whisper/Crisp ASR
    // transcripts merged Japanese and Chinese up to the 86-char Latin cap
    // (issue #13548).
    [Theory]
    [InlineData("jp", 32)]
    [InlineData("ja", 32)]
    [InlineData("cn", 36)]
    [InlineData("zh", 36)]
    [InlineData("yue", 36)]
    [InlineData("en", 86)]
    public void MergeShortLines_UsesTheLineLengthCapForTheLanguage(string languageCode, int expectedMaxChars)
    {
        var postProcessor = new SpeechToTextPostProcessor(languageCode);

        postProcessor.MergeShortLines(new Subtitle(), languageCode);

        Assert.Equal(expectedMaxChars, postProcessor.ParagraphMaxChars);
    }
}
