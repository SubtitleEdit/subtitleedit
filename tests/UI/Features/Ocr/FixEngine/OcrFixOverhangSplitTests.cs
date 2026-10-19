using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// An overhanging f, r or t leans over the j, v or w after it, so nOCR's space detector swallows
// the word space: "Hoorje", "kentje", "Vijfvan", "ofwe" (#16031, Dutch Blu-ray). An unknown word
// is split there when the dictionary knows both halves - only for image-matcher output.
public class OcrFixOverhangSplitTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixOverhangSplitTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixOverhangSplitTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "nld_OCRFixReplaceList.xml"),
            "<ReplaceList></ReplaceList>");
    }

    [Theory]
    [InlineData("Hoorje me niet?", "Hoor je me niet?")]
    [InlineData("Hij kentje man.", "Hij kent je man.")]
    [InlineData("Ox, achterje.", "Ox, achter je.")]
    [InlineData("Vijfvan elk eskader...", "Vijf van elk eskader...")]
    [InlineData("We rukken in ofwe ontploffen.", "We rukken in of we ontploffen.")]
    public void FixOcrErrors_UnknownWordWithSwallowedSpace_IsSplit(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("De software werkt.")] // a known word is never split
    [InlineData("Hij kentjex man.")] // the right half is no word
    [InlineData("Hij xkentje man.")] // the left half is no word
    [InlineData("Hij kentje2 man.")] // digits
    [InlineData("Getverdikkeme. Je jatten.")] // both halves are words, but the right one is no short function word
    public void FixOcrErrors_NoDictionaryHalves_IsLeftAlone(string input)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_TextEngineOutput_IsLeftAlone()
    {
        var engine = CreateEngine(fixImageMatcherMisreads: false);

        var result = engine.FixOcrErrors(0, "Hoorje me niet?", doTryToGuessUnknownWords: false);

        Assert.Equal("Hoorje me niet?", result.GetText());
    }

    private static IOcrFixEngine CreateEngine(bool fixImageMatcherMisreads = true)
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeSpellChecker()) { FixImageMatcherMisreads = fixImageMatcherMisreads };
        engine.Initialize(new Subtitle(), "nld", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "hoor", "je", "me", "niet", "hij", "kent", "man", "ox", "achter", "vijf", "van", "elk", "eskader",
            "we", "rukken", "in", "of", "ontploffen", "de", "software", "werkt", "get", "verdikkeme", "jatten",
        };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return true;
            }

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
