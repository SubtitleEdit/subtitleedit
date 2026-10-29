using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// A bold italic apostrophe fits nOCR's "#" entry ("He#s", "isn#t"). No word has a hash between two
// letters, so the engine reads it as an apostrophe, gated on the dictionary.
public class OcrFixHashMisreadAsApostropheTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixHashMisreadAsApostropheTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixHashMisreadAsApostropheTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"),
            "<ReplaceList></ReplaceList>");
    }

    [Theory]
    [InlineData("<i>He#s doing it.</i>", "<i>He's doing it.</i>")]
    [InlineData("He isn#t winning.", "He isn't winning.")]
    [InlineData("<i>That#s right.</i>", "<i>That's right.</i>")]
    [InlineData("- It#s him.\n- Isn#t it?", "- It's him.\n- Isn't it?")]
    public void FixOcrErrors_HashMisreadAsApostrophe_IsFixed(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }

    [Theory]
    [InlineData("C# is a language")]
    [InlineData("the #1 fan")]
    [InlineData("tag #winning")]
    [InlineData("A#B is no word")]
    public void FixOcrErrors_RealHash_IsLeftAlone(string input)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input, result.GetText());
    }

    private static IOcrFixEngine CreateEngine()
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeSpellChecker()) { FixImageMatcherMisreads = true };
        engine.Initialize(new Subtitle(), "eng", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "he's", "doing", "it", "he", "isn't", "winning", "that's", "right", "it's", "him", "is",
            "a", "language", "the", "fan", "tag", "no", "word", "C",
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
