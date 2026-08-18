using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Issue #13660 (follow-up): binary image compare reads lowercase l as uppercase I, and the shipped
// lists only had rules for a single I per word ("traiIing"), so words with two of them were left
// for the spell checker: "deIectabIe", "ExceIIencies". The rule under test replaces every I in the
// word at once and keeps the result only when the dictionary confirms it.
public class OcrFixUppercaseIForLowercaseLTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixUppercaseIForLowercaseLTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixUppercaseIForLowercaseLTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;

        // The rule as shipped in eng_OCRFixReplaceList.xml.
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"),
            "<ReplaceList>" +
            "<RegularExpressionsIfSpelledCorrectly>" +
            "<RegEx find=\"\\b([A-Za-z](?=[a-zI]*I)[a-zI]{2,})\\b\" replaceAllFrom=\"I\" replaceAllTo=\"l\" spellCheck=\"$1\" replaceWith=\"$1\" />" +
            "</RegularExpressionsIfSpelledCorrectly>" +
            "</ReplaceList>");
    }

    [Fact]
    public void FixOcrErrors_TwoUppercaseIsInLowercaseWord_AreBothFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "she smells most deIectabIe.", doTryToGuessUnknownWords: false);

        Assert.Equal("she smells most delectable.", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_TwoUppercaseIsInCapitalizedWord_AreBothFixed()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "Would Your ExceIIencies", doTryToGuessUnknownWords: false);

        Assert.Equal("Would Your Excellencies", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_UppercaseIsRightAfterFirstLetter_AreFixed()
    {
        var engine = CreateEngine();

        // "AII" has no lowercase letter before the first I - the rule must still match.
        var result = engine.FixOcrErrors(0, "AII of it", doTryToGuessUnknownWords: false);

        Assert.Equal("All of it", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_WordWithGenuineUppercaseI_IsLeftAlone()
    {
        var engine = CreateEngine();

        // "MacIntosh" is a real word, so the already-spelled-correctly gate protects it.
        var result = engine.FixOcrErrors(0, "A MacIntosh weII.", doTryToGuessUnknownWords: false);

        Assert.Equal("A MacIntosh well.", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_TwoLetterWord_IsLeftAlone()
    {
        var engine = CreateEngine();

        // "AI" must not become "Al" even though "Al" is in the dictionary as a name.
        var result = engine.FixOcrErrors(0, "AI is here", doTryToGuessUnknownWords: false);

        Assert.Equal("AI is here", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_ReplacementIsNotAWord_IsLeftAlone()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "ZorbIx is here", doTryToGuessUnknownWords: false);

        Assert.Equal("ZorbIx is here", result.GetText());
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
            "she", "smells", "most", "delectable", "would", "your", "excellencies",
            "all", "of", "it", "is", "here", "well", "MacIntosh", "Al", "a",
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
