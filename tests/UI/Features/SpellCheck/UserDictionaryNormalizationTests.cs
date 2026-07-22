using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace UITests.Features.SpellCheck;

public class UserDictionaryNormalizationTests : IDisposable
{
    private const string LanguageName = "en_US";
    private const string HangulWord = "매트릭스의";
    private readonly string _nfcWord = HangulWord.Normalize(NormalizationForm.FormC);
    private readonly string _nfdWord = HangulWord.Normalize(NormalizationForm.FormD);

    private readonly string _originalDictionariesFolder;
    private readonly Func<string> _originalSpellCheckDictionariesFolder;
    private readonly string _tempDictionariesFolder;

    public UserDictionaryNormalizationTests()
    {
        // Redirect the dictionaries folder to an isolated temp dir so the real
        // user dictionary is not touched and pre-existing words don't leak in.
        _originalDictionariesFolder = Se.DictionariesFolder;
        _originalSpellCheckDictionariesFolder = SpellCheckConfig.DictionariesFolder;
        _tempDictionariesFolder = Path.Combine(
            Path.GetTempPath(),
            "SeUserDictTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDictionariesFolder);
        Se.DictionariesFolder = _tempDictionariesFolder;
        // SpellCheckWordLists reads its folder from SpellCheckConfig (wired to Se in Program.cs at
        // startup, which the tests don't run); point it at the same temp dir.
        SpellCheckConfig.DictionariesFolder = () => _tempDictionariesFolder;
    }

    [Fact]
    public void AddToUserDictionary_ShouldTreatVisuallyIdenticalHangulAsSameWordAcrossNormalizationForms()
    {
        Assert.True(UserWordsHelper.AddToUserDictionary(_nfdWord, LanguageName));
        Assert.False(UserWordsHelper.AddToUserDictionary(_nfcWord, LanguageName));

        var wordLists = new SpellCheckWordLists(LanguageName, new AlwaysMissSpellChecker());
        Assert.True(wordLists.HasUserWord(_nfcWord));
        Assert.True(wordLists.HasUserWord(_nfdWord));

        var loadedWords = new List<string>();
        UserWordsHelper.LoadUserWordList(loadedWords, LanguageName);

        var storedWord = Assert.Single(loadedWords);
        Assert.Equal(_nfcWord, storedWord);
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
