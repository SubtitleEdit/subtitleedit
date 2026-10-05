using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class InterjectionsSettingsTests
{
    [Fact]
    public void Defaults_HaveListsForSeededLanguages()
    {
        var settings = new SeRemoveTextForHi();

        foreach (var code in new[] { "da", "de", "en", "es", "et", "fr", "it", "nb", "nl", "pl", "sv" })
        {
            Assert.NotEmpty(settings.GetInterjections(code)!.Interjections);
        }

        Assert.Contains("Uh-huh", settings.GetInterjections("en")!.Interjections);
        Assert.Contains("Ah bon", settings.GetInterjections("fr")!.SkipStartList);
        Assert.Null(settings.GetInterjections("xx"));
        Assert.Empty(settings.Interjections);
    }

    [Fact]
    public void SetInterjections_StoresOnlyDifferencesFromDefaults()
    {
        var settings = new SeRemoveTextForHi();
        var words = settings.GetInterjections("de")!.Interjections.Where(p => p != "Ach").Append("Mine").ToList();

        settings.SetInterjections("de", words, ["Ach so"]);

        var stored = Assert.Single(settings.Interjections);
        Assert.Equal(["Mine"], stored.Added);
        Assert.Equal(["Ach"], stored.Removed);
        Assert.Equal(["Ach so"], stored.SkipStartAdded);
        Assert.Empty(stored.SkipStartRemoved);

        var effective = settings.GetInterjections("de")!;
        Assert.Contains("Mine", effective.Interjections);
        Assert.DoesNotContain("Ach", effective.Interjections);
        Assert.Contains("Wow", effective.Interjections);
    }

    [Fact]
    public void SetInterjections_UnchangedList_StoresNothing()
    {
        var settings = new SeRemoveTextForHi();

        settings.SetInterjections("fr", settings.GetInterjections("fr")!.Interjections, ["Ah bon"]);

        Assert.Empty(settings.Interjections);
    }

    [Fact]
    public void SetInterjections_LanguageWithoutDefaults_StoresAll()
    {
        var settings = new SeRemoveTextForHi();

        settings.SetInterjections("fi", ["Öh", "Hmm"], []);

        Assert.Equal(["Hmm", "Öh"], settings.GetInterjections("fi")!.Interjections);
    }

    [Fact]
    public void Load_OldFullLists_MigratedToDifferences()
    {
        var savedSettings = Se.Settings;
        var path = Path.Combine(Path.GetTempPath(), "se_interjections_" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            Se.Settings = new Se();
            Se.Settings.Tools.RemoveTextForHi.Interjections =
            [
                // old built-in English list with "Gee" removed and "Mine" added
                new SeRemoveTextForHi.InterjectionLanguage
                {
                    LanguageCode = "en",
                    Interjections = ["Ugh", "Oh", "Ah", "Whoa", "Ouch", "Ow", "Hmm", "Uh", "Er", "Uh-huh", "Mine"],
                    SkipStartList = [],
                },
                // no old built-in Danish list, so nothing counts as removed
                new SeRemoveTextForHi.InterjectionLanguage
                {
                    LanguageCode = "da",
                    Interjections = ["Øh", "Hov"],
                    SkipStartList = ["Åh nej"],
                },
            ];

            Se.SaveSettings(path);
            Se.Settings = new Se();
            Se.LoadSettings(path);

            var hi = Se.Settings.Tools.RemoveTextForHi;
            var en = hi.Interjections.Single(p => p.LanguageCode == "en");
            Assert.Equal(["Mine"], en.Added);
            Assert.Equal(["Gee"], en.Removed);
            Assert.Null(en.Interjections);

            var english = hi.GetInterjections("en")!.Interjections;
            Assert.Contains("Mine", english);
            Assert.DoesNotContain("Gee", english);
            Assert.Contains("Phew", english); // new built-in word is not treated as removed

            var danish = hi.GetInterjections("da")!;
            Assert.Contains("Hov", danish.Interjections);
            Assert.Contains("Æh", danish.Interjections); // built-in words the user never saw stay
            Assert.Equal(["Åh nej"], danish.SkipStartList);
        }
        finally
        {
            Se.Settings = savedSettings;
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_OldUnchangedEnglishList_LeavesNoUserEntry()
    {
        var hi = new SeRemoveTextForHi
        {
            Interjections =
            [
                new SeRemoveTextForHi.InterjectionLanguage
                {
                    LanguageCode = "en",
                    Interjections = ["Ugh", "Oh", "Ah", "Whoa", "Gee", "Ouch", "Ow", "Hmm", "Uh", "Er", "Uh-huh"],
                    SkipStartList = [],
                },
            ],
        };

        hi.MigrateFullInterjectionLists();

        Assert.Empty(hi.Interjections);
    }
}
