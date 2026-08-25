using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// OCR of Cyrillic text often emits Latin homoglyphs (a/c/e/o/p/x/y, B/H/K...) or digits (3/6/0)
// inside Cyrillic words. A Latin letter with a Cyrillic neighbor is definitely wrong, so the
// rules are ungated and work without any dictionary; fully Latin words (IKEA, YouTube) have no
// Cyrillic neighbors and are never touched.
// Runs against the shipped Dictionaries/rus_OCRFixReplaceList.xml, not a copy of its rules.
public class OcrFixRussianHomoglyphTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixRussianHomoglyphTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixRussianHomoglyphTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        File.Copy(
            Path.Combine(FindRepoRoot(), "Dictionaries", "rus_OCRFixReplaceList.xml"),
            Path.Combine(_tempDictionariesFolder, "rus_OCRFixReplaceList.xml"));

        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Dictionaries")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root");
    }

    [Theory]
    [InlineData("Bот нaш дом.", "Вот наш дом.")] // Latin B, Latin a
    [InlineData("Мы xотим есть.", "Мы хотим есть.")] // Latin x
    [InlineData("Yтро доброе.", "Утро доброе.")] // Latin Y at word start
    [InlineData("Пошли на о6ед.", "Пошли на обед.")] // digit 6
    [InlineData("Это отка3.", "Это отказ.")] // digit 3 at word end
    [InlineData("Ему с0рок лет.", "Ему сорок лет.")] // digit 0
    public void FixOcrErrors_LatinHomoglyphInCyrillicWord_IsFixedWithoutADictionary(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Это IKEA.")] // fully Latin word has no Cyrillic neighbors
    [InlineData("Смотрю YouTube.")]
    [InlineData("У меня 6 книг.")] // free-standing digit
    [InlineData("Вес 16кг.")] // digit after digit is left alone
    public void FixOcrErrors_LatinWordsAndRealDigits_AreLeftAlone(string text)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(text, result.GetText());
    }

    private static IOcrFixEngine CreateEngine()
    {
        // No dictionary at all: the homoglyph rules must work stand-alone.
        IOcrFixEngine engine = new OcrFixEngine(new EmptySpellChecker());
        engine.Initialize(new Subtitle(), "rus", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class EmptySpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => false;

        public bool IsWordCorrect(string word) => string.IsNullOrWhiteSpace(word);

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
