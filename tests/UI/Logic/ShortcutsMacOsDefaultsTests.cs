using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// Some defaults moved off standard macOS shortcuts (#14941): Cmd+H hides the app, Cmd+Space opens
/// Spotlight, F11 shows the desktop, Cmd+G is find next and Cmd+Shift+Z redo, Apple keyboards
/// have no Insert key or forward Delete key, and Option+letter types a character (#14508).
/// Shortcut migration v4 moves persisted mac settings still on the old defaults, leaving user
/// re-bindings and Windows/Linux alone.
/// </summary>
public class ShortcutsMacOsDefaultsTests
{
    private static List<string>? DefaultKeys(bool isMacOS, string actionName)
    {
        return ShortcutsMain.GetDefaultShortcuts(null!, isMacOS).SingleOrDefault(s => s.ActionName == actionName)?.Keys;
    }

    [Fact]
    public void MacOsDefaultsUseTheNewKeys()
    {
        foreach (var change in ShortcutsMain.MacOsDefaultChanges)
        {
            var keys = DefaultKeys(true, change.ActionName);
            if (change.NewKeys.Length == 0)
            {
                Assert.Null(keys);
            }
            else
            {
                Assert.Equal(change.NewKeys, keys);
            }
        }
    }

    [Fact]
    public void WindowsAndLinuxDefaultsAreUnchanged()
    {
        foreach (var change in ShortcutsMain.MacOsDefaultChanges)
        {
            var expected = change.OldKeys.Select(k => k == "Win" ? "Ctrl" : k);
            Assert.Equal(expected, DefaultKeys(false, change.ActionName));
        }
    }

    [Fact]
    public void MacOsDefaultsDoNotBindSystemShortcuts()
    {
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!, true);

        Assert.DoesNotContain(defaults, s => IsKeys(s.Keys, "Win", "H"));
        Assert.DoesNotContain(defaults, s => IsKeys(s.Keys, "Win", "Space"));
        Assert.DoesNotContain(defaults, s => IsKeys(s.Keys, "F11"));
        Assert.DoesNotContain(defaults, s => s.Keys.Contains("Insert"));
    }

    [Fact]
    public void MacOsDefaultsDoNotBindOptionLetterChords()
    {
        // Option(+Shift)+letter types a character on macOS (Option+Shift+E is È on Italian layouts),
        // and a General shortcut on it would swallow that character in the text box.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!, true);

        Assert.DoesNotContain(defaults, s =>
            s.Keys.Contains("Alt") &&
            !s.Keys.Contains("Win") && !s.Keys.Contains("Ctrl") &&
            s.Keys.Any(k => (k.Length == 1 && char.IsLetter(k[0])) || (k.Length == 2 && k[0] == 'D' && char.IsDigit(k[1]))));
    }

    [Fact]
    public void MacOsNewKeysAreNotUsedByAnyOtherDefault()
    {
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!, true);
        foreach (var change in ShortcutsMain.MacOsDefaultChanges.Where(c => c.NewKeys.Length > 0))
        {
            Assert.DoesNotContain(defaults, s => s.ActionName != change.ActionName && IsKeys(s.Keys, change.NewKeys));
        }
    }

    [Fact]
    public void MigrationMovesOldMacDefaultsAndKeepsCustomKeys()
    {
        var settings = new Se();
        foreach (var change in ShortcutsMain.MacOsDefaultChanges)
        {
            settings.Shortcuts.Add(new SeShortCut(change.ActionName, [.. change.OldKeys]));
        }
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.UndoCommand), ["Win", "Z"]));

        settings.MigrateShortcuts(isMacOS: true);

        foreach (var change in ShortcutsMain.MacOsDefaultChanges)
        {
            Assert.Equal(change.NewKeys, settings.Shortcuts.Single(s => s.ActionName == change.ActionName).Keys);
        }
        Assert.Equal(["Win", "Z"], settings.Shortcuts.Single(s => s.ActionName == nameof(MainViewModel.UndoCommand)).Keys);
        Assert.Equal(Se.CurrentShortcutsMigrationVersion, settings.ShortcutsMigrationVersion);
    }

    [Fact]
    public void MigrationLeavesUserReboundKeysAndOtherPlatformsAlone()
    {
        var rebound = new Se();
        rebound.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.ShowReplaceCommand), ["Win", "Shift", "H"]));
        rebound.MigrateShortcuts(isMacOS: true);
        Assert.Equal(["Win", "Shift", "H"], rebound.Shortcuts[0].Keys);

        var windows = new Se();
        windows.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.ShowReplaceCommand), ["Win", "H"]));
        windows.MigrateShortcuts(isMacOS: false);
        Assert.Equal(["Win", "H"], windows.Shortcuts[0].Keys);
    }

    [Fact]
    public void MigrationSkipsMovesThatWouldDuplicateABinding()
    {
        // Ctrl+G is taken by the user, so go-to-line keeps Cmd+G - and then find next must keep
        // F3 too, or it would land on go-to-line's Cmd+G.
        var settings = new Se();
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.FocusSelectedLineCommand), ["Ctrl", "G"]));
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.ShowGoToLineCommand), ["Win", "G"]));
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.FindNextCommand), ["F3"]));
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.ShowReplaceCommand), ["Win", "H"]));

        settings.MigrateShortcuts(isMacOS: true);

        Assert.Equal(["Ctrl", "G"], settings.Shortcuts[0].Keys);
        Assert.Equal(["Win", "G"], settings.Shortcuts[1].Keys);
        Assert.Equal(["F3"], settings.Shortcuts[2].Keys);
        Assert.Equal(["Win", "Alt", "F"], settings.Shortcuts[3].Keys);
    }

    [Fact]
    public void MigrationRunsOnlyOnce()
    {
        var settings = new Se();
        settings.MigrateShortcuts(isMacOS: true);
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.ShowReplaceCommand), ["Win", "H"]));

        settings.MigrateShortcuts(isMacOS: true);

        Assert.Equal(["Win", "H"], settings.Shortcuts[0].Keys);
    }

    private static bool IsKeys(List<string> keys, params string[] expected)
    {
        return keys.Count == expected.Length && expected.All(e => keys.Contains(e, StringComparer.OrdinalIgnoreCase));
    }
}
