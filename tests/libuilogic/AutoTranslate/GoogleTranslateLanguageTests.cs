using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// The free endpoint and the paid Cloud Translation API do not speak the same set of languages, so
/// they no longer share one hardcoded list: the free one mirrors what translate_a/l reports, the
/// V2 one what Google documents for Cloud Translation.
/// </summary>
public class GoogleTranslateLanguageTests
{
    [Fact]
    public void V1_HasNoDuplicateCodes()
    {
        var codes = GoogleTranslateV1.GetTranslationPairs().Select(p => p.Code.ToLowerInvariant()).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void V2_HasNoDuplicateCodes()
    {
        var codes = new GoogleTranslateV2().GetSupportedTargetLanguages().Select(p => p.Code.ToLowerInvariant()).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void Languages_HaveANameAndACode()
    {
        Assert.All(GoogleTranslateV1.GetTranslationPairs(), p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.False(string.IsNullOrWhiteSpace(p.Code));
        });

        Assert.All(new GoogleTranslateV2().GetSupportedTargetLanguages(), p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.False(string.IsNullOrWhiteSpace(p.Code));
        });
    }

    /// <summary>"mni" is rejected with HTTP 400 - Google's code for Meiteilon is "mni-Mtei".</summary>
    [Fact]
    public void Meiteilon_UsesTheCodeGoogleAccepts()
    {
        foreach (var codes in new[]
                 {
                     GoogleTranslateV1.GetTranslationPairs().Select(p => p.Code).ToList(),
                     new GoogleTranslateV2().GetSupportedTargetLanguages().Select(p => p.Code).ToList(),
                 })
        {
            Assert.DoesNotContain("mni", codes);
            Assert.Contains(codes, c => c.Equals("mni-Mtei", StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// "romanji" is not a language code - Google answers it with the untranslated source text, so
    /// picking it produced a file that was never translated.
    /// </summary>
    [Fact]
    public void NoRomanjiPseudoLanguage()
    {
        Assert.DoesNotContain("romanji", GoogleTranslateV1.GetTranslationPairs().Select(p => p.Code));
        Assert.DoesNotContain("romanji", new GoogleTranslateV2().GetSupportedTargetLanguages().Select(p => p.Code));
    }

    /// <summary>Google lists these as "iw" and "zh-CN", but accepts SE's aliases - and users have them saved.</summary>
    [Theory]
    [InlineData("he")]
    [InlineData("zh")]
    public void KeepsTheAliasesSeHasAlwaysOffered(string code)
    {
        Assert.Contains(code, GoogleTranslateV1.GetTranslationPairs().Select(p => p.Code));
        Assert.Contains(code, new GoogleTranslateV2().GetSupportedTargetLanguages().Select(p => p.Code));
    }

    [Theory]
    [InlineData("oc")]        // Occitan
    [InlineData("pap")]       // Papiamento
    [InlineData("scn")]       // Sicilian
    [InlineData("tn")]        // Tswana
    [InlineData("crh")]       // Crimean Tatar
    [InlineData("fr-ca")]
    public void BothEnginesOfferTheLanguagesGoogleDocumentsForCloudToo(string code)
    {
        Assert.Contains(GoogleTranslateV1.GetTranslationPairs(), p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(new GoogleTranslateV2().GetSupportedTargetLanguages(), p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Languages the free endpoint translates but Cloud Translation does not document - offering
    /// them on the paid engine would just fail at request time.
    /// </summary>
    [Theory]
    [InlineData("wo")]        // Wolof
    [InlineData("bo")]        // Tibetan
    [InlineData("fo")]        // Faroese
    [InlineData("war")]       // Waray
    [InlineData("to")]        // Tongan
    public void OnlyTheFreeEngineOffersTheFreeOnlyLanguages(string code)
    {
        Assert.Contains(GoogleTranslateV1.GetTranslationPairs(), p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(new GoogleTranslateV2().GetSupportedTargetLanguages(), p => p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TheFreeListIsTheLargerOne()
    {
        Assert.True(GoogleTranslateV1.GetTranslationPairs().Count > new GoogleTranslateV2().GetSupportedTargetLanguages().Count);
    }
}
