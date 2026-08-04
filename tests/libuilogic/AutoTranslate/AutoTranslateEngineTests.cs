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
        Assert.Equal("pt", languages.Single(p => p.Name == "Brazilian Portuguese").TwoLetterIsoLanguageName);
        // "ay" is Aymara; Awadhi has no two-letter code.
        Assert.Equal(string.Empty, languages.Single(p => p.Name == "Awadhi").TwoLetterIsoLanguageName);
    }

    [Fact]
    public void Perplexity_Url_IsNotTypoDomain()
    {
        Assert.Equal("https://www.perplexity.ai/", new PerplexityTranslate().Url);
    }
}
