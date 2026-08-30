using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Options.Shortcuts;

/// <summary>
/// SE 4 keeps its five custom search slots in VideoControls settings (CustomSearchTextN +
/// CustomSearchUrlN) rather than with the shortcuts that fire them, so importing SE 4 shortcuts
/// has to carry both - a key pointing at a slot that searches a different site is worse than no
/// key at all.
/// </summary>
public class Se4CustomSearchImportTests
{
    private const string SearchKey = "<MainTranslateCustomSearch1>Control+Shift+W</MainTranslateCustomSearch1>";

    private static string SettingsXml(string videoControls, string shortcuts = SearchKey)
    {
        return $"<Settings><VideoControls>{videoControls}</VideoControls><Shortcuts>{shortcuts}</Shortcuts></Settings>";
    }

    [Fact]
    public void ImportMapsTheCustomSearchShortcuts()
    {
        var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml(string.Empty));

        var imported = Assert.Single(result.Shortcuts);
        Assert.Equal(nameof(MainViewModel.CustomSearch1Command), imported.ActionName);
    }

    [Fact]
    public void ImportReadsTheNameAndUrlOfEverySlotWithAUrl()
    {
        var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml(
            "<CustomSearchText1>Wiktionary</CustomSearchText1>" +
            "<CustomSearchUrl1>https://en.wiktionary.org/wiki/{0}</CustomSearchUrl1>" +
            // A name without a URL is not a slot - SE 4 hides it too.
            "<CustomSearchText2>No URL</CustomSearchText2>" +
            "<CustomSearchUrl2></CustomSearchUrl2>"));

        var slot = Assert.Single(result.CustomSearches);
        Assert.Equal(1, slot.Key);
        Assert.Equal("Wiktionary", slot.Value.Name);
        Assert.Equal("https://en.wiktionary.org/wiki/{0}", slot.Value.Url);
    }

    [Fact]
    public void ImportHasNoSearchesWhenTheFileHasNoVideoControlsSection()
    {
        // An exported SE_Shortcuts.xml is rooted at <Shortcuts> and carries no other settings.
        var result = Se4ShortcutsImporter.ImportFromXml($"<Shortcuts>{SearchKey}</Shortcuts>");

        Assert.Empty(result.CustomSearches);
    }

    [Fact]
    public void ImportedSlotsOverwriteTheSe5OnesSoTheKeySearchesTheSameSite()
    {
        var settings = new Se();
        var previous = Se.Settings;
        Se.Settings = settings;
        try
        {
            var vm = new ShortcutsViewModel(null!, null!);
            var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml(
                "<CustomSearchText1>Wiktionary</CustomSearchText1>" +
                "<CustomSearchUrl1>https://en.wiktionary.org/wiki/{0}</CustomSearchUrl1>"));

            vm.ApplySe4CustomSearches(result);

            Assert.Equal("Wiktionary", settings.GetCustomSearchName(1));
            Assert.Equal("https://en.wiktionary.org/wiki/{0}", settings.GetCustomSearchUrl(1));
            // Slots SE 4 says nothing about keep SE 5's defaults.
            Assert.Equal("Wikipedia", settings.GetCustomSearchName(2));
        }
        finally
        {
            Se.Settings = previous;
        }
    }
}
