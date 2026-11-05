using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

// nOCR reads a slanted italic "l" as "/" ("p/ease", "re/igions", "/itt/e"). The line splitter cuts
// words at the slash, so the engine repairs these on the line, gated on the dictionary.
public class OcrFixSlashMisreadAsLTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public OcrFixSlashMisreadAsLTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeOcrFixSlashMisreadAsLTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
        File.WriteAllText(
            Path.Combine(_tempDictionariesFolder, "eng_OCRFixReplaceList.xml"),
            "<ReplaceList></ReplaceList>");
    }

    [Theory]
    [InlineData("<i>One moment, p/ease.</i>", "<i>One moment, please.</i>")]
    [InlineData("by any of the re/igions.", "by any of the religions.")]
    [InlineData("Your /itt/e bird has flown.", "Your little bird has flown.")]
    [InlineData("<i>A private jet wi/I pick you up</i>", "<i>A private jet will pick you up</i>")]
    [InlineData("and was teI/ing her", "and was telling her")]
    [InlineData("keep a/I of your be/ongings", "keep all of your belongings")]
    [InlineData("<i>On/y I can tie others.</i>", "<i>Only I can tie others.</i>")]
    [InlineData("<i>a// of them, we// done</i>", "<i>all of them, well done</i>")]
    [InlineData("stop ki//ing", "stop killing")]
    public void FixOcrErrors_SlashMisreadAsL_IsFixed(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }


    // A slanted italic "i" or "I" is a slash too: "w/th", "aff/n/ty", and at the start of a word
    // ("/t's", "/'m", "/n") or on its own ("/ can"). The "l" reading is tried first, so "/itt/e"
    // still becomes "little".
    [Theory]
    [InlineData("<i>/t's according to plan.</i>", "<i>It's according to plan.</i>")]
    [InlineData("<i>/'m doing nothing.</i>", "<i>I'm doing nothing.</i>")]
    [InlineData("<i>/n the morning</i>", "<i>In the morning</i>")]
    [InlineData("<i>/ can see it.</i>", "<i>I can see it.</i>")]
    [InlineData("- /s it?\n- /t is.", "- Is it?\n- It is.")]
    [InlineData("<i>You won /n poker w/th them.</i>", "<i>You won in poker with them.</i>")]
    [InlineData("an aff/n/ty for clubs", "an affinity for clubs")]
    [InlineData("<i>/'m do/ng noth/ng.</i>", "<i>I'm doing nothing.</i>")]
    public void FixOcrErrors_SlashMisreadAsCapitalI_IsFixed(string input, string expected)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(expected.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }

    [Theory]
    [InlineData("[AC/DC's \"Back in Black\" plays]")]
    [InlineData("and/or one of your")]
    [InlineData("see http://example.com/please")]
    [InlineData("8600 A/varado Street")]
    [InlineData("1/2 of it")]
    [InlineData("<i>Heil</i> Hitler.")]
    [InlineData("<i>One moment.</i>")]
    [InlineData("<font color=\"red\">Hello</font>")]
    public void FixOcrErrors_RealSlash_IsLeftAlone(string input)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }

    // A lone slash next to a number or between two all-caps words is a separator (a title card
    // "PILOTEN / OMGEKOMEN" became "PILOTEN I OMGEKOMEN", #16031), not a slanted italic "I".
    [Theory]
    [InlineData("PILOTEN / OMGEKOMEN")]
    [InlineData("<i>PILOTEN / OMGEKOMEN</i>")]
    [InlineData("PILOTEN /\nOMGEKOMEN")]
    [InlineData("AC / DC")]
    [InlineData("1 / 2")]
    [InlineData("Scene 3 / Take 2")]
    public void FixOcrErrors_LoneSeparatorSlash_IsLeftAlone(string input)
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }

    // The capital I of the title card is read as "l"; the all-caps rule repairs that afterwards,
    // so the slash guard has to count the "l" as a capital or the slash is gone before it runs.
    [Fact]
    public void FixOcrErrors_LoneSlashBetweenAllCapsWordsWithMisreadBar_KeepsSlash()
    {
        var engine = CreateEngine();

        var result = engine.FixOcrErrors(0, "PlLOTEN / OMGEKOMEN", doTryToGuessUnknownWords: false);

        Assert.Equal("PILOTEN / OMGEKOMEN", result.GetText().Replace("\r\n", "\n"));
    }

    // Only image matchers (nOCR, binary image compare) read a slanted stroke as "/"; text engines
    // (Tesseract, Paddle, llama.cpp) never do, so the rule is off for their output and every
    // slash they produce stays.
    [Theory]
    [InlineData("PILOTEN / OMGEKOMEN")]
    [InlineData("<i>/ can see it.</i>")]
    [InlineData("<i>One moment, p/ease.</i>")]
    [InlineData("<i>/t's according to plan.</i>")]
    public void FixOcrErrors_TextEngineOutput_SlashIsLeftAlone(string input)
    {
        var engine = CreateEngine(fixImageMatcherMisreads: false);

        var result = engine.FixOcrErrors(0, input, doTryToGuessUnknownWords: false);

        Assert.Equal(input.Replace("\r\n", "\n"), result.GetText().Replace("\r\n", "\n"));
    }

    private static IOcrFixEngine CreateEngine(bool fixImageMatcherMisreads = true)
    {
        IOcrFixEngine engine = new OcrFixEngine(new FakeSpellChecker()) { FixImageMatcherMisreads = fixImageMatcherMisreads };
        engine.Initialize(new Subtitle(), "eng", new SpellCheckDictionaryDisplay());
        return engine;
    }

    private sealed class FakeSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "I", "one", "moment", "please", "by", "any", "of", "the", "religions", "your", "little",
            "bird", "has", "flown", "a", "private", "jet", "will", "pick", "you", "up", "and", "was",
            "telling", "her", "keep", "all", "belongings", "only", "can", "tie", "others", "or",
            "back", "in", "black", "plays", "see", "street", "it", "AC", "DC",
            // "li" is a dictionary word in Dutch; "</i>" must not become "<li>" because of it.
            "li", "Heil", "Hitler", "hello",
            "it's", "according", "to", "plan", "I'm", "doing", "nothing", "morning", "is", "it", "seeing",
            "won", "poker", "with", "them", "an", "affinity", "for", "clubs", "well", "done", "stop", "killing",
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
