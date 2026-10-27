using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// nOCR splits the dots of a wide-set ellipsis into three words: "voor. . .", "<i>Hostile. . .</i>".
// The hardcoded rules join the three spaced dots anywhere on the line; the two-dot Tesseract
// forms (". ..", ".. .") stay with the replace lists, which only touch them at the end of a line.
public class OcrFixSpacedEllipsisTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixSpacedEllipsisTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrFixSpacedEllipsisTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        File.WriteAllText(Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"), "<ReplaceList></ReplaceList>");
    }

    [Theory]
    [InlineData("we staan er alleen voor. . .", "we staan er alleen voor...")]
    [InlineData("<i>Hostile. . .</i>", "<i>Hostile...</i>")]
    [InlineData("-Hostile. . .\n-Over.", "-Hostile...\n-Over.")]
    [InlineData(". . .en toen niets.", "...en toen niets.")]
    [InlineData("Wait. . . what?", "Wait... what?")]
    [InlineData("Wait. .. what?", "Wait. .. what?")] // Tesseract's two-dot form: replace lists only
    [InlineData("Groep 1. 2. 3.", "Groep 1. 2. 3.")] // dots with text between them are sentences
    public void FixOcrErrors_ThreeSpacedDots_BecomeEllipsis(string input, string expected)
    {
        IOcrFixEngine engine = new OcrFixEngine(new AcceptAllSpellChecker());
        engine.Initialize(new Subtitle(), "eng", new SpellCheckDictionaryDisplay());

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }

    [Fact]
    public void NormalizeSpacedEllipses_LeavesTextWithoutSpacedDotsAlone()
    {
        const string text = "Already fine... and 1.5 too.";

        Assert.Same(text, OcrFixEngine.NormalizeSpacedEllipses(text));
    }

    private sealed class AcceptAllSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        // "li" is refused so the slash-to-l rule does not rewrite the closing italic tag of a
        // probe line into "<li>" (that rule is fixed on its own in #16196).
        public bool IsWordCorrect(string word) => word != "li";
        public List<string> GetSuggestions(string word) => new();
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        try { Directory.Delete(_tempDictionariesFolder, true); } catch { /* best effort */ }
    }
}
