using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Options.Shortcuts;

/// <summary>
/// SE 4 keeps the "toggle custom tags" characters in General settings (TagsInToggleCustomTags,
/// one "startÆend" string) rather than with the shortcut, so importing SE 4 shortcuts used to
/// carry the key but not the pair it toggled (#14232, matter L). The pair now lands on a
/// surround-with slot and the imported key follows it there.
/// </summary>
public class Se4CustomTagsImportTests
{
    private const string ToggleKey = "<MainListViewToggleCustomTags>Control+Shift+D</MainListViewToggleCustomTags>";

    private static string SettingsXml(string? customTags, string shortcuts = ToggleKey)
    {
        var general = customTags == null
            ? string.Empty
            : $"<General><TagsInToggleCustomTags>{customTags}</TagsInToggleCustomTags></General>";
        return $"<Settings>{general}<Shortcuts>{shortcuts}</Shortcuts></Settings>";
    }

    [Theory]
    [InlineData("(Æ)", "(", ")")]
    // SE 4 writes the pair XML-escaped; the reader must hand back the decoded characters.
    [InlineData("&lt;i&gt;Æ&lt;/i&gt;", "<i>", "</i>")]
    // SE 4 reads a single field as both sides.
    [InlineData("#", "#", "#")]
    public void ImportReadsTheCustomTagPair(string stored, string start, string end)
    {
        var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml(stored));

        Assert.Equal(start, result.CustomTagsStart);
        Assert.Equal(end, result.CustomTagsEnd);
    }

    [Fact]
    public void ImportHasNoPairWhenTheFileHasNoGeneralSection()
    {
        // An exported SE_Shortcuts.xml is rooted at <Shortcuts> and carries no General settings.
        var result = Se4ShortcutsImporter.ImportFromXml($"<Shortcuts>{ToggleKey}</Shortcuts>");

        Assert.Null(result.CustomTagsStart);
        Assert.Null(result.CustomTagsEnd);
    }

    [Fact]
    public void ImportHasNoPairWhenBothSidesAreEmpty()
    {
        var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml("Æ"));

        Assert.Null(result.CustomTagsStart);
    }

    private static void WithSettings(Se settings, Action<ShortcutsViewModel> act)
    {
        var previous = Se.Settings;
        Se.Settings = settings;
        try
        {
            act(new ShortcutsViewModel(null!, null!));
        }
        finally
        {
            Se.Settings = previous;
        }
    }

    [Fact]
    public void CustomTagsLandOnTheFirstFreeSlotAndTheKeyFollows()
    {
        var settings = new Se();

        WithSettings(settings, vm =>
        {
            var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml("(Æ)"));
            vm.ApplySe4CustomTags(result);

            // Slots 1-3 ship configured, so SE 4's pair takes slot 4 - the music-symbol pair on
            // slot 1 is what an SE 4 user would miss most if the import overwrote it.
            Assert.Equal("(", settings.GetSurroundLeft(4));
            Assert.Equal(")", settings.GetSurroundRight(4));
            Assert.Equal("♪", settings.Surround1Left);
            Assert.Equal(
                nameof(MainViewModel.SurroundWith4Command),
                Assert.Single(result.Shortcuts).ActionName);
        });
    }

    [Fact]
    public void CustomTagsReuseASlotThatAlreadyHoldsThePair()
    {
        var settings = new Se();

        WithSettings(settings, vm =>
        {
            var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml("[Æ]"));
            vm.ApplySe4CustomTags(result);

            // Slot 3 is "[" / "]" out of the box - no reason to spend a second slot on it.
            Assert.Equal(
                nameof(MainViewModel.SurroundWith3Command),
                Assert.Single(result.Shortcuts).ActionName);
            Assert.Empty(settings.GetSurroundLeft(4));
        });
    }

    [Fact]
    public void CustomTagsAreLeftAloneWhenEverySlotIsInUse()
    {
        var settings = new Se();
        for (var slot = 1; slot <= Se.SurroundWithSlotCount; slot++)
        {
            settings.SetSurround(slot, "<" + slot, ">");
        }

        WithSettings(settings, vm =>
        {
            var result = Se4ShortcutsImporter.ImportFromXml(SettingsXml("(Æ)"));
            vm.ApplySe4CustomTags(result);

            Assert.Equal(
                nameof(MainViewModel.SurroundWith1Command),
                Assert.Single(result.Shortcuts).ActionName);
            Assert.Equal("<1", settings.Surround1Left);
        });
    }

    // SE 4 ships "(Æ)" as the default pair, so a file where the shortcut itself was never
    // assigned must not spend a slot on characters the user never chose.
    [Fact]
    public void CustomTagsAreIgnoredWhenTheShortcutIsUnassigned()
    {
        var settings = new Se();

        WithSettings(settings, vm =>
        {
            var result = Se4ShortcutsImporter.ImportFromXml(
                SettingsXml("(Æ)", "<MainListViewToggleCustomTags></MainListViewToggleCustomTags>"));
            vm.ApplySe4CustomTags(result);

            Assert.Empty(settings.GetSurroundLeft(4));
            Assert.Empty(result.Shortcuts);
        });
    }
}
