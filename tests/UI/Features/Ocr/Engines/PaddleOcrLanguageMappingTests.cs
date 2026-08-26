using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Features.Ocr;

namespace UITests.Features.Ocr.Engines;

/// <summary>
/// Guards the Paddle OCR language dropdown against the two ways it can go wrong:
/// offering a language whose recognition/detection model is not on disk, and offering a
/// language that no script group claims - which silently OCRs it with the Latin model.
/// </summary>
public class PaddleOcrLanguageMappingTests
{
    // Model folders shipped in "PaddleOCR.PP-OCRv6.support.files" (standalone v3.7.0), which
    // is the only model source - nothing is downloaded per language. Listed from the unpacked
    // archive: the v6 bundle dropped the general-purpose PP-OCRv5 recognition models (mobile,
    // server and en_), so anything still asking for one of those points at a missing folder.
    private static readonly HashSet<string> BundledRecModels = new()
    {
        "PP-OCRv6_medium_rec",
        "PP-OCRv6_small_rec",
        "arabic_PP-OCRv5_mobile_rec",
        "cyrillic_PP-OCRv5_mobile_rec",
        "devanagari_PP-OCRv5_mobile_rec",
        "el_PP-OCRv5_mobile_rec",
        "eslav_PP-OCRv5_mobile_rec",
        "ka_PP-OCRv3_mobile_rec",
        "korean_PP-OCRv5_mobile_rec",
        "latin_PP-OCRv5_mobile_rec",
        "ta_PP-OCRv5_mobile_rec",
        "te_PP-OCRv5_mobile_rec",
        "th_PP-OCRv5_mobile_rec",
    };

    private static readonly HashSet<string> BundledDetModels = new()
    {
        "PP-OCRv3_mobile_det",
        "PP-OCRv5_mobile_det",
        "PP-OCRv5_server_det",
        "PP-OCRv6_medium_det",
        "PP-OCRv6_small_det",
    };

    public static TheoryData<string, string> LanguageAndMode()
    {
        var data = new TheoryData<string, string>();
        foreach (var language in PaddleOcr.GetLanguages())
        {
            data.Add(language.Code, "mobile");
            data.Add(language.Code, "server");
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(LanguageAndMode))]
    public void EveryOfferedLanguage_MapsToBundledModels(string code, string mode)
    {
        Assert.Contains(PaddleOcr.GetRecName(code, mode), BundledRecModels);
        Assert.Contains(PaddleOcr.GetDetectionName(code, mode), BundledDetModels);
    }

    [Theory]
    [MemberData(nameof(LanguageAndMode))]
    public void OnlyPali_UsesTheLatinPpOcrV5Model(string code, string mode)
    {
        // PP-OCRv6 recognizes every Latin language except Pali, so the PP-OCRv5 Latin model is
        // Pali's now - and still the fallback for a code no script group claims, which is what
        // makes this the test that catches such a code (it would report "latin" but not be pi).
        Assert.Equal(code == "pi", PaddleOcr.GetRecName(code, mode) == "latin_PP-OCRv5_mobile_rec");
    }

    [Theory]
    [MemberData(nameof(LanguageAndMode))]
    public void PpOcrV6Languages_UseTheUnifiedV6ModelPair(string code, string mode)
    {
        // _PPOCRV6_LANGS in PaddleOCR 3.7: Chinese, English, Japanese and the Latin languages
        // except Pali. Everything else has no v6 model at all and must stay on PP-OCRv5/v3.
        var isV6Language =
            code is "ch" or "chinese_cht" or "en" or "japan" ||
            (PaddleOcr.GetLatinLanguageCodesForTest().Contains(code) && code != "pi");

        // "mobile"/"server" is what the setting stores; v6 ships tiers instead.
        var tier = mode == "server" ? "medium" : "small";

        Assert.Equal(isV6Language, PaddleOcr.GetRecName(code, mode) == $"PP-OCRv6_{tier}_rec");
        Assert.Equal(isV6Language, PaddleOcr.GetDetectionName(code, mode) == $"PP-OCRv6_{tier}_det");
    }

    [Fact]
    public void OfferedLanguages_HaveNoDuplicateCodesOrNames()
    {
        var languages = PaddleOcr.GetLanguages();

        Assert.Empty(languages.GroupBy(p => p.Code).Where(g => g.Count() > 1).Select(g => g.Key));
        Assert.Empty(languages.GroupBy(p => p.Name).Where(g => g.Count() > 1).Select(g => g.Key));
    }

    [Fact]
    public void OfferedLanguages_CoverEveryLanguageInTheScriptGroups()
    {
        var offered = PaddleOcr.GetLanguages().Select(p => p.Code).ToHashSet();

        // PaddleOCR accepts both "fr"/"french" and "de"/"german" for French and German. The
        // dropdown offers the ISO code, so the two legacy aliases are expected to be absent
        // from it - NormalizeLanguageCode maps them onto the offered codes.
        var aliases = new[] { "french", "german" };
        var missing = PaddleOcr.GetAllScriptGroupCodesForTest()
            .Where(p => !aliases.Contains(p))
            .Where(p => !offered.Contains(p))
            .OrderBy(p => p)
            .ToList();

        Assert.Empty(missing);
    }

    [Theory]
    [InlineData("german", "de")]
    [InlineData("french", "fr")]
    public void LegacySavedCode_SelectsTheSameLanguage(string savedCode, string expectedCode)
    {
        var normalized = PaddleOcr.NormalizeLanguageCode(savedCode);

        Assert.Equal(expectedCode, normalized);
        Assert.Contains(normalized, PaddleOcr.GetLanguages().Select(p => p.Code));
    }

    [Theory]
    [InlineData("it")]
    [InlineData("en")]
    [InlineData("")]
    [InlineData(null)]
    public void NormalizeLanguageCode_LeavesEverythingElseAlone(string? code)
    {
        Assert.Equal(code ?? string.Empty, PaddleOcr.NormalizeLanguageCode(code));
    }
}
