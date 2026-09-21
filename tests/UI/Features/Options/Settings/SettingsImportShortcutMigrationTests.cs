using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Options.Settings;

/// <summary>
/// Imported shortcuts replace the list the shortcut migrations already ran on, so the import has
/// to run them again - and translate the macOS-only defaults (#14941) when the file crosses to or
/// from macOS, which renaming Ctrl and Cmd alone does not do.
/// </summary>
public class SettingsImportShortcutMigrationTests
{
    private static List<string> Keys(Se settings, string actionName) => settings.Shortcuts.Single(s => s.ActionName == actionName).Keys;

    [Fact]
    public void OldMacOsExportOnMacOs_MovesToTheNewDefaults()
    {
        var settings = new Se { ShortcutsMigrationVersion = Se.CurrentShortcutsMigrationVersion };
        var imported = new List<SeShortCut>
        {
            new(nameof(MainViewModel.ShowReplaceCommand), ["Win", "H"]),
            new(nameof(MainViewModel.ShowGoToLineCommand), ["Win", "G"]),
            new(nameof(MainViewModel.FindNextCommand), ["F3"]),
        };

        SettingsImportExportViewModel.ApplyImportedShortcuts(settings, imported, null, "MacOS", "MacOS");

        Assert.Equal(new[] { "Win", "Alt", "F" }, Keys(settings, nameof(MainViewModel.ShowReplaceCommand)));
        Assert.Equal(new[] { "Ctrl", "G" }, Keys(settings, nameof(MainViewModel.ShowGoToLineCommand)));
        Assert.Equal(new[] { "Win", "G" }, Keys(settings, nameof(MainViewModel.FindNextCommand)));
        Assert.Equal(Se.CurrentShortcutsMigrationVersion, settings.ShortcutsMigrationVersion);
    }

    [Fact]
    public void CurrentMacOsExportOnMacOs_KeepsADeliberateOldBinding()
    {
        var settings = new Se();
        var imported = new List<SeShortCut> { new(nameof(MainViewModel.ShowReplaceCommand), ["Win", "H"]) };

        SettingsImportExportViewModel.ApplyImportedShortcuts(settings, imported, Se.CurrentShortcutsMigrationVersion, "MacOS", "MacOS");

        Assert.Equal(new[] { "Win", "H" }, Keys(settings, nameof(MainViewModel.ShowReplaceCommand)));
    }

    [Fact]
    public void WindowsExportOnMacOs_GetsTheMacOsDefaults()
    {
        var settings = new Se();
        var imported = new List<SeShortCut>
        {
            new(nameof(MainViewModel.ShowReplaceCommand), ["Ctrl", "H"]),
            new(nameof(MainViewModel.RedoCommand), ["Ctrl", "Y"]),
            new(nameof(MainViewModel.DeleteSelectedLinesCommand), ["Delete"]),
            new(nameof(MainViewModel.OpenDataFolderCommand), ["Ctrl", "Alt", "Shift", "D"]),
            new(nameof(MainViewModel.CommandFileSaveCommand), ["Ctrl", "S"]),
        };

        // a Windows file at the current version: the macOS-only steps never ran on it
        SettingsImportExportViewModel.ApplyImportedShortcuts(settings, imported, Se.CurrentShortcutsMigrationVersion, "Windows", "MacOS");

        Assert.Equal(new[] { "Win", "Alt", "F" }, Keys(settings, nameof(MainViewModel.ShowReplaceCommand)));
        Assert.Equal(new[] { "Win", "Shift", "Z" }, Keys(settings, nameof(MainViewModel.RedoCommand)));
        Assert.Equal(new[] { "Win", "Back" }, Keys(settings, nameof(MainViewModel.DeleteSelectedLinesCommand)));
        Assert.Equal(new[] { "Ctrl", "Win", "Alt", "Shift", "D" }, Keys(settings, nameof(MainViewModel.OpenDataFolderCommand)));
        Assert.Equal(new[] { "Win", "S" }, Keys(settings, nameof(MainViewModel.CommandFileSaveCommand)));
    }

    [Fact]
    public void MacOsExportOnWindows_GetsTheSharedDefaults_WithoutDuplicates()
    {
        var settings = new Se();
        var imported = new List<SeShortCut>
        {
            new(nameof(MainViewModel.FindNextCommand), ["Win", "G"]),
            new(nameof(MainViewModel.ShowGoToLineCommand), ["Ctrl", "G"]),
            new(nameof(MainViewModel.DeleteSelectedLinesCommand), ["Win", "Back"]),
            new(nameof(MainViewModel.OpenDataFolderCommand), ["Ctrl", "Win", "Alt", "Shift", "D"]),
            new(nameof(MainViewModel.ShowReplaceCommand), ["Win", "Shift", "F9"]), // the user's own
        };

        SettingsImportExportViewModel.ApplyImportedShortcuts(settings, imported, Se.CurrentShortcutsMigrationVersion, "MacOS", "Windows");

        Assert.Equal(new[] { "F3" }, Keys(settings, nameof(MainViewModel.FindNextCommand)));
        Assert.Equal(new[] { "Ctrl", "G" }, Keys(settings, nameof(MainViewModel.ShowGoToLineCommand)));
        Assert.Equal(new[] { "Delete" }, Keys(settings, nameof(MainViewModel.DeleteSelectedLinesCommand)));
        Assert.Equal(new[] { "Ctrl", "Alt", "Shift", "D" }, Keys(settings, nameof(MainViewModel.OpenDataFolderCommand)));
        Assert.Equal(new[] { "Ctrl", "Shift", "F9" }, Keys(settings, nameof(MainViewModel.ShowReplaceCommand)));
    }
}
