using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

// Batch OCR used one language for every file. The tracks of one disc come in several languages
// ("movie [eng].sup", "movie [fra].sup"), so the language the source declares now wins.
public class BatchOcrLanguageTests : IDisposable
{
    private readonly string _modelFolder;
    private static readonly string[] PaddleCodes = { "en", "fr", "it", "ro" };
    private static readonly string[] AppleCodes = { "en-US", "en-GB", "fr-FR", "it-IT" };

    public BatchOcrLanguageTests()
    {
        _modelFolder = Path.Combine(Path.GetTempPath(), "se-batch-ocr-language-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_modelFolder);
        File.WriteAllText(Path.Combine(_modelFolder, "eng.traineddata"), string.Empty);
        File.WriteAllText(Path.Combine(_modelFolder, "fra.traineddata"), string.Empty);
    }

    public void Dispose()
    {
        Directory.Delete(_modelFolder, true);
    }

    [Theory]
    [InlineData("Movie [fra].sup", "fr")]
    [InlineData("Movie [ron].sup", "ro")]
    [InlineData("Movie [rum].sup", "ro")]
    [InlineData("movie.it.sup", "it")]
    public void SourceLanguage_FromFileNameTag(string fileName, string expectedTwoLetter)
    {
        var item = new BatchConvertItem { FileName = Path.Combine("disc", fileName) };

        Assert.Equal(expectedTwoLetter, BatchOcrLanguage.ResolveSourceLanguage(item)?.TwoLetterCode);
    }

    [Fact]
    public void SourceLanguage_NoTag_IsNull()
    {
        var item = new BatchConvertItem { FileName = Path.Combine("disc", "Movie.sup") };

        Assert.Null(BatchOcrLanguage.ResolveSourceLanguage(item));
    }

    [Fact]
    public void SourceLanguage_TrackLanguageWinsOverFileName()
    {
        var item = new BatchConvertItem { FileName = Path.Combine("disc", "movie.en.mkv"), LanguageCode = "fre" };

        Assert.Equal("fr", BatchOcrLanguage.ResolveSourceLanguage(item)?.TwoLetterCode);
    }

    [Fact]
    public void SourceLanguage_UndeterminedTrack_DoesNotFallBackToTheContainerFileName()
    {
        var item = new BatchConvertItem { FileName = Path.Combine("disc", "movie.en.mkv"), LanguageCode = "und" };

        Assert.Null(BatchOcrLanguage.ResolveSourceLanguage(item));
    }

    [Fact]
    public void SourceLanguage_SingleIdxLanguage_IsUsed_SeveralAreNot()
    {
        var item = new BatchConvertItem { FileName = Path.Combine("disc", "movie.sub") };

        Assert.Equal("it", BatchOcrLanguage.ResolveSourceLanguage(item, new[] { "it" })?.TwoLetterCode);
        Assert.Null(BatchOcrLanguage.ResolveSourceLanguage(item, new[] { "it", "en" }));
    }

    [Fact]
    public void Tesseract_UsesSourceLanguage_OnlyWhenTheModelIsInstalled()
    {
        Assert.Equal("fra", BatchOcrLanguage.ForTesseract(OcrViewModel.ResolveIsoLanguage("fr"), "eng", _modelFolder));
        Assert.Equal("eng", BatchOcrLanguage.ForTesseract(OcrViewModel.ResolveIsoLanguage("it"), "eng", _modelFolder));
        Assert.Equal("eng", BatchOcrLanguage.ForTesseract(null, "eng", _modelFolder));
    }

    [Fact]
    public void TwoLetterEngine_UsesSourceLanguage_OnlyWhenListed()
    {
        Assert.Equal("ro", BatchOcrLanguage.ForTwoLetterEngine(OcrViewModel.ResolveIsoLanguage("ron"), "en", PaddleCodes));
        Assert.Equal("en", BatchOcrLanguage.ForTwoLetterEngine(OcrViewModel.ResolveIsoLanguage("da"), "en", PaddleCodes));
        Assert.Equal("en", BatchOcrLanguage.ForTwoLetterEngine(null, "en", PaddleCodes));
    }

    [Fact]
    public void Bcp47Engine_KeepsTheConfiguredRegionOfTheSameLanguage()
    {
        Assert.Equal("fr-FR", BatchOcrLanguage.ForBcp47Engine(OcrViewModel.ResolveIsoLanguage("fr"), "en-GB", AppleCodes));
        Assert.Equal("en-GB", BatchOcrLanguage.ForBcp47Engine(OcrViewModel.ResolveIsoLanguage("en"), "en-GB", AppleCodes));
        Assert.Equal("en-GB", BatchOcrLanguage.ForBcp47Engine(OcrViewModel.ResolveIsoLanguage("da"), "en-GB", AppleCodes));
    }

    [Fact]
    public void LanguageNameEngine_UsesTheEnglishName()
    {
        Assert.Equal("Italian", BatchOcrLanguage.ForLanguageNameEngine(OcrViewModel.ResolveIsoLanguage("ita"), "English"));
        Assert.Equal("English", BatchOcrLanguage.ForLanguageNameEngine(null, "English"));
    }
}
