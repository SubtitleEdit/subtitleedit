using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Issue #14885: nOCR reads the uppercase I of sans-serif fonts as lowercase l, so all-caps words
// come out as "MlSSlSSlPPl". The shipped English rules only repaired a single l per word
// ("SlNG"), and the word-level hardcoded rule is never applied. The rule under test replaces
// every l in an all-caps word at once and keeps the result only when the dictionary confirms it.
public class OcrFixLowercaseLInUppercaseWordTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixLowercaseLInUppercaseWordTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixLowercaseLInUppercaseWordTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;

        // The rule as shipped in eng_OCRFixReplaceList.xml.
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"),
            "<ReplaceList>" +
            "<RegularExpressionsIfSpelledCorrectly>" +
            "<RegEx find=\"\\b((?=[A-Zl]*[A-Z])(?=[A-Zl]*l)[A-Zl]{2,})\\b\" replaceAllFrom=\"l\" replaceAllTo=\"I\" spellCheck=\"$1\" replaceWith=\"$1\" />" +
            "</RegularExpressionsIfSpelledCorrectly>" +
            "</ReplaceList>");
    }

    [Fact]
    public void FixOcrErrors_SeveralLowercaseLsInAllCapsWord_AreAllFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "MlSSlSSlPPl", doTryToGuessUnknownWords: false);

        Assert.Equal("MISSISSIPPI", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_EveryAllCapsWordOnTheLine_IsFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "MlSSlSSlPPl lS A RlVER.", doTryToGuessUnknownWords: false);

        Assert.Equal("MISSISSIPPI IS A RIVER.", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_AllCapsWordInsideBrackets_IsFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "[GEARS GRlNDlNG]", doTryToGuessUnknownWords: false);

        Assert.Equal("[GEARS GRINDING]", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_UnknownAllCapsWord_IsLeftAlone()
    {
        var engine = CreateEngine();

        // "KILLIAN" is not in the dictionary, so the gate keeps the OCR text as is.
        var result = engine.FixOcrErrors(0, "KlLLlAN lS HERE", doTryToGuessUnknownWords: false);

        Assert.Equal("KlLLlAN IS HERE", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_CorrectlySpelledWordEndingInL_IsLeftAlone()
    {
        var engine = CreateEngine();

        // "Al" is a real word, so it must not become "AI" just because that is a word too.
        var result = engine.FixOcrErrors(0, "Al is here.", doTryToGuessUnknownWords: false);

        Assert.Equal("Al is here.", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_AllLowercaseLs_AreNotTouched()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "ll", doTryToGuessUnknownWords: false);

        Assert.Equal("ll", result.GetText());
    }

    private static IOcrFixEngine CreateEngine()
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeEnglishSpellChecker());
        engine.Initialize(new Subtitle(), "eng", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeEnglishSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "Mississippi", "is", "a", "river", "gears", "grinding", "here", "Al", "AI",
        };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return true;
            }

            // Like Hunspell, accept the capitalized and all-uppercase forms of a dictionary word.
            return Words.Contains(word) ||
                   Words.Contains(word.ToLowerInvariant()) ||
                   (word == word.ToUpperInvariant() && Words.Contains(char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
        }

        public List<string> GetSuggestions(string word) => new();
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        try
        {
            Directory.Delete(_tempDictionariesFolder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
