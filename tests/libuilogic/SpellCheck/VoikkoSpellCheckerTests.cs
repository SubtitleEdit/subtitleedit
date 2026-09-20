using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.SpellCheck;

public class VoikkoSpellCheckerTests
{
    [Fact]
    public void IsVoikkoDictionary_MatchesMarkerOnly()
    {
        Assert.True(SpellChecker.IsVoikkoDictionary(@"C:\x\Dictionaries\Voikko\fi_FI.voikko"));
        Assert.False(SpellChecker.IsVoikkoDictionary(@"C:\x\Dictionaries\fi_FI.dic"));
    }

    [Fact]
    public void MissingDictionary_IsNotAvailable()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-voikko-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            Assert.False(VoikkoSpellChecker.HasDictionary(folder));
            Assert.False(VoikkoSpellChecker.IsAvailable(folder));
            Assert.Null(VoikkoSpellChecker.TryCreate(folder, out var error));
            Assert.False(string.IsNullOrEmpty(error));
            Assert.False(new SpellChecker().Initialize(VoikkoSpellChecker.GetMarkerFile(folder), "fi"));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void BundledDictionary_NextToApp_IsFoundWhenUserFolderHasNone()
    {
        // The macOS bundle / Flatpak ship the dictionary outside the user's dictionaries folder;
        // the app folder is one of the probed roots.
        var userFolder = Path.Combine(Path.GetTempPath(), "se-voikko-" + Guid.NewGuid().ToString("N"));
        var bundled = Path.Combine(AppContext.BaseDirectory, "voikko", "5", "mor-standard");
        Directory.CreateDirectory(userFolder);
        Directory.CreateDirectory(bundled);
        try
        {
            File.WriteAllText(Path.Combine(bundled, "mor.vfst"), "x");
            Assert.True(VoikkoSpellChecker.HasDictionary(userFolder));
            Assert.Equal(Path.Combine(AppContext.BaseDirectory, "voikko"), VoikkoSpellChecker.FindDictionaryRoot(userFolder));

            // The user's own copy wins over the bundled one.
            var user = Path.Combine(VoikkoSpellChecker.GetVoikkoFolder(userFolder), "5", "mor-standard");
            Directory.CreateDirectory(user);
            File.WriteAllText(Path.Combine(user, "mor.vfst"), "x");
            Assert.Equal(VoikkoSpellChecker.GetVoikkoFolder(userFolder), VoikkoSpellChecker.FindDictionaryRoot(userFolder));
        }
        finally
        {
            Directory.Delete(userFolder, true);
            Directory.Delete(Path.Combine(AppContext.BaseDirectory, "voikko"), true);
        }
    }

    /// <summary>
    /// Runs only when a libvoikko + dictionary is installed in the SE_VOIKKO_DICTIONARIES folder
    /// (the SE dictionaries folder); exercises the real native library end to end.
    /// </summary>
    [Fact]
    public void RealVoikko_SpellsInflectedFinnish()
    {
        var folder = Environment.GetEnvironmentVariable("SE_VOIKKO_DICTIONARIES");
        if (string.IsNullOrEmpty(folder) || !VoikkoSpellChecker.IsAvailable(folder))
        {
            return; // libvoikko not available
        }

        var checker = new SpellChecker();
        Assert.True(checker.Initialize(VoikkoSpellChecker.GetMarkerFile(folder), "fi"));

        // Inflected / compound forms the Hunspell word list typically lacks.
        Assert.True(checker.DoSpell("taloissammekin"));
        Assert.True(checker.DoSpell("lentokonesuihkuturbiinimoottori"));
        Assert.True(checker.IsWordCorrect("kirjoitin"));
        Assert.False(checker.DoSpell("talossx"));

        var suggestions = checker.GetSuggestions("talosa");
        Assert.True(suggestions.Count > 0, "no suggestions");
        Assert.True(suggestions.Contains("talossa"), string.Join(", ", suggestions));

        var languages = checker.GetDictionaryLanguages(folder);
        Assert.True(languages.Any(l => l.Name.Contains("Voikko")), "Voikko not listed");
        var voikko = languages.First(l => l.Name.Contains("Voikko"));
        Assert.Equal("fi", SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(voikko));
        Assert.Equal("fin", voikko.GetThreeLetterCode());
        Assert.Equal("fi_FI", voikko.GetFiveLetterLanguageName());
    }
}
