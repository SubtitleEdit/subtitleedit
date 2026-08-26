using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// The MADLAD combo used to be filled from .NET's culture list, which has nothing to do with what
/// the model knows: 107 of those languages had no control token in the model vocabulary and were
/// translated into Spanish without any error (discussion #12929 asked for Cantonese, which is one
/// of them). The list now mirrors the vocabulary of madlad400-3b-mt, so these tests guard the
/// properties that make it right.
/// </summary>
public class CrispAsrMadladLanguageTests
{
    [Fact]
    public void Languages_MatchTheModelVocabularyCount()
    {
        Assert.Equal(419, CrispAsrMadladLanguages.List().Count);
    }

    [Fact]
    public void Languages_HaveNoDuplicateCodes()
    {
        var codes = CrispAsrMadladLanguages.List().Select(p => p.Code).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void Languages_HaveANameACodeAndATwoLetterName()
    {
        Assert.All(CrispAsrMadladLanguages.List(), p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.False(string.IsNullOrWhiteSpace(p.Code));

            // Empty would reach the line-break and text-split rules, which match on it.
            Assert.False(string.IsNullOrWhiteSpace(p.TwoLetterIsoLanguageName));
        });
    }

    /// <summary>
    /// Only bare codes survive the trip through the CrispASR command line - a tag with a script or
    /// region suffix ("zh_Hant") lands in the same wrong-language fallback as an unknown code.
    /// </summary>
    [Fact]
    public void Codes_AreBareLowercaseIsoCodes()
    {
        Assert.All(CrispAsrMadladLanguages.List(), p => Assert.Matches(new Regex("^[a-z]{2,3}$"), p.Code));
    }

    [Fact]
    public void Languages_AreOrderedByName()
    {
        var names = CrispAsrMadladLanguages.List().Select(p => p.Name).ToList();

        Assert.Equal(names.OrderBy(n => n.ToLowerInvariant(), StringComparer.Ordinal), names);
    }

    /// <summary>
    /// The languages the culture list used to offer although the model has no token for them -
    /// picking any of these returned Spanish.
    /// </summary>
    [Theory]
    [InlineData("yue")] // Cantonese - discussion #12929
    [InlineData("nb")]  // Norwegian Bokmal - "no" is the code MADLAD knows
    [InlineData("ast")] // Asturian
    [InlineData("kab")] // Kabyle
    [InlineData("kok")] // Konkani
    [InlineData("ia")]  // Interlingua
    [InlineData("zgh")] // Standard Moroccan Tamazight
    public void UnsupportedLanguages_AreNotOffered(string code)
    {
        Assert.DoesNotContain(CrispAsrMadladLanguages.List(), p => p.Code == code);
        Assert.False(CrispAsrMadladLanguages.IsSupported(code));
    }

    /// <summary>
    /// Languages the model translates into but the culture list never offered.
    /// </summary>
    [Theory]
    [InlineData("no")]  // Norwegian
    [InlineData("ilo")] // Iloko
    [InlineData("war")] // Waray
    [InlineData("pap")] // Papiamento
    [InlineData("ary")] // Moroccan Arabic
    [InlineData("crh")] // Crimean Tatar
    [InlineData("min")] // Minangkabau
    [InlineData("wuu")] // Wu Chinese
    [InlineData("la")]  // Latin
    public void SupportedLanguages_AreOffered(string code)
    {
        Assert.Contains(CrispAsrMadladLanguages.List(), p => p.Code == code);
        Assert.True(CrispAsrMadladLanguages.IsSupported(code));
    }

    [Fact]
    public void CommonLanguages_SurviveTheRewrite()
    {
        var codes = CrispAsrMadladLanguages.List().Select(p => p.Code).ToList();

        Assert.Contains("en", codes);
        Assert.Contains("de", codes);
        Assert.Contains("es", codes);
        Assert.Contains("zh", codes);
        Assert.Contains("ja", codes);
        Assert.Contains("ar", codes);
    }

    [Fact]
    public void IsSupported_RejectsNothingAndNonsense()
    {
        Assert.False(CrispAsrMadladLanguages.IsSupported(string.Empty));
        Assert.False(CrispAsrMadladLanguages.IsSupported("zzz"));
        Assert.False(CrispAsrMadladLanguages.IsSupported("zh_Hant"));
        Assert.False(CrispAsrMadladLanguages.IsSupported("EN"));
    }

    [Fact]
    public void Engine_OffersTheSameListForSourceAndTarget()
    {
        var engine = new CrispAsrMadladTranslate();

        Assert.Equal(
            engine.GetSupportedTargetLanguages().Select(p => p.Code),
            engine.GetSupportedSourceLanguages().Select(p => p.Code));
    }
}
