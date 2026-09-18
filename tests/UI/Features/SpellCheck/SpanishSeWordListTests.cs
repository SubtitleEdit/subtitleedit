using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.SpellCheck;
using System;
using System.IO;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.SpellCheck;

// Issue #15004: the Spanish Hunspell dictionaries (RLA-ES) only accept imperative + enclitic
// pronoun forms ("pruébalo", "escúchalo") for a short whitelist of verbs. Subtitle Edit ships
// those forms in a word list, but it was named "es_MX_se.xml" and only "<fiveLetter>_se.xml" was
// read, so es_ES / es_US / es_AR users never got it. The list is now the language-wide
// "es_se.xml", read for every Spanish dictionary.
// Runs against the shipped Dictionaries/es_se.xml, not a copy of its words.
public class SpanishSeWordListTests : IDisposable
{
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public SpanishSeWordListTests()
    {
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;

        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeSpanishSeWordListTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);

        File.Copy(
            Path.Combine(FindRepoRoot(), "Dictionaries", "es_se.xml"),
            Path.Combine(_tempDictionariesFolder, "es_se.xml"));

        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Dictionaries")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root");
    }

    [Theory]
    [InlineData("es_ES")]
    [InlineData("es_MX")]
    [InlineData("es_US")]
    [InlineData("es_AR")]
    public void TwoLetterSeList_IsReadForEverySpanishDictionary(string dictionaryName)
    {
        var wordLists = new SpellCheckWordLists(dictionaryName, new AlwaysMissSpellChecker());

        Assert.True(wordLists.HasUserWord("pruébalo"));
        Assert.True(wordLists.HasUserWord("Pruébalo"));
    }

    [Theory]
    [InlineData("pruébalo")]    // o -> ue verb, informal
    [InlineData("cómpralo")]
    [InlineData("cómprala")]
    [InlineData("escúchalo")]
    [InlineData("escúchala")]
    [InlineData("muéstralo")]
    [InlineData("recuérdalo")]
    [InlineData("muévelo")]
    [InlineData("quítatelo")]   // reflexive + direct object
    [InlineData("póntelo")]
    [InlineData("explíquemelo")] // formal, -car -> -que
    [InlineData("averígüelo")]
    [InlineData("atrápenlo")]   // plural
    [InlineData("vámonos")]
    public void ImperativeWithEncliticPronoun_IsKnown(string word)
    {
        var wordLists = new SpellCheckWordLists("es_ES", new AlwaysMissSpellChecker());

        Assert.True(wordLists.HasUserWord(word));
    }

    [Fact]
    public void OtherLanguage_DoesNotReadSpanishList()
    {
        var wordLists = new SpellCheckWordLists("en_US", new AlwaysMissSpellChecker());

        Assert.False(wordLists.HasUserWord("pruébalo"));
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
            // Best-effort cleanup; leaving a temp dir behind is harmless.
        }
    }

    private sealed class AlwaysMissSpellChecker : IDoSpell
    {
        public bool DoSpell(string word)
        {
            return false;
        }
    }
}
