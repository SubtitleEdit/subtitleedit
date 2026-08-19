using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// "Text box: Delete selection (no clipboard)" grew into the forward-delete (Delete key) command
/// - defaulting to Shift+Backspace on macOS only, so Apple keyboards, whose Delete key is only a
/// backspace, get a real Delete key while PC keyboards keep Shift+Backspace as a backspace. The
/// rename is carried into persisted settings by shortcut migration v2 so user assignments
/// (including a deliberately cleared binding) survive the upgrade.
/// </summary>
public class ShortcutsForwardDeleteMigrationTests
{
    private const string OldName = "TextBoxDeleteSelectionCommand";
    private const string NewName = nameof(MainViewModel.TextBoxDeleteForwardCommand);

    [Fact]
    public void ForwardDeleteDefaultsToShiftBackspaceOnMacOsOnly()
    {
        // GetDefaultShortcuts only uses the vm parameter for nameof() - never dereferenced.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!);

        var forwardDelete = defaults.SingleOrDefault(s => s.ActionName == NewName);
        if (OperatingSystem.IsMacOS())
        {
            Assert.Equal(new[] { "Shift", "Back" }, forwardDelete!.Keys);
        }
        else
        {
            // No default off macOS - the command stays bindable in the shortcuts dialog.
            Assert.Null(forwardDelete);
        }

        Assert.DoesNotContain(defaults, s => s.ActionName == OldName);
    }

    [Fact]
    public void MigrationRenamesPersistedEntryAndKeepsCustomKeys()
    {
        var settings = new Se();
        settings.Shortcuts.Add(new SeShortCut(OldName, ["Control", "D"]));

        settings.MigrateShortcuts();

        Assert.Equal(new[] { "Control", "D" }, settings.Shortcuts.Single(s => s.ActionName == NewName).Keys);
        Assert.DoesNotContain(settings.Shortcuts, s => s.ActionName == OldName);
        Assert.Equal(Se.CurrentShortcutsMigrationVersion, settings.ShortcutsMigrationVersion);
    }

    [Fact]
    public void MigratedClearedBindingIsNotRevivedByDefaultsMerge()
    {
        // The user unbound the old command on purpose; after rename the defaults merge must not
        // hand the renamed command its Shift+Backspace default back.
        var settings = new Se();
        settings.Shortcuts.Add(new SeShortCut(OldName, []));

        settings.InitializeMainShortcuts(null!);

        Assert.Empty(settings.Shortcuts.Single(s => s.ActionName == NewName).Keys);
    }

    [Fact]
    public void MigrationFromVersion1OnlyRunsVersion2Step()
    {
        // A user already at migration v1 re-assigned F10 afterwards; jumping to v2 must apply
        // only the rename and leave the v1-covered F10 binding alone.
        var settings = new Se { ShortcutsMigrationVersion = 1 };
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.WaveformSetEndAndGoToNextCommand), ["F10"]));
        settings.Shortcuts.Add(new SeShortCut(OldName, ["Shift", "Back"]));

        settings.MigrateShortcuts();

        Assert.Equal(new[] { "F10" }, settings.Shortcuts[0].Keys);
        Assert.Equal(NewName, settings.Shortcuts[1].ActionName);
    }
}
