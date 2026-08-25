using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Macedonian version of the Cyrillic homoglyph rules, including the letters Latin script
// shares glyphs with only in Macedonian: j (ј) and s (ѕ). Ungated - a Latin letter with a
// Cyrillic neighbor is definitely wrong - so no dictionary is needed.
// Runs against the shipped Dictionaries/mkd_OCRFixReplaceList.xml, not a copy of its rules.
public class OcrFixMacedonianHomoglyphTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixMacedonianHomoglyphTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixMacedonianHomoglyphTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        File.Copy(
            Path.Combine(FindRepoRoot(), "Dictionaries", "mkd_OCRFixReplaceList.xml"),
            Path.Combine(_tempDictionariesFolder, "mkd_OCRFixReplaceList.xml"));

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
    [InlineData("Каj си?", "Кај си?")] // Latin j after Cyrillic
    [InlineData("Тоа е jасно.", "Тоа е јасно.")] // Latin j before Cyrillic
    [InlineData("Bо ред е.", "Во ред е.")] // Latin B
    [InlineData("Гледам sвезда.", "Гледам ѕвезда.")] // Latin s for dze
    public void FixOcrErrors_LatinHomoglyphInCyrillicWord_IsFixedWithoutADictionary(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_FullyLatinWord_IsLeftAlone()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "Ова е Windows.", doTryToGuessUnknownWords: false);

        Assert.Equal("Ова е Windows.", result.GetText());
    }

    private static IOcrFixEngine CreateEngine()
    {
        // No dictionary at all: the homoglyph rules must work stand-alone.
        IOcrFixEngine engine = new OcrFixEngine(new EmptySpellChecker());
        engine.Initialize(new Subtitle(), "mkd", new SpellCheckDictionaryDisplay());
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
