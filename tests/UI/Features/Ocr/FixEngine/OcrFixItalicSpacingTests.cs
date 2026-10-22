using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// Italic glyphs lean into the next one, so nOCR's space detector puts a word space after a
// thousands-separator period ("20. 000") and before a closing quote ("voorbij. '") (#16031).
// Both are repaired on the line, only for image-matcher output.
public class OcrFixItalicSpacingTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixItalicSpacingTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixItalicSpacingTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        foreach (var language in new[] { "nld", "fra" })
        {
            File.WriteAllText(
                Path.Combine(_tempDictionariesFolder, language + "_OCRFixReplaceList.xml"),
                "<ReplaceList></ReplaceList>");
        }
    }

    [Theory]
    [InlineData("<i>en klim tot 20. 000.</i>", "<i>en klim tot 20.000.</i>")]
    [InlineData("<i>op 20. 000 voet richting west.</i>", "<i>op 20.000 voet richting west.</i>")]
    [InlineData("klim naar 1. 500 voet", "klim naar 1.500 voet")]
    public void FixOcrErrors_SpacedThousandsSeparator_IsJoined(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Het was 1940. 300 vliegtuigen kwamen.")] // a year ends the sentence
    [InlineData("Het was 20. 30 vliegtuigen kwamen.")] // not a three-digit group
    [InlineData("Het was 20. 3000 vliegtuigen kwamen.")]
    [InlineData("1.000. 000")] // already a separator before the period
    public void FixOcrErrors_SentenceEndingInNumber_IsLeftAlone(string input)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    [Theory]
    [InlineData("<i>\"Generaal Weygand zei:\n'De Franse strijd is voorbij. '</i>", "<i>\"Generaal Weygand zei:\n'De Franse strijd is voorbij.'</i>")]
    [InlineData("<i>\"De Engelse strijd gaat nu beginnen. \"</i>", "<i>\"De Engelse strijd gaat nu beginnen.\"</i>")]
    [InlineData("Hij zei: 'Kom. ' 'Nee.'", "Hij zei: 'Kom.' 'Nee.'")]
    public void FixOcrErrors_SpaceBeforeClosingQuote_IsRemoved(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    [Theory]
    [InlineData("Hij zei: 'Kom.' 'Nee.'")] // the quote opens the next word
    [InlineData("Hij komt. 's Avonds.")] // apostrophe of the next word
    [InlineData("Hij komt. \"Nee.\"")]
    public void FixOcrErrors_QuoteOpeningNextWord_IsLeftAlone(string input)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    // French puts a space before the closing guillemet - next to a « (any language) and on
    // every French line.
    [Theory]
    [InlineData("nld", "« Il est parti. »")]
    [InlineData("nld", "Il a dit : « Non ! »")]
    [InlineData("fra", "Non ! »")]
    public void FixOcrErrors_FrenchClosingGuillemetSpace_IsKept(string language, string input)
    {
        var engine = CreateEngine(language: language);

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    [Fact]
    public void FixOcrErrors_SpaceBeforeLoneClosingGuillemet_IsRemoved()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "Det är slut. »", doTryToGuessUnknownWords: false);

        Assert.Equal("Det är slut.»", result.GetText());
    }

    [Theory]
    [InlineData("<i>en klim tot 20. 000.</i>")]
    [InlineData("<i>\"De Engelse strijd gaat nu beginnen. \"</i>")]
    public void FixOcrErrors_TextEngineOutput_IsLeftAlone(string input)
    {
        var engine = CreateEngine(fixImageMatcherMisreads: false);

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    private static IOcrFixEngine CreateEngine(bool fixImageMatcherMisreads = true, string language = "nld")
    {
        IOcrFixEngine engine = new OcrFixEngine(new AcceptAllSpellChecker()) { FixImageMatcherMisreads = fixImageMatcherMisreads };
        engine.Initialize(new Subtitle(), language, new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class AcceptAllSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
        public bool IsWordCorrect(string word) => true;
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
