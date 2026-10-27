using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System.Xml;

namespace UITests.Features.SpellCheck;

// #16031: Dutch "weetje", "watje", "wasje" and "moetje" are real (rare) words, so Hunspell accepts
// them - and an OCR'd "weet je" with the space lost is then never flagged by spell check, nor
// split by the OCR fix rules that only fire on unknown words. Options > Word lists can now remove
// a word from the dictionary: the <removed> section of "<lang>_user.xml".
public class RemovedDictionaryWordsTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _folder;

    public RemovedDictionaryWordsTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _folder = Path.Combine(Path.GetTempPath(), "SeRemovedDictionaryWordsTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        SpellCheckConfig.DictionariesFolder = () => _folder;

        File.WriteAllText(Path.Combine(_folder, "nl_NL.dic"), "9\nweetje\nweet\nje\nwatje\nwat\nik\nbedoel\nwasje\nwas\n");
        File.WriteAllText(Path.Combine(_folder, "nl_NL.aff"), "SET UTF-8\n");
    }

    private SpellChecker CreateChecker()
    {
        var spellChecker = new SpellChecker();
        Assert.True(spellChecker.Initialize(Path.Combine(_folder, "nl_NL.dic"), "nl"));
        return spellChecker;
    }

    private void WriteUserFile(string xml)
    {
        File.WriteAllText(Path.Combine(_folder, "nl_NL_user.xml"), xml);
    }

    [Fact]
    public void RemovedWord_IsMisspelled_InEveryCase()
    {
        WriteUserFile("<words><removed><word>weetje</word></removed></words>");
        var spellChecker = CreateChecker();

        Assert.False(spellChecker.IsWordCorrect("weetje"));
        Assert.False(spellChecker.IsWordCorrect("Weetje"));
        Assert.False(spellChecker.DoSpell("WEETJE"));
        Assert.False(spellChecker.IsWordCorrect(new SpellCheckWord { Index = 0, Text = "Weetje" }, "Weetje wat ik bedoel?"));
    }

    [Fact]
    public void OtherDictionaryWords_StayCorrect()
    {
        WriteUserFile("<words><removed><word>weetje</word></removed></words>");
        var spellChecker = CreateChecker();

        Assert.True(spellChecker.IsWordCorrect("weet"));
        Assert.True(spellChecker.IsWordCorrect("watje"));
    }

    [Fact]
    public void WithoutRemovedSection_DictionaryWordIsCorrect()
    {
        WriteUserFile("<words><word>foo</word></words>");
        var spellChecker = CreateChecker();

        Assert.True(spellChecker.IsWordCorrect("weetje"));
        Assert.True(spellChecker.IsWordCorrect(new SpellCheckWord { Index = 0, Text = "foo" }, "foo"));
    }

    [Fact]
    public void RemovedWord_IsNotSuggested()
    {
        WriteUserFile("<words><removed><word>watje</word></removed></words>");
        var spellChecker = CreateChecker();

        var suggestions = spellChecker.GetSuggestions("wasje1");

        Assert.DoesNotContain("watje", suggestions);
    }

    [Fact]
    public void UserWord_WinsOverShippedRemovedWord()
    {
        File.WriteAllText(Path.Combine(_folder, "nl_se.xml"), "<words><removed><word>watje</word><word>wasje</word></removed></words>");
        WriteUserFile("<words><word>watje</word></words>");
        var wordLists = new SpellCheckWordLists("nl_NL");

        Assert.False(wordLists.IsRemovedWord("watje"));
        Assert.True(wordLists.HasUserWord("watje"));
        Assert.True(wordLists.IsRemovedWord("wasje"));
    }

    [Fact]
    public void UserRemovedWord_WinsOverShippedWord()
    {
        File.WriteAllText(Path.Combine(_folder, "nl_se.xml"), "<words><word>flutlandje</word></words>");
        WriteUserFile("<words><removed><word>flutlandje</word></removed></words>");
        var wordLists = new SpellCheckWordLists("nl_NL");

        Assert.True(wordLists.IsRemovedWord("flutlandje"));
        Assert.False(wordLists.HasUserWord("flutlandje"));
    }

    [Fact]
    public void WritingWords_KeepsRemovedSection()
    {
        var doc = new XmlDocument();
        doc.LoadXml("<words><word>b</word><removed><word>weetje</word></removed></words>");

        UserDictionaryXml.WriteWords(doc, new List<string> { "c", "a" });

        Assert.Equal(new[] { "a", "c" }, UserDictionaryXml.ReadWords(doc));
        Assert.Equal(new[] { "weetje" }, UserDictionaryXml.ReadRemovedWords(doc));
    }

    [Fact]
    public void RemovedWords_AreNotReadAsAcceptedWords()
    {
        var doc = new XmlDocument();
        doc.LoadXml("<words><removed><word>weetje</word></removed></words>");

        Assert.Empty(UserDictionaryXml.ReadWords(doc));
    }

    [Fact]
    public void WritingEmptyRemovedList_DropsTheSection()
    {
        var doc = new XmlDocument();
        doc.LoadXml("<words><word>a</word><removed><word>weetje</word></removed></words>");

        UserDictionaryXml.RemoveFromRemovedWords(doc, "weetje");

        Assert.Null(doc.DocumentElement!.SelectSingleNode(UserDictionaryXml.RemovedElementName));
        Assert.Equal(new[] { "a" }, UserDictionaryXml.ReadWords(doc));
    }

    [Theory]
    [InlineData(true, "Weet je wat ik bedoel?")]
    [InlineData(false, "Weetje wat ik bedoel?")]
    public void OcrFix_SplitsGluedPronoun_OnlyWhenTheJoinedWordIsRemoved(bool removeWeetje, string expected)
    {
        WriteUserFile(removeWeetje ? "<words><removed><word>weetje</word></removed></words>" : "<words />");

        // The glued-pronoun rule shipped in nld_OCRFixReplaceList.xml, cut to one verb.
        File.WriteAllText(
            Path.Combine(_folder, "nld_OCRFixReplaceList.xml"),
            "<ReplaceList><RegularExpressionsIfSpelledCorrectly>" +
            "<RegEx find=\"\\b([Ww]eet)(je)\\b\" spellCheck=\"$2\" replaceWith=\"$1 $2\" />" +
            "</RegularExpressionsIfSpelledCorrectly></ReplaceList>");

        IOcrFixEngine engine = new OcrFixEngine(new SpellChecker());
        engine.Initialize(new Subtitle(), "nld", new SpellCheckDictionaryDisplay { DictionaryFileName = Path.Combine(_folder, "nl_NL.dic") });

        var result = engine.FixOcrErrors(0, "Weetje wat ik bedoel?", doTryToGuessUnknownWords: false);

        Assert.Equal(expected, result.GetText());
    }

    public void Dispose()
    {
        SpellCheckConfig.DictionariesFolder = _originalSpellCheckDictionariesFolder;
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
