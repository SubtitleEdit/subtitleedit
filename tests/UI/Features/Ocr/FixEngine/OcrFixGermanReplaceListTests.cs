using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Issue #13658: German OCR kept producing "SpitzeI", "lst" and "ln" (I/l confusion) even though
// deu_OCRFixReplaceList.xml has spell-check-gated regexes for exactly these errors. The shipped
// deu/nld/nor lists contain TWO RegularExpressionsIfSpelledCorrectly sections, and the loader
// only read the first one, so the whole second block was silently dead. The list here mirrors
// the shipped file's structure: a small first section plus the real rules in a second section.
public class OcrFixGermanReplaceListTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixGermanReplaceListTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixGermanReplaceListTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;

        // Same section layout and I/l rules as the shipped deu_OCRFixReplaceList.xml.
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "deu_OCRFixReplaceList.xml"),
            "<ReplaceList>" +
            "<RegularExpressionsIfSpelledCorrectly>" +
            "<RegEx find=\"\\bl([A-Z]+)\\b\" spellCheck=\"I$1\" replaceWith=\"I$1\" />" +
            "</RegularExpressionsIfSpelledCorrectly>" +
            "<RegularExpressionsIfSpelledCorrectly>" +
            "<RegEx find=\"\\b([a-zäöüß]+)I([a-zäöüß]+)\\b\" spellCheck=\"$1l$2\" replaceWith=\"$1l$2\" />" +
            "<RegEx find=\"\\b([A-ZÄÖÜ][a-zäöüß]+)I\\b\" spellCheck=\"$1l\" replaceWith=\"$1l\" />" +
            "<RegEx find=\"\\b([a-zäöüß]+)I\\b\" spellCheck=\"$1l\" replaceWith=\"$1l\" />" +
            "<RegEx find=\"\\b([A-ZÄÖÜa-zäöüß][a-zäöüß]*)II\\b\" spellCheck=\"$1ll\" replaceWith=\"$1ll\" />" +
            "<RegEx find=\"\\bl([a-zäöüß]+)\\b\" spellCheck=\"I$1\" replaceWith=\"I$1\" />" +
            "</RegularExpressionsIfSpelledCorrectly>" +
            "</ReplaceList>");
    }

    [Fact]
    public void FixOcrErrors_TrailingUppercaseIInCapitalizedWord_IsFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "Hängen da SpitzeI mit drin?", doTryToGuessUnknownWords: false);

        Assert.Equal("Hängen da Spitzel mit drin?", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_LeadingLowercaseLBeforeTwoChars_IsFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "lst da jemand?", doTryToGuessUnknownWords: false);

        Assert.Equal("Ist da jemand?", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_LeadingLowercaseLBeforeSingleChar_IsFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "Du? ln Paris?", doTryToGuessUnknownWords: false);

        Assert.Equal("Du? In Paris?", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_CorrectGermanWordStartingWithL_IsLeftAlone()
    {
        var engine = CreateEngine();

        // "lieben" is a real word - the leading l→I rule must not touch it even though the
        // line contains another error that makes the regex rules run.
        var result = engine.FixOcrErrors(0, "Wir lieben SpitzeI.", doTryToGuessUnknownWords: false);

        Assert.Equal("Wir lieben Spitzel.", result.GetText());
    }

    private static IOcrFixEngine CreateEngine()
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeGermanSpellChecker());
        engine.Initialize(new Subtitle(), "deu", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeGermanSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "hängen", "da", "mit", "drin", "ist", "in", "jemand", "du", "wir", "lieben",
            "Paris", "Spitzel",
        };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return true;
            }

            // Like Hunspell, accept the initial-capitalized form of a lowercase dictionary word.
            return Words.Contains(word) || Words.Contains(word.ToLowerInvariant());
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
