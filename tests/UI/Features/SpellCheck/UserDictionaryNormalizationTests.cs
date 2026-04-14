using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UITests.Features.SpellCheck;

public class UserDictionaryNormalizationTests : IDisposable
{
    private const string LanguageName = "en_US";
    private const string HangulWord = "매트릭스의";
    private readonly string _nfcWord = HangulWord.Normalize(NormalizationForm.FormC);
    private readonly string _nfdWord = HangulWord.Normalize(NormalizationForm.FormD);

    public UserDictionaryNormalizationTests()
    {
        Directory.CreateDirectory(Se.DictionariesFolder);
        UserWordsHelper.RemoveWord(_nfcWord, LanguageName);
        UserWordsHelper.RemoveWord(_nfdWord, LanguageName);
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
        UserWordsHelper.RemoveWord(_nfcWord, LanguageName);
        UserWordsHelper.RemoveWord(_nfdWord, LanguageName);
    }

    private sealed class AlwaysMissSpellChecker : IDoSpell
    {
        public bool DoSpell(string word)
        {
            return false;
        }
    }
}
