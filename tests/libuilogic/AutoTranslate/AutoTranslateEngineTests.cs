using System.Text.Json;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

public class AutoTranslateEngineTests
{
    [Fact]
    public void LibreTranslate_RequestBody_WithApiKey_IsValidJson()
    {
        var body = LibreTranslate.MakeRequestBody("Hello \"world\"", "en", "fr", "my-secret-key");

        using var doc = JsonDocument.Parse(body);
        Assert.Equal("Hello \"world\"", doc.RootElement.GetProperty("q").GetString());
        Assert.Equal("en", doc.RootElement.GetProperty("source").GetString());
        Assert.Equal("fr", doc.RootElement.GetProperty("target").GetString());
        Assert.Equal("my-secret-key", doc.RootElement.GetProperty("api_key").GetString());
    }

    [Fact]
    public void LibreTranslate_RequestBody_WithoutApiKey_IsValidJsonWithoutKey()
    {
        var body = LibreTranslate.MakeRequestBody("Hello", "en", "fr", string.Empty);

        using var doc = JsonDocument.Parse(body);
        Assert.Equal("Hello", doc.RootElement.GetProperty("q").GetString());
        Assert.False(doc.RootElement.TryGetProperty("api_key", out _));
    }

    [Fact]
    public void MyMemory_Url_SendsApiKeyWhenSet()
    {
        var url = MyMemoryApi.MakeUrl("Hello", "en", "da", "my key+1");

        Assert.Contains("&key=my%20key%2b1", url, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("langpair=en|da", url);
    }

    [Fact]
    public void MyMemory_Url_OmitsApiKeyWhenEmpty()
    {
        var url = MyMemoryApi.MakeUrl("Hello", "en", "da", string.Empty);

        Assert.DoesNotContain("key=", url);
    }

    [Fact]
    public void Gemini_LanguageCodes_AreCorrectIsoCodes()
    {
        var languages = new GeminiTranslate().GetSupportedTargetLanguages();

        // "iw" is the legacy Hebrew code; SE detects "he", so "iw" breaks auto-selection.
        Assert.Equal("he", languages.Single(p => p.Name == "Hebrew").TwoLetterIsoLanguageName);
        // "br" is Breton, not Brazilian Portuguese.
        Assert.Equal("pt-BR", languages.Single(p => p.Name == "Brazilian Portuguese").TwoLetterIsoLanguageName);
        // "ay" is Aymara, not Awadhi.
        Assert.Equal("awa", languages.Single(p => p.Name == "Awadhi").TwoLetterIsoLanguageName);
    }

    [Fact]
    public void Gemini_UsesSharedLlmLanguageList()
    {
        var gemini = new GeminiTranslate().GetSupportedTargetLanguages().Select(p => p.Name);

        Assert.Equal(ChatGptTranslate.ListLanguages().Select(p => p.Name), gemini);
    }

    [Theory]
    [InlineData("Malayalam", "ml")] // #14963
    [InlineData("Tamil", "ta")]
    [InlineData("Telugu", "te")]
    [InlineData("Filipino", "tl")]
    [InlineData("Lao", "lo")]
    [InlineData("Zulu", "zu")]
    public void LlmLanguageList_ContainsMajorLanguages(string name, string isoCode)
    {
        var language = ChatGptTranslate.ListLanguages().Single(p => p.Name == name);

        Assert.Equal(name, language.Code);
        Assert.Equal(isoCode, language.TwoLetterIsoLanguageName);
    }

    [Fact]
    public void LlmLanguageList_HasNoDuplicates()
    {
        var languages = ChatGptTranslate.ListLanguages();

        Assert.Equal(languages.Count, languages.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(languages.Count, languages.Select(p => p.TwoLetterIsoLanguageName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Perplexity_Url_IsNotTypoDomain()
    {
        Assert.Equal("https://www.perplexity.ai/", new PerplexityTranslate().Url);
    }
}
