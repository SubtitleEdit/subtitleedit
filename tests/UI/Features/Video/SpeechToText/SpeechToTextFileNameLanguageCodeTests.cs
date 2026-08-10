using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

// The token can come from the online engines' free-text "language hint" setting, so it may be
// a full language name (the APIs accept those) or arbitrary text with path-hostile characters.
public class SpeechToTextFileNameLanguageCodeTests
{
    [Theory]
    [InlineData("en", "en")] // code passes through
    [InlineData("EN", "en")] // case-insensitive
    [InlineData("English", "en")] // full name maps to whisper code
    [InlineData("danish", "da")]
    [InlineData("pt-BR", "pt-br")] // unknown but code-shaped is kept as typed
    [InlineData("yue", "yue")]
    public void MapsCodesAndFullNames(string token, string expected)
    {
        Assert.Equal(expected, SpeechToTextViewModel.NormalizeFileNameLanguageCode(token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("pt/BR")] // path separator must never reach a file name
    [InlineData("en:us")]
    [InlineData("Portuguese (Brazil)")] // unmappable free text is dropped, not embedded
    [InlineData("simplified chinese")]
    public void DropsUnmappableTokens(string? token)
    {
        Assert.Null(SpeechToTextViewModel.NormalizeFileNameLanguageCode(token));
    }
}
