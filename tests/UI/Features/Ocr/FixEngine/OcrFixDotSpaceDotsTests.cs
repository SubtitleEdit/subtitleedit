using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;

namespace UITests.Features.Ocr.FixEngine;

// Discussion #12929: Tesseract reads a trailing ellipsis as ". .." ("Wait. ..", "What the. ..").
// The English replace list turns a letter or digit + ". .." or ".. ." at the end of a line back into "...".
public class OcrFixDotSpaceDotsTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixDotSpaceDotsTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrFixDotSpaceDotsTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;

        // The rule as shipped in eng_OCRFixReplaceList.xml.
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"),
            "<ReplaceList><RegularExpressions>" +
            "<RegEx find=\"([\\p{L}\\d])(?:\\. \\.\\.|\\.\\. \\.)(?=\\r?$)\" replaceWith=\"$1...\" />" +
            "</RegularExpressions></ReplaceList>");
    }

    [Theory]
    [InlineData("Wait. ..", "Wait...")]
    [InlineData("What the. ..", "What the...")]
    [InlineData("-Kick-Ass. ..\r\n-Get him out of here.", "-Kick-Ass...\r\n-Get him out of here.")]
    [InlineData("-Kick-Ass. ..\n-Get him out of here.", "-Kick-Ass...\n-Get him out of here.")]
    [InlineData("Wait. .. what?", "Wait. .. what?")] // not at the end of the line - left alone
    [InlineData("3. ..", "3...")]
    [InlineData("3.. .", "3...")]
    [InlineData("Wait.. .", "Wait...")]
    [InlineData("Wait.. . what?", "Wait.. . what?")] // not at the end of the line - left alone
    [InlineData("?. ..", "?. ..")] // punctuation before it - left alone
    public void FixOcrErrors_LetterDotSpaceDots_BecomesEllipsis(string input, string expected)
    {
        IOcrFixEngine engine = new OcrFixEngine(new AcceptAllSpellChecker());
        engine.Initialize(new Subtitle(), "eng", new SpellCheckDictionaryDisplay());

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        // The engine re-joins lines with the platform newline, so compare newline-agnostically.
        Assert.Equal(expected.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }

    private sealed class AcceptAllSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
        public bool IsWordCorrect(string word) => true;
        public List<string> GetSuggestions(string word) => new();
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        try { Directory.Delete(_tempDictionariesFolder, true); } catch { /* best effort */ }
    }
}
