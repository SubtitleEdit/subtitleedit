using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// The spell-check-gated OCR regexes (RegularExpressionsIfSpelledCorrectly) only verified that the
// FIXED form was a dictionary word, never that the original already was one. With the Dutch list's
// rule "\b([A-Z]+)l\b" → "$1I", the correct word "Al" was rewritten to "AI" ("Al die regels..." →
// "AI die regels...") because "AI" is also in the dictionary. A word that is already spelled
// correctly must never be "fixed".
public class OcrFixSpellCheckRegexTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixSpellCheckRegexTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixSpellCheckRegexTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;

        // Same trailing-l rule as shipped in nld/eng/dan _OCRFixReplaceList.xml.
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "nld_OCRFixReplaceList.xml"),
            "<ReplaceList><RegularExpressionsIfSpelledCorrectly>" +
            "<RegEx find=\"\\b([A-Z]+)l\\b\" spellCheck=\"$1I\" replaceWith=\"$1I\" />" +
            "</RegularExpressionsIfSpelledCorrectly></ReplaceList>");
    }

    [Fact]
    public void FixOcrErrors_DoesNotChangeCorrectWordToOtherDictionaryWord()
    {
        var engine = CreateEngine();

        // "Zzyzx" makes the line fail the whole-line spell check, so the regex rules run.
        var text = "Al die regels en kamers" + Environment.NewLine + "waar we niet mogen komen, Zzyzx.";
        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(text, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_MultiLineTextWithOnlyCorrectWords_IsLeftAlone()
    {
        var engine = CreateEngine();

        // No misspelled words at all - the whole-line gate must treat the line break as a
        // word separator and skip the regex rules entirely.
        var text = "Al die regels en kamers" + Environment.NewLine + "waar we niet mogen komen.";
        var result = engine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false);

        Assert.Equal(text, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_StillFixesTrailingLowercaseLInRealOcrError()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "FBl agent", doTryToGuessUnknownWords: false);

        Assert.Equal("FBI agent", result.GetText());
    }

    private static IOcrFixEngine CreateEngine()
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeDutchSpellChecker());
        engine.Initialize(new Subtitle(), "nld", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeDutchSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "al", "die", "regels", "en", "kamers", "waar", "we", "niet", "mogen", "komen",
            "AI", "FBI", "agent",
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
