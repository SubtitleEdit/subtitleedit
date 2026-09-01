using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The AI OCR engines (llama.cpp, Ollama, Mistral) collapse the stray spaces their models
/// leave around punctuation. French and Breton typography require a real space before "?"
/// and "!", so an ungated " ?" -> "?" replacement silently damaged correct OCR output
/// ("n'est-ce pas ?" came back as "n'est-ce pas?"). Same guard as libse's
/// Utilities.FixOcrErrors.
/// </summary>
public class OcrHelperPunctuationTests
{
    [Theory]
    [InlineData("French")]
    [InlineData("french")]
    [InlineData("fr")]
    [InlineData("fra")]
    [InlineData("Breton")]
    [InlineData("br")]
    public void KeepsSpaceBeforeQuestionMarkForFrenchAndBreton(string language)
    {
        Assert.Equal("n'est-ce pas ?", OcrHelper.FixAiOcrPunctuationSpaces("n'est-ce pas ?", language));
    }

    [Theory]
    [InlineData("French")]
    [InlineData("fr")]
    [InlineData("Breton")]
    [InlineData("br")]
    public void KeepsSpaceBeforeExclamationMarkForFrenchAndBreton(string language)
    {
        Assert.Equal("Bonjour !", OcrHelper.FixAiOcrPunctuationSpaces("Bonjour !", language));
    }

    [Theory]
    [InlineData("English")]
    [InlineData("en")]
    [InlineData("German")]
    [InlineData("")]
    [InlineData(null)]
    public void RemovesSpaceBeforeQuestionAndExclamationMarkForOtherLanguages(string? language)
    {
        Assert.Equal("Really? Yes!", OcrHelper.FixAiOcrPunctuationSpaces("Really ? Yes !", language));
    }

    [Fact]
    public void StillRemovesSpaceBeforeCommaAndPeriodForFrench()
    {
        // French puts no space before "," or "." - only before "?", "!", ":" and ";".
        Assert.Equal("Oui, bien sûr.", OcrHelper.FixAiOcrPunctuationSpaces("Oui , bien sûr .", "French"));
    }

    [Fact]
    public void FixesParenthesesAndEscapedQuotesForFrench()
    {
        Assert.Equal("(un \"mot\") ?", OcrHelper.FixAiOcrPunctuationSpaces("( un \\\"mot\\\" ) ?", "French"));
    }

    [Fact]
    public void TrimsTrailingApostropheAfterExclamationMark()
    {
        Assert.Equal("Stop!", OcrHelper.FixAiOcrPunctuationSpaces("Stop!'", "English"));
    }

    [Fact]
    public void HandlesNullAndEmptyInput()
    {
        Assert.Equal(string.Empty, OcrHelper.FixAiOcrPunctuationSpaces(null, "French"));
        Assert.Equal(string.Empty, OcrHelper.FixAiOcrPunctuationSpaces(string.Empty, "French"));
    }

    [Theory]
    [InlineData("French", true)]
    [InlineData("fr", true)]
    [InlineData("Breton", true)]
    [InlineData("br", true)]
    [InlineData("English", false)]
    [InlineData("en", false)]
    [InlineData("Frisian", false)]
    [InlineData(null, false)]
    public void UsesSpaceBeforeQuestionAndExclamationMarkMapsLanguages(string? language, bool expected)
    {
        Assert.Equal(expected, OcrHelper.UsesSpaceBeforeQuestionAndExclamationMark(language));
    }
}
