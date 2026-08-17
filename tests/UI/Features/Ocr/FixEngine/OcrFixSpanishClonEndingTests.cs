using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Issue #13701: the Spanish list fixed the OCR error "clón" -> "ción" with a plain regex that
// matched ANY word ending in "clón", so real words were destroyed ("ciclón" -> "cición",
// "anticiclón" -> "anticición"). The general case now lives in the spell-check-gated section
// (a word that is already in the dictionary is never "fixed"), and the ungated regex is limited
// to "-cclón"/"-uclón", endings no real Spanish word has.
// Runs against the shipped Dictionaries/spa_OCRFixReplaceList.xml, not a copy of its rules.
public class OcrFixSpanishClonEndingTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixSpanishClonEndingTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixSpanishClonTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        File.Copy(
            Path.Combine(FindRepoRoot(), "Dictionaries", "spa_OCRFixReplaceList.xml"),
            Path.Combine(_tempDictionariesFolder, "spa_OCRFixReplaceList.xml"));

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
    [InlineData("Se acerca el ciclón Zzyzx.")]
    [InlineData("Se acerca el anticiclón Zzyzx.")]
    public void FixOcrErrors_WordThatReallyEndsInClon_IsLeftAlone(string text)
    {
        // "Zzyzx" keeps the line from being spelled OK, so every regex rule really runs.
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(text, result.GetText());
    }

    [Theory]
    [InlineData("Vamos a la estaclón.", "Vamos a la estación.")]
    [InlineData("Canta una canclón.", "Canta una canción.")]
    public void FixOcrErrors_MisreadCionEnding_IsFixed(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Entra en acclón Zzyzx.", "Entra en acción Zzyzx.")]
    [InlineData("Busca una soluclón Zzyzx.", "Busca una solución Zzyzx.")]
    public void FixOcrErrors_CcionAndUcionEndings_AreFixedWithoutADictionary(string text, string expected)
    {
        // No Spanish Hunspell dictionary is installed in this scenario, so the gated rules are
        // dead - the two endings that no real word uses must still be repaired.
        var engine = CreateEngine(new EmptySpellChecker());

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_WithoutADictionary_RealClonWordIsLeftAlone()
    {
        var engine = CreateEngine(new EmptySpellChecker());

        var result = engine.FixOcrErrors(0, "Se acerca el ciclón.", doTryToGuessUnknownWords: false);

        Assert.Equal("Se acerca el ciclón.", result.GetText());
    }

    private static IOcrFixEngine CreateEngine(ISpellChecker? spellChecker = null)
    {
        IOcrFixEngine engine = new OcrFixEngine(spellChecker ?? new FakeSpanishSpellChecker());
        engine.Initialize(new Subtitle(), "spa", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeSpanishSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "se", "acerca", "el", "ciclón", "anticiclón", "vamos", "a", "la", "estación",
            "canta", "una", "canción", "entra", "en", "acción", "busca", "solución",
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

    // Stands in for "no Hunspell dictionary installed for the language".
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
