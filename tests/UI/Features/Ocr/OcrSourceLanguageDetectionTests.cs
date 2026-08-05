using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Ocr;

namespace UITests.Features.Ocr;

public class OcrSourceLanguageDetectionTests
{
    [Theory]
    [InlineData(@"C:\Video\kongekabale.2004_track5_[dut].sub", "dut")] // SE's own mkv track export naming (#13116)
    [InlineData("/home/user/movie_track3_[fre].sup", "fre")]
    [InlineData("movie_[xx]_track2_[nld].sub", "nld")] // last valid bracketed token wins
    [InlineData("movie.nl.sub", "nl")]
    [InlineData("movie.NL.sub", "NL")] // two-letter tags may be upper case
    [InlineData("movie.dut.forced.sub", "dut")] // skips the "forced" marker
    [InlineData("movie.en.hi.sub", "en")] // "hi" is hearing-impaired, not Hindi
    [InlineData("movie.deu.sup", "deu")]
    public void DetectLanguageCodeFromFileName_FindsLanguage(string fileName, string expected)
    {
        Assert.Equal(expected, OcrViewModel.DetectLanguageCodeFromFileName(fileName));
    }

    [Theory]
    [InlineData("movie.sub")]
    [InlineData("")]
    [InlineData("movie_[cd1].sub")] // bracketed token that is not a language
    [InlineData("Big.Ben.sub")] // capitalized three-letter title word is not a language tag
    [InlineData("movie.hi.sub")] // lone "hi" reads as hearing-impaired, so no detection
    [InlineData("The.Movie.2004.sub")]
    public void DetectLanguageCodeFromFileName_NoLanguage(string fileName)
    {
        Assert.Null(OcrViewModel.DetectLanguageCodeFromFileName(fileName));
    }

    [Theory]
    [InlineData("nl", "Dutch")]
    [InlineData("nld", "Dutch")]
    [InlineData("dut", "Dutch")] // ISO 639-2/B (bibliographic) form, as used by Matroska
    [InlineData("nl-NL", "Dutch")] // region subtag is stripped
    [InlineData("pt_BR", "Portuguese")]
    [InlineData("GER", "German")]
    public void ResolveIsoLanguage_Resolves(string code, string expectedEnglishName)
    {
        var iso = OcrViewModel.ResolveIsoLanguage(code);
        Assert.NotNull(iso);
        Assert.Equal(expectedEnglishName, iso!.EnglishName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("und")]
    [InlineData("xxxx")]
    public void ResolveIsoLanguage_Unknown(string? code)
    {
        Assert.Null(OcrViewModel.ResolveIsoLanguage(code));
    }

    [Fact]
    public void Idx_ParsesLanguageCodes_ParallelToLanguages()
    {
        var idx = new Idx(new List<string>
        {
            "id: en, index: 0",
            "id: nl, index: 1",
        });

        Assert.Equal(2, idx.Languages.Count);
        Assert.Equal(new List<string> { "en", "nl" }, idx.LanguageCodes);
        Assert.Contains("(0x20)", idx.Languages[0]);
        Assert.Contains("(0x21)", idx.Languages[1]);
    }
}
