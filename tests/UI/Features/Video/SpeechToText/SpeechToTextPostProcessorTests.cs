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
}
