using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// nOCR reads a plain letter as an accented lookalike when the accent sits where the letter's dot
// is ("ìs", "rîght") and mixes up accents ("Màske" for "Måske"). Measured on real subtitles these
// were the most common errors left after matching; the dictionary decides between the lookalikes.
public class OcrFixAccentMisreadTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixAccentMisreadTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(Path.GetTempPath(), "SeOcrFixAccentMisreadTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    [Theory]
    [InlineData("This ìs it.", "This is it.")]
    [InlineData("All rîght, bîg guy.", "All right, big guy.")]
    [InlineData("Wìth a bunch of movíes.", "With a bunch of movies.")]
    public void FixOcrErrors_PlainLetterReadAsAccented_IsFixed(string input, string expected)
    {
        var engine = CreateEngine("eng", "this", "is", "it", "all", "right", "big", "guy", "with", "a", "bunch", "of", "movies");

        Assert.Equal(expected, engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false).GetText());
    }

    [Fact]
    public void FixOcrErrors_WrongAccent_PrefersAnotherAccentOverThePlainLetter()
    {
        // Both "maske" (mask) and "måske" (maybe) are Danish words; the OCR saw a mark, so the
        // accented reading wins.
        var engine = CreateEngine("dan", "maske", "måske", "får", "far", "du", "det");

        Assert.Equal("Måske får du det.", engine.FixOcrErrors(0, "Màske fàr du det.", doTryToGuessUnknownWords: false).GetText());
    }

    [Fact]
    public void FixOcrErrors_CorrectAccentedWord_IsLeftAlone()
    {
        var engine = CreateEngine("fra", "café", "cafe", "un");

        Assert.Equal("Un café.", engine.FixOcrErrors(0, "Un café.", doTryToGuessUnknownWords: false).GetText());
    }

    [Theory]
    [InlineData("A café.")]
    [InlineData("René is here.")]
    [InlineData("A résumé.")]
    [InlineData("So naïve.")]
    public void FixOcrErrors_English_AccentedLoanwordOrName_IsNotStripped(string input)
    {
        // English dictionaries are plain ASCII, so "cafe"/"Rene"/"resume" would always be "confirmed".
        var engine = CreateEngine("eng", "a", "cafe", "rene", "is", "here", "resume", "so", "naive");

        Assert.Equal(input, engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false).GetText());
    }

    [Fact]
    public void FixOcrErrors_French_AccentedToPlain_StillFixed()
    {
        var engine = CreateEngine("fra", "il", "est", "là");

        Assert.Equal("Il est là.", engine.FixOcrErrors(0, "Il ést là.", doTryToGuessUnknownWords: false).GetText());
    }

    [Theory]
    [InlineData("Eén voor de sergeant.")]
    [InlineData("Vijf zijn schroot. Eén mist z'n")]
    [InlineData("Eénmaal is genoeg.")]
    public void FixOcrErrors_Dutch_CapitalizedEen_IsNotStripped(string input)
    {
        // Dutch capitalizes "één" as "Eén"; the spell checker lowercases that to "eén", which is no
        // word, and "Een" is - so the accent fix used to strip it (#16031).
        var engine = CreateEngine("nld", "een", "één", "éénmaal", "voor", "de", "sergeant", "vijf", "zijn", "schroot", "mist", "z'n", "is", "genoeg");

        Assert.Equal(input, engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false).GetText());
    }

    [Fact]
    public void FixOcrErrors_Dutch_MisreadAccentWithoutAccentedWord_StillFixed()
    {
        var engine = CreateEngine("nld", "een", "voor", "de", "sergeant");

        Assert.Equal("Een voor de sergeant.", engine.FixOcrErrors(0, "Eèn voor de sergeant.", doTryToGuessUnknownWords: false).GetText());
    }

    [Fact]
    public void FixOcrErrors_NoLookalikeInDictionary_IsLeftAlone()
    {
        var engine = CreateEngine("eng", "is");

        Assert.Equal("Zoë is", engine.FixOcrErrors(0, "Zoë is", doTryToGuessUnknownWords: false).GetText());
    }

    private static IOcrFixEngine CreateEngine(string language, params string[] words)
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeSpellChecker(words));
        engine.Initialize(new Subtitle(), language, new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeSpellChecker(string[] words) : ISpellChecker
    {
        private readonly HashSet<string> _words = new(words, StringComparer.Ordinal);

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return true;
            }

            return _words.Contains(word) || _words.Contains(word.ToLowerInvariant());
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
