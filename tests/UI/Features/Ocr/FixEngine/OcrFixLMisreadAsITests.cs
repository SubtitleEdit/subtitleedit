using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Issue #13660 (beta20 follow-up): binary image compare reads lowercase l as i or uppercase I in
// non-italic text too ("friendiy", "hostiie", "InteIIigence"). Those were only repaired by the
// PartialWords guesses, which need "try to guess unknown words" on - and the I->l regex rule
// swaps the genuine leading capital of "InteIIigence" as well, so it never matched. The engine now
// tries the i/I->l swaps itself for every unknown word, gated on the dictionary, in any language.
public class OcrFixLMisreadAsITests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixLMisreadAsITests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixLMisreadAsITest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;

        // No replace list at all - the fix must not depend on per-language list entries.
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"),
            "<ReplaceList></ReplaceList>");
    }

    [Theory]
    [InlineData("<i>I have friendiy forces moving</i>", "<i>I have friendly forces moving</i>")]
    [InlineData("<i>possible hostiie forces approaching</i>", "<i>possible hostile forces approaching</i>")]
    [InlineData("<i>Friendiies are continuing on</i>", "<i>Friendlies are continuing on</i>")]
    [InlineData("InteIIigence said", "Intelligence said")]
    [InlineData("she smells most deIectabIe.", "she smells most delectable.")]
    [InlineData("Lass uns nicht noch zu CIowns werden.", "Lass uns nicht noch zu Clowns werden.")]
    public void FixOcrErrors_LMisreadAsI_IsFixedWithoutGuessing(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_LMisreadAsI_IsReportedAsGuess()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "I have friendiy forces", doTryToGuessUnknownWords: false);

        var word = Assert.Single(result.Words, w => w.GuessUsed);
        Assert.Equal("friendiy", word.Word);
        Assert.Equal("friendly", word.FixedWord);
    }

    [Fact]
    public void FixOcrErrors_CorrectWordWithI_IsLeftAlone()
    {
        var engine = CreateEngine();

        // "Hail" -> "Hall" and "Mali" -> "Mall" would both be dictionary words, but the originals are correct.
        var result = engine.FixOcrErrors(0, "Hail from Mali", doTryToGuessUnknownWords: false);

        Assert.Equal("Hail from Mali", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_ShortUnknownWord_IsLeftAlone()
    {
        var engine = CreateEngine();

        // Four letters: too short to risk the swap ("Cali" is a name, not a misread "Call").
        var result = engine.FixOcrErrors(0, "From Cali", doTryToGuessUnknownWords: false);

        Assert.Equal("From Cali", result.GetText());
    }

    [Fact]
    public void FixOcrErrors_UnknownWordWithNoMatchingSwap_IsLeftAlone()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "Meet Dimitri", doTryToGuessUnknownWords: false);

        Assert.Equal("Meet Dimitri", result.GetText());
    }

    private static IOcrFixEngine CreateEngine()
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeSpellChecker());
        engine.Initialize(new Subtitle(), "eng", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "I", "have", "friendly", "forces", "moving", "possible", "hostile", "approaching",
            "friendlies", "are", "continuing", "on", "intelligence", "said", "she", "smells", "most",
            "delectable", "lass", "uns", "nicht", "noch", "zu", "clowns", "werden",
            "hail", "hall", "from", "Mali", "mall", "call", "meet",
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
