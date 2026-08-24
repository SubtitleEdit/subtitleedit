using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Gated Portuguese rules: lost tildes/accents on the final syllable (nao -> não,
// coracao -> coração, voce -> você) and l/I confusion in all-caps captions. A rule is only
// applied when the misread form is NOT in the dictionary and the fixed form IS; lowercase-only
// classes keep proper names (Joao as a name) untouched.
// Runs against the shipped Dictionaries/por_OCRFixReplaceList.xml, not a copy of its rules.
public class OcrFixPortugueseAccentAndCapsTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;
    private readonly bool _originalUseHardcodedRules;

    public OcrFixPortugueseAccentAndCapsTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        // The hardcoded rules fix some all-caps l/I words on their own; turn them off so these
        // tests prove the XML rules, not the hardcoded fallback.
        _originalUseHardcodedRules = Configuration.Settings.Tools.OcrFixUseHardcodedRules;
        Configuration.Settings.Tools.OcrFixUseHardcodedRules = false;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixPortugueseAccentTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        File.Copy(
            Path.Combine(FindRepoRoot(), "Dictionaries", "por_OCRFixReplaceList.xml"),
            Path.Combine(_tempDictionariesFolder, "por_OCRFixReplaceList.xml"));

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
    [InlineData("Eles sao irmaos.", "Eles são irmãos.")]
    [InlineData("Os avioes chegam.", "Os aviões chegam.")]
    [InlineData("Isso e necessario.", "Isso e necessário.")]
    [InlineData("Sabe que voce fala ingles.", "Sabe que você fala inglês.")]
    [InlineData("Quero um cafe.", "Quero um café.")]
    public void FixOcrErrors_LostFinalSyllableAccent_IsRestored(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("SlM", "SIM")]
    [InlineData("[GRlTOS]", "[GRITOS]")]
    public void FixOcrErrors_MisreadLAndI_IsFixed(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Joao chega logo Zzyzx.")] // capitalized name is never accented
    [InlineData("O cachorro late Zzyzx.")] // correct -e word is in the dictionary
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

        // "voce"/"ingles" have no ungated rules ("nao" would be fixed even without a dictionary,
        // by the ungated \b(n|N)ao\b regex), so this proves the gate really needs the dictionary.
        var result = engine.FixOcrErrors(0, "Sabe que voce fala ingles Zzyzx.", doTryToGuessUnknownWords: false);

        Assert.Equal("Sabe que voce fala ingles Zzyzx.", result.GetText());
    }

    private static IOcrFixEngine CreateEngine(ISpellChecker? spellChecker = null)
    {
        IOcrFixEngine engine = new OcrFixEngine(spellChecker ?? new FakePortugueseSpellChecker());
        engine.Initialize(new Subtitle(), "por", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakePortugueseSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "eles", "são", "irmãos", "os", "aviões", "chegam", "isso", "e", "necessário",
            "sabe", "que", "você", "fala", "inglês", "quero", "um", "café", "sim",
            "gritos", "chega", "logo", "o", "cachorro", "late",
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
