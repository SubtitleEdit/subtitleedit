using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class InterjectionsSettingsTests
{
    [Fact]
    public void Defaults_HaveListsForSeededLanguages()
    {
        var defaults = SeRemoveTextForHi.CreateDefaultInterjections();

        foreach (var code in new[] { "da", "de", "en", "es", "et", "fr", "it", "nb", "nl", "pl", "sv" })
        {
            var language = defaults.Single(p => p.LanguageCode == code);
            Assert.NotEmpty(language.Interjections);
        }

        Assert.Contains("Uh-huh", defaults.Single(p => p.LanguageCode == "en").Interjections);
        Assert.Contains("Ah bon", defaults.Single(p => p.LanguageCode == "fr").SkipStartList);
    }

    [Fact]
    public void Load_OldSettingsWithOnlyEnglish_AddsOtherLanguagesAndKeepsUserEnglish()
    {
        var savedSettings = Se.Settings;
        var path = Path.Combine(Path.GetTempPath(), "se_interjections_" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            Se.Settings = new Se();
            Se.Settings.Tools.RemoveTextForHi.Interjections =
            [
                new SeRemoveTextForHi.InterjectionLanguage
                {
                    LanguageCode = "en",
                    Interjections = ["Mine"],
                    SkipStartList = [],
                },
            ];

            Se.SaveSettings(path);
            Se.Settings = new Se();
            Se.LoadSettings(path);

            var interjections = Se.Settings.Tools.RemoveTextForHi.Interjections;
            Assert.Equal(["Mine"], interjections.Single(p => p.LanguageCode == "en").Interjections);
            Assert.Contains("Øh", interjections.Single(p => p.LanguageCode == "da").Interjections);
            Assert.Single(interjections, p => p.LanguageCode == "de");
        }
        finally
        {
            Se.Settings = savedSettings;
            File.Delete(path);
        }
    }
}
