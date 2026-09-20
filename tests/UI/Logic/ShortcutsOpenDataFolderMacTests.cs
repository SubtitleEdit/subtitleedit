using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The macOS default for "Open Subtitle Edit folder" was Option+Shift+Cmd+D, which never reached
/// the app (#14508); adding Control makes the chord work. Shortcut migration v3 moves persisted
/// mac settings still on the dead chord onto the new one while leaving user re-bindings alone.
/// </summary>
public class ShortcutsOpenDataFolderMacTests
{
    private const string Name = nameof(MainViewModel.OpenDataFolderCommand);

    [Fact]
    public void DefaultIncludesControlOnMacOsOnly()
    {
        var keys = ShortcutsMain.GetDefaultShortcuts(null!).Single(s => s.ActionName == Name).Keys;
        if (OperatingSystem.IsMacOS())
        {
            Assert.Equal(new[] { "Ctrl", "Win", "Alt", "Shift", "D" }, keys);
        }
        else
        {
            Assert.Equal(new[] { "Ctrl", "Alt", "Shift", "D" }, keys);
        }
    }

    [Fact]
    public void MigrationMovesOldMacDefaultAndKeepsCustomKeys()
    {
        var settings = new Se();
        settings.Shortcuts.Add(new SeShortCut(Name, ["Win", "Alt", "Shift", "D"]));
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.SaveLanguageFileCommand), ["Win", "Alt", "Shift", "L"]));

        settings.MigrateShortcuts();

        var expected = OperatingSystem.IsMacOS()
            ? new[] { "Ctrl", "Win", "Alt", "Shift", "D" }
            : new[] { "Win", "Alt", "Shift", "D" };
        Assert.Equal(expected, settings.Shortcuts[0].Keys);
        Assert.Equal(new[] { "Win", "Alt", "Shift", "L" }, settings.Shortcuts[1].Keys);
        Assert.Equal(Se.CurrentShortcutsMigrationVersion, settings.ShortcutsMigrationVersion);
    }

    [Fact]
    public void MigrationLeavesUserReboundKeysAlone()
    {
        var settings = new Se();
        settings.Shortcuts.Add(new SeShortCut(Name, ["Win", "Shift", "F9"]));

        settings.MigrateShortcuts();

        Assert.Equal(new[] { "Win", "Shift", "F9" }, settings.Shortcuts[0].Keys);
    }
}
