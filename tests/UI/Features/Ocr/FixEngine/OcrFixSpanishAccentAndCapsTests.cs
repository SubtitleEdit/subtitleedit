using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// The Spanish list only enumerated hand-picked accent fixes ("dia" -> "día") as WholeWords, so
// every word not on the list slipped through. The gated rules generalize the pattern: a final
// syllable accent (cancion -> canción) or an l/I confusion is only applied when the misread form
// is NOT in the dictionary and the fixed form IS. Lowercase-only classes keep proper names
// (Mia, Sofia) untouched.
// Runs against the shipped Dictionaries/spa_OCRFixReplaceList.xml, not a copy of its rules.
public class OcrFixSpanishAccentAndCapsTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;
    private readonly bool _originalUseHardcodedRules;

    public OcrFixSpanishAccentAndCapsTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        // The hardcoded rules fix some all-caps l/I words on their own; turn them off so these
        // tests prove the XML rules, not the hardcoded fallback.
        _originalUseHardcodedRules = Configuration.Settings.Tools.OcrFixUseHardcodedRules;
        Configuration.Settings.Tools.OcrFixUseHardcodedRules = false;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixSpanishAccentTest_" + Guid.NewGuid().ToString("N"));
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
    [InlineData("Un monton de cosas.", "Un montón de cosas.")]
    [InlineData("El capitan llega.", "El capitán llega.")]
    [InlineData("Hay un jardin.", "Hay un jardín.")]
    [InlineData("Vendra quizas.", "Vendra quizás.")]
    [InlineData("Ella vivio sola.", "Ella vivió sola.")]
    [InlineData("Toma un cafe.", "Toma un café.")]
    public void FixOcrErrors_LostFinalSyllableAccent_IsRestored(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("SlN SALlDA", "SIN SALIDA")]
    [InlineData("[GRlTOS]", "[GRITOS]")]
    [InlineData("Vamos a Iavar la ropa.", "Vamos a lavar la ropa.")]
    public void FixOcrErrors_MisreadLAndI_IsFixed(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Mia canta bien Zzyzx.")] // capitalized name, even though "mía" is a word
    [InlineData("La familia llega Zzyzx.")] // correct -ia word is in the dictionary
    [InlineData("Mi amigo llega Zzyzx.")] // correct -o word is in the dictionary
    public void FixOcrErrors_CorrectWordsAndNames_AreLeftAlone(string text)
    {
        // "Zzyzx" keeps the line from being spelled OK, so every gated rule really runs.
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(text, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_WithoutADictionary_AccentRulesDoNothing()
    {
        var engine = CreateEngine(new EmptySpellChecker());

        // "capitan" has no ungated rule ("cancion" would be fixed even without a dictionary,
        // by the ungated ([sc]i)o(n) regex), so this proves the gate really needs the dictionary.
        var result = engine.FixOcrErrors(0, "El capitan llega Zzyzx.", doTryToGuessUnknownWords: false);

        Assert.Equal("El capitan llega Zzyzx.", result.GetText());
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
            "un", "montón", "de", "cosas", "el", "capitán", "llega", "hay", "jardín",
            "quizás", "ella", "vivió", "sola", "toma", "café", "sin", "salida", "gritos",
            "vamos", "a", "lavar", "la", "ropa", "mía", "canta", "bien", "familia", "mi", "amigo",
        };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return true;
            }

            // Like Hunspell, accept capitalized/all-caps forms of a lowercase dictionary word.
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
        Configuration.Settings.Tools.OcrFixUseHardcodedRules = _originalUseHardcodedRules;
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
