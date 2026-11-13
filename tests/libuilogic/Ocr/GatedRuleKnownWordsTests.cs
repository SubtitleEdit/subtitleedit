using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.Ocr;

// #16378: the Spanish list restores a lost final accent ("cafe" -> "café") only when the accented
// form is a dictionary word, which also turned the French "rue" into "rué". "Malebranche" is not a
// dictionary word, so the line fails the spell check and the gated regexes really run.
public class GatedRuleKnownWordsTests
{
    private const string Line = "En la rue Malebranche.";

    [Fact]
    public void DictionaryOnly_FrenchWordWithAnAccentedTwin_IsAccented()
    {
        Assert.Equal("En la rué Malebranche.", Fix(Line));
    }

    [Fact]
    public void WordInTheUserDictionary_IsLeftAlone()
    {
        Assert.Equal(Line, Fix(Line, "rue"));
    }

    [Fact]
    public void WordInTheUserDictionary_DoesNotStopTheOtherFixesOnTheLine()
    {
        var text = "En la rue Malebranche, un monton de gente.";

        Assert.Equal("En la rue Malebranche, un montón de gente.", Fix(text, "rue"));
    }

    // The word a rule wants to write must still be a dictionary word - widening that side as well
    // would let a name or a user word trigger a rewrite.
    [Fact]
    public void ReplacementStillHasToBeADictionaryWord()
    {
        Assert.Equal(Line, Fix(Line, new FakeSpanishSpellChecker("en", "la", "malebranche"), "rué"));
    }

    // The cases above drive the gate directly. These go through OcrFixEngine, so the call site that
    // builds the "known word" test is covered too.
    [Fact]
    public void Engine_AccentsTheWordWithoutAUserDictionary()
    {
        Assert.Equal("En la rué Malebranche.", FixThroughEngine());
    }

    [Fact]
    public void Engine_LeavesAWordFromTheUserDictionaryAlone()
    {
        Assert.Equal(Line, FixThroughEngine(userWords: new[] { "rue" }));
    }

    // HasName only holds one-word names; "la rue" is a multi-word name, which needs HasNameExtended.
    [Fact]
    public void Engine_LeavesAWordInsideAMultiWordNameAlone()
    {
        Assert.Equal(Line, FixThroughEngine(multiWordName: "la rue"));
    }

    // What "Skip all" in the fix window fills.
    [Fact]
    public void Engine_LeavesASkippedWordAlone()
    {
        Assert.Equal(Line, FixThroughEngine(prepare: engine => engine.SkipAll("rue")));
    }

    // eng "\bl\b" -> "I" matches a one-letter token. IsWordOrName ignores words of one character,
    // so routing the gate through it would drop the dictionary protection for exactly these rules.
    // The "l-" keeps the ungated "(\bl\b)(?!-)" rule in the same list from firing first.
    [Fact]
    public void Engine_LeavesASingleLetterDictionaryWordAlone()
    {
        var checker = new FakeSpanishSpellChecker("l", "i");

        Assert.Equal("l- Zzzz", FixThroughEngine(
            line: "l- Zzzz", language: "eng", dictionaryFileName: "en_US.dic", checker: checker));
    }

    private static string Fix(string text, params string[] userWords)
    {
        return Fix(text, new FakeSpanishSpellChecker(), userWords);
    }

    private static string Fix(string text, ISpellChecker checker, params string[] userWords)
    {
        var list = new OcrFixReplaceList2(Path.Combine(RepositoryRoot(), "Dictionaries", "spa_OCRFixReplaceList.xml"));

        return list.FixOcrErrorViaLineReplaceList(
            text, new Subtitle(), 0, checker, new List<string>(), spelledOK: false,
            word => checker.IsWordCorrect(word) || userWords.Contains(word, StringComparer.OrdinalIgnoreCase));
    }

    private static string FixThroughEngine(
        string[]? userWords = null,
        string? multiWordName = null,
        Action<IOcrFixEngine>? prepare = null,
        string line = Line,
        string language = "spa",
        string dictionaryFileName = "es_ES.dic",
        ISpellChecker? checker = null)
    {
        var folder = Path.Combine(Path.GetTempPath(), "SeOcrGatedRule_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var previousFolder = SpellCheckConfig.DictionariesFolder;
        try
        {
            var listFileName = language + "_OCRFixReplaceList.xml";
            File.Copy(
                Path.Combine(RepositoryRoot(), "Dictionaries", listFileName),
                Path.Combine(folder, listFileName));
            if (userWords != null)
            {
                WriteList(folder, "es_ES_user.xml", "words", "word", userWords);
            }

            if (multiWordName != null)
            {
                WriteList(folder, "es_names_user.xml", "names", "name", new[] { multiWordName });
            }

            SpellCheckConfig.DictionariesFolder = () => folder;

            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(line, 0, 1000));
            IOcrFixEngine engine = new OcrFixEngine(checker ?? new FakeSpanishSpellChecker());
            engine.Initialize(subtitle, language, new SpellCheckDictionaryDisplay { DictionaryFileName = dictionaryFileName });
            prepare?.Invoke(engine);

            return engine.FixOcrErrors(0, line, doTryToGuessUnknownWords: false).GetText();
        }
        finally
        {
            SpellCheckConfig.DictionariesFolder = previousFolder;
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void WriteList(string folder, string fileName, string root, string element, IEnumerable<string> items)
    {
        File.WriteAllText(
            Path.Combine(folder, fileName),
            $"<{root}>" + string.Concat(items.Select(i => $"<{element}>{i}</{element}>")) + $"</{root}>");
    }

    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("SubtitleEdit.sln not found");
    }

    // "rué" is a -uar verb form such as "actué"; "rue" is not a dictionary word - that is what
    // makes the rule fire on the French word. Case-insensitive, like Hunspell.
    private sealed class FakeSpanishSpellChecker : ISpellChecker
    {
        private static readonly string[] DefaultWords = { "en", "la", "un", "de", "gente", "rué", "montón" };

        private readonly HashSet<string> _words;

        public FakeSpanishSpellChecker(params string[] words)
        {
            _words = new HashSet<string>(words.Length == 0 ? DefaultWords : words, StringComparer.OrdinalIgnoreCase);
        }

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word) => string.IsNullOrWhiteSpace(word) || _words.Contains(word);

        public List<string> GetSuggestions(string word) => new();
    }
}
