using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

public class DeepLTranslateTests
{
    /// <summary>
    /// A ":fx" key is free-tier and works only on api-free.deepl.com; a key without it works only
    /// on api.deepl.com. The wrong pairing answers 403 "Wrong endpoint", so the host follows the key.
    /// </summary>
    [Theory]
    [InlineData("https://api.deepl.com/", "abc-123:fx", "https://api-free.deepl.com/")]
    [InlineData("https://api-free.deepl.com/", "abc-123", "https://api.deepl.com/")]
    [InlineData("https://api-free.deepl.com/", "abc-123:fx", "https://api-free.deepl.com/")]
    [InlineData("https://api.deepl.com/", "abc-123", "https://api.deepl.com/")]
    public void ResolveApiUrl_PicksTheHostThatMatchesTheKey(string url, string key, string expected)
    {
        Assert.Equal(expected, DeepLTranslate.ResolveApiUrl(url, key));
    }

    [Theory]
    [InlineData("http://localhost:1188/", "abc-123:fx")]
    [InlineData("https://deepl.example.com/v2/", "abc-123")]
    public void ResolveApiUrl_LeavesAnyOtherHostAlone(string url, string key)
    {
        Assert.Equal(url, DeepLTranslate.ResolveApiUrl(url, key));
    }

    [Theory]
    [InlineData("", "abc-123:fx")]
    [InlineData("https://api.deepl.com/", "")]
    public void ResolveApiUrl_WithoutBothPartsChangesNothing(string url, string key)
    {
        Assert.Equal(url, DeepLTranslate.ResolveApiUrl(url, key));
    }

    /// <summary>
    /// The lists mirror GET /v2/languages (checked 2026-08-25). These guard the shape rather than
    /// the exact contents: no duplicates, nothing empty, and the languages the reporter of #14092
    /// and others asked about are actually offered.
    /// </summary>
    [Fact]
    public void TargetLanguages_HaveNoDuplicateCodes()
    {
        var codes = new DeepLTranslate().GetSupportedTargetLanguages().Select(p => p.Code.ToLowerInvariant()).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void SourceLanguages_HaveNoDuplicateCodes()
    {
        var codes = new DeepLTranslate().GetSupportedSourceLanguages().Select(p => p.Code.ToLowerInvariant()).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void Languages_HaveANameAndACode()
    {
        var deepL = new DeepLTranslate();

        Assert.All(deepL.GetSupportedSourceLanguages(), p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.False(string.IsNullOrWhiteSpace(p.Code));
        });

        Assert.All(deepL.GetSupportedTargetLanguages(), p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.False(string.IsNullOrWhiteSpace(p.Code));
        });
    }

    [Theory]
    [InlineData("hi")]  // Hindi
    [InlineData("bn")]  // Bengali
    [InlineData("ta")]  // Tamil
    [InlineData("fa")]  // Persian
    [InlineData("sr")]  // Serbian
    [InlineData("hr")]  // Croatian
    [InlineData("ms")]  // Malay
    [InlineData("sw")]  // Swahili
    [InlineData("is")]  // Icelandic
    [InlineData("de-CH")]
    [InlineData("fr-CA")]
    public void TargetLanguages_IncludeTheLanguagesDeepLAdded(string code)
    {
        var codes = new DeepLTranslate().GetSupportedTargetLanguages().Select(p => p.Code).ToList();

        Assert.Contains(code, codes);
    }

    /// <summary>DeepL only accepts a formality setting for these targets - supports_formality in the API.</summary>
    [Fact]
    public void OnlyTheDocumentedTargetsSupportFormality()
    {
        var withFormality = new DeepLTranslate().GetSupportedTargetLanguages()
            .Where(p => p.HasFormality == true)
            .Select(p => p.Code)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            new[] { "de", "de-CH", "es", "es-419", "fr", "fr-CA", "it", "ja", "nl", "pl", "pt-BR", "pt-PT", "ru" }
                .OrderBy(p => p, StringComparer.Ordinal).ToList(),
            withFormality);
    }

    /// <summary>
    /// The two lists come from one shared list: targets are exactly the sources plus the two
    /// regional variants DeepL only accepts as targets (fr-CA and de-CH), each right after its
    /// base language. This guards a future refresh from letting the lists silently diverge.
    /// </summary>
    [Fact]
    public void TargetLanguages_AreTheSourceLanguagesPlusTheTwoRegionalTargets()
    {
        var deepL = new DeepLTranslate();
        var sources = deepL.GetSupportedSourceLanguages();
        var targets = deepL.GetSupportedTargetLanguages();

        var extras = targets.Where(t => sources.All(s => s.Code != t.Code)).Select(t => t.Code).ToList();
        Assert.Equal(new[] { "fr-CA", "de-CH" }, extras);

        // With the two extras removed, the target list is the source list - same entries, same
        // order, same formality flags.
        var targetsWithoutExtras = targets.Where(t => t.Code != "fr-CA" && t.Code != "de-CH").ToList();
        Assert.Equal(
            sources.Select(p => (p.Name, p.Code, p.HasFormality)).ToList(),
            targetsWithoutExtras.Select(p => (p.Name, p.Code, p.HasFormality)).ToList());
    }

    /// <summary>
    /// DeepL takes no regional variant as a source language, so the source codes SE offers are cut
    /// back to the base code before they are sent.
    /// </summary>
    [Fact]
    public void SourceLanguages_KeepTheRegionalPairsSeHasAlwaysOffered()
    {
        var codes = new DeepLTranslate().GetSupportedSourceLanguages().Select(p => p.Code).ToList();

        Assert.Contains("en-GB", codes);
        Assert.Contains("en-US", codes);
        Assert.Contains("zh-hans", codes);
    }
}
