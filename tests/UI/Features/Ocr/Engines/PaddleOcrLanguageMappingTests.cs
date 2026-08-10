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
    // Model folders shipped in "PaddleOCR.PP-OCRv5.support.files" (standalone v1.4.0),
    // which is the only model source - nothing is downloaded per language.
    private static readonly HashSet<string> BundledRecModels = new()
    {
        "PP-OCRv5_mobile_rec",
        "PP-OCRv5_server_rec",
        "arabic_PP-OCRv5_mobile_rec",
        "cyrillic_PP-OCRv5_mobile_rec",
        "devanagari_PP-OCRv5_mobile_rec",
        "el_PP-OCRv5_mobile_rec",
        "en_PP-OCRv5_mobile_rec",
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
    public void OnlyLatinLanguages_UseTheLatinModel(string code, string mode)
    {
        var isLatinCode = PaddleOcr.GetLatinLanguageCodesForTest().Contains(code);

        // A code missing from every script group falls through to the Latin model, so
        // "uses latin" and "is a Latin language" must be the same set.
        Assert.Equal(isLatinCode, PaddleOcr.GetRecName(code, mode) == "latin_PP-OCRv5_mobile_rec");
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
