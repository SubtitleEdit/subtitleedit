using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// nOCR fits the sans-serif capital I and the lowercase l to the same bar (#16031, Dutch Blu-ray):
// names come out as "CowsIip", "HeinkeIs", "FIight", all-caps words as "JUNl", "lFF",
// "WAARSCHlJNLlJK". No dictionary knows the names, so for a word that is unknown anyway the
// letters around the bar decide - only when the text comes from an image matcher.
public class OcrFixBarMisreadByCaseTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixBarMisreadByCaseTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixBarMisreadByCaseTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "nld_OCRFixReplaceList.xml"),
            "<ReplaceList></ReplaceList>");
    }

    [Theory]
    [InlineData("hier is CowsIip.", "hier is Cowslip.")]
    [InlineData("<i>Begrepen, Turkey-controI.</i>", "<i>Begrepen, Turkey-control.</i>")]
    [InlineData("<i>HeinkeIs, 20 plus.</i>", "<i>Heinkels, 20 plus.</i>")]
    [InlineData("<i>Rabbit-eskader. HaIIaIi, Sectie rood.</i>", "<i>Rabbit-eskader. Hallali, Sectie rood.</i>")]
    [InlineData("voor A FIight.", "voor A Flight.")]
    [InlineData("<i>gelezen door AIvar LiddeII.</i>", "<i>gelezen door Alvar Liddell.</i>")]
    [InlineData("<i>in AIIenwood, Pennsylvania,</i>", "<i>in Allenwood, Pennsylvania,</i>")]
    [InlineData("Dat is CUCKOOlNG.", "Dat is CUCKOOING.")]
    [InlineData("JUNl 1940", "JUNI 1940")]
    [InlineData("lN POSlTlE", "IN POSITIE")]
    [InlineData("Geen lFF.", "Geen IFF.")]
    [InlineData("1 - lSRAELlERS - 0", "1 - ISRAELIERS - 0")]
    [InlineData("(WAARSCHlJNLlJK OMGEKOMEN)", "(WAARSCHIJNLIJK OMGEKOMEN)")]
    public void FixOcrErrors_UnknownWordWithMisreadBar_IsFixedByCase(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Ik zie McIntyre.")] // Mc/Mac keep their capital
    [InlineData("MacIsaac komt.")]
    [InlineData("Deel Il is klaar.")] // a roman numeral, and Italian "Il": capital first, bar last
    [InlineData("lk ben hier.")] // lowercase word: the dictionary-gated rules decide
    [InlineData("I ben hier.")]
    [InlineData("Dat is McDONALD.")] // a real lowercase letter among capitals
    public void FixOcrErrors_NoBarPattern_IsLeftAlone(string input)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    [Theory]
    [InlineData("hier is CowsIip.")]
    [InlineData("JUNl 1940")]
    public void FixOcrErrors_TextEngineOutput_IsLeftAlone(string input)
    {
        var engine = CreateEngine(fixImageMatcherMisreads: false);

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_FixedBarWordKnownToDictionary_IsSpelledOk()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "Turkey-controI", doTryToGuessUnknownWords: false);

        Assert.Equal("Turkey-control", result.GetText());
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
            "I", "hier", "is", "begrepen", "plus", "sectie", "rood", "voor", "a", "gelezen", "door", "geen",
            "ik", "zie", "komt", "deel", "klaar", "ben", "dat", "control", "flight", "in", "positie",
            "waarschijnlijk", "omgekomen", "turkey", "rabbit", "eskader",
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
