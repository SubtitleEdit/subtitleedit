using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.SpellCheck;

// #10126: a hyphenated name added to the names list (e.g. "Soo-bin") was flagged part by part,
// because the tokenizer splits on '-'. HasNameExtended must recognize each part as a name when the
// combined name is in the names list and present in the line.
public class HyphenatedNameSpellCheckTests : IDisposable
{
    private const string LanguageName = "en_US";

    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public HyphenatedNameSpellCheckTests()
    {
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeHyphenNameTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        Se.DictionariesFolder = _tempDictionariesFolder;
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    [Fact]
    public void HyphenatedNameInNamesList_RecognizesEachPart()
    {
        var wordLists = new SpellCheckWordLists(LanguageName);
        wordLists.AddName("Soo-bin");

        const string line = "I met Soo-bin today.";
        Assert.True(wordLists.HasNameExtended("Soo", line));
        Assert.True(wordLists.HasNameExtended("bin", line));
    }

    [Fact]
    public void DashNamePart_NotMatched_WhenCombinedNameAbsentFromLine()
    {
        var wordLists = new SpellCheckWordLists(LanguageName);
        wordLists.AddName("Soo-bin");

        // "Soo" on its own (no "Soo-bin" in the line) must not be treated as a name.
        Assert.False(wordLists.HasNameExtended("Soo", "I met Soo today."));
    }

    // #16472: AddName indexed plain names for the dash/period part lookup and RemoveName never took
    // them out, so after spell-check Undo of "Add to names list" the word still counted as a name.
    [Fact]
    public void RemoveName_PlainName_NoLongerRecognized()
    {
        var wordLists = new SpellCheckWordLists(LanguageName);
        wordLists.AddName("Zorbulon");

        const string line = "Zorbulon came home.";
        Assert.True(wordLists.HasName("Zorbulon"));

        wordLists.RemoveName("Zorbulon");

        Assert.False(wordLists.HasName("Zorbulon"));
        Assert.False(wordLists.HasNameExtended("Zorbulon", line));
    }

    [Fact]
    public void RemoveName_HyphenatedName_PartsNoLongerRecognized()
    {
        var wordLists = new SpellCheckWordLists(LanguageName);
        wordLists.AddName("Soo-bin");
        wordLists.RemoveName("Soo-bin");

        const string line = "I met Soo-bin today.";
        Assert.False(wordLists.HasNameExtended("Soo", line));
        Assert.False(wordLists.HasNameExtended("bin", line));
    }

    // #16472: Undo wrote the removed name to the blacklist, so adding it again later was refused
    // on disk and the name was lost on the next start.
    [Fact]
    public void AddName_AfterRemove_IsSavedAgain()
    {
        var wordLists = new SpellCheckWordLists(LanguageName);
        wordLists.AddName("Zorbulon");
        wordLists.RemoveName("Zorbulon");
        wordLists.AddName("Zorbulon");

        var reloaded = new SpellCheckWordLists(LanguageName);
        Assert.True(reloaded.HasName("Zorbulon"));
    }

    public void Dispose()
    {
        Se.DictionariesFolder = _originalDictionariesFolder;
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
