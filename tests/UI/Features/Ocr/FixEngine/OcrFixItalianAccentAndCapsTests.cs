using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Gated Italian rules: lost accents on the final vowel (citta -> città, perche -> perché,
// caffe -> caffè - both accents are tried, the dictionary picks the right one) and l/I
// confusion in all-caps captions. A rule is only applied when the misread form is NOT in the
// dictionary and the fixed form IS; lowercase-only classes keep proper names untouched.
// Runs against the shipped Dictionaries/ita_OCRFixReplaceList.xml, not a copy of its rules.
public class OcrFixItalianAccentAndCapsTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;
    private readonly bool _originalUseHardcodedRules;

    public OcrFixItalianAccentAndCapsTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        // The hardcoded rules fix some all-caps l/I words on their own; turn them off so these
        // tests prove the XML rules, not the hardcoded fallback.
        _originalUseHardcodedRules = Configuration.Settings.Tools.OcrFixUseHardcodedRules;
        Configuration.Settings.Tools.OcrFixUseHardcodedRules = false;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixItalianAccentTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        File.Copy(
            Path.Combine(FindRepoRoot(), "Dictionaries", "ita_OCRFixReplaceList.xml"),
            Path.Combine(_tempDictionariesFolder, "ita_OCRFixReplaceList.xml"));

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
    [InlineData("La citta e bella.", "La città e bella.")]
    [InlineData("Non so perche.", "Non so perché.")]
    [InlineData("Si fa cosi.", "Si fa così.")]
    [InlineData("Ne voglio di piu.", "Ne voglio di più.")]
    [InlineData("Prendo un caffe.", "Prendo un caffè.")]
    [InlineData("Va bene, pero non ora.", "Va bene, però non ora.")]
    public void FixOcrErrors_LostFinalVowelAccent_IsRestored(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("[MUSlCA]", "[MUSICA]")]
    [InlineData("FlNE", "FINE")]
    public void FixOcrErrors_MisreadLAndI_IsFixed(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Arrivo daRoma.", "Arrivo da Roma.")]
    public void FixOcrErrors_MissingSpaceAfterPreposition_IsSplitWhenTheNameIsKnown(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Vieni anche tu Zzyzx.")] // correct -e word is in the dictionary
    [InlineData("Maria canta Zzyzx.")] // capitalized name is never accented
    public void FixOcrErrors_CorrectWordsAndNames_AreLeftAlone(string text)
    {
        // "Zzyzx" keeps the line from being spelled OK, so every gated rule really runs.
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(text, result.GetText());
    }

    [Theory]
    [InlineData("E' un problema.", "È un problema.")] // straight apostrophe
    [InlineData("- E' vero!", "- È vero!")] // after a dialog dash
    [InlineData("E’ tardi.", "È tardi.")] // typographic apostrophe
    public void FixOcrErrors_CapitalEWithApostrophe_BecomesEGrave(string text, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("E 'sti soldi, Zzyzx?")] // the apostrophe belongs to the next word (elision)
    public void FixOcrErrors_CapitalEBeforeDetachedElision_IsLeftAlone(string text)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(text, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_WithoutADictionary_AccentRulesDoNothing()
    {
        var engine = CreateEngine(new EmptySpellChecker());

        var result = engine.FixOcrErrors(0, "La citta Zzyzx.", doTryToGuessUnknownWords: false);

        Assert.Equal("La citta Zzyzx.", result.GetText());
    }

    private static IOcrFixEngine CreateEngine(ISpellChecker? spellChecker = null)
    {
        IOcrFixEngine engine = new OcrFixEngine(spellChecker ?? new FakeItalianSpellChecker());
        engine.Initialize(new Subtitle(), "ita", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeItalianSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "arrivo", "roma",
            "la", "città", "e", "bella", "non", "so", "perché", "si", "fa", "così",
            "ne", "voglio", "di", "più", "prendo", "un", "caffè", "va", "bene", "però",
            "ora", "musica", "fine", "vieni", "anche", "tu", "canta",
            "è", "problema", "vero", "tardi",
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
