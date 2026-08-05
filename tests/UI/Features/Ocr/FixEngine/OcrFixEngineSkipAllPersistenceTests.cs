using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.Ocr.FixEngine;

public class OcrFixEngineSkipAllPersistenceTests : IDisposable
{
    private readonly Func<string> _originalDictionariesFolder;
    private readonly string _tempFolder;

    public OcrFixEngineSkipAllPersistenceTests()
    {
        _originalDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempFolder = Path.Combine(Path.GetTempPath(), "SeOcrSkipAllTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempFolder);
        SpellCheckConfig.DictionariesFolder = () => _tempFolder;
    }

    [Fact]
    public void SkipAll_PersistsOncePerLanguageWithoutCrossLanguageContamination()
    {
        var engine = new OcrFixEngine(new FakeSpellChecker());
        var dictionary = new SpellCheckDictionaryDisplay { DictionaryFileName = "test.dic" };
        var subtitle = new Subtitle();

        ((IOcrFixEngine)engine).Initialize(subtitle, "eng", dictionary);
        engine.SkipAll(new[] { "foo", "foo", "FOO" });

        var englishFile = Path.Combine(_tempFolder, "eng_OcrSkipAllList.txt");
        Assert.Equal(new[] { "FOO", "foo" }, File.ReadAllLines(englishFile));

        // Re-use the same engine, as OcrViewModel does when the dictionary changes.
        ((IOcrFixEngine)engine).Initialize(subtitle, "nld", dictionary);
        engine.SkipAll(new[] { "bar", "baz" });

        var dutchFile = Path.Combine(_tempFolder, "nld_OcrSkipAllList.txt");
        Assert.Equal(new[] { "bar", "baz" }, File.ReadAllLines(dutchFile));
        Assert.DoesNotContain("foo", File.ReadAllLines(dutchFile));
        Assert.Equal(new[] { "FOO", "foo" }, File.ReadAllLines(englishFile));
        Assert.Empty(Directory.GetFiles(_tempFolder, "*.tmp"));

        var reloadedEngine = new OcrFixEngine(new FakeSpellChecker());
        var verificationSubtitle = new Subtitle();
        verificationSubtitle.Paragraphs.Add(new Paragraph("foo control", 0, 1000));
        ((IOcrFixEngine)reloadedEngine).Initialize(verificationSubtitle, "eng", dictionary);

        var result = reloadedEngine.FixOcrErrors(0, "foo control", doTryToGuessUnknownWords: false);
        Assert.True(result.Words.Single(p => p.Word == "foo").IsSpellCheckedOk);
        Assert.False(result.Words.Single(p => p.Word == "control").IsSpellCheckedOk);
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalDictionariesFolder;
        Directory.Delete(_tempFolder, true);
    }

    private sealed class FakeSpellChecker : ISpellChecker
    {
        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
        public bool IsWordCorrect(string word) => false;
        public List<string> GetSuggestions(string word) => new();
    }
}
