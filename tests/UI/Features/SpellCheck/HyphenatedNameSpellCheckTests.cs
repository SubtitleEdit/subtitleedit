using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
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
        var wordLists = new SpellCheckWordLists(LanguageName, new AlwaysMissSpellChecker());
        wordLists.AddName("Soo-bin");

        const string line = "I met Soo-bin today.";
        Assert.True(wordLists.HasNameExtended("Soo", line));
        Assert.True(wordLists.HasNameExtended("bin", line));
    }

    [Fact]
    public void DashNamePart_NotMatched_WhenCombinedNameAbsentFromLine()
    {
        var wordLists = new SpellCheckWordLists(LanguageName, new AlwaysMissSpellChecker());
        wordLists.AddName("Soo-bin");

        // "Soo" on its own (no "Soo-bin" in the line) must not be treated as a name.
        Assert.False(wordLists.HasNameExtended("Soo", "I met Soo today."));
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

    private sealed class AlwaysMissSpellChecker : IDoSpell
    {
        public bool DoSpell(string word) => false;
    }
}
