using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// Bare F10 must activate the main menu bar (Windows standard, #11745) unless the user
/// themselves assigned F10 to an action (#12504). v5.0.0 - v5.2.0-beta1 shipped F10 as the
/// default for "set end and go to next", and visiting the Shortcuts window persisted that
/// default to Settings.json - permanently disabling F10 menu activation (#13083). The default
/// is gone and a one-time migration clears the stale persisted copy.
/// </summary>
public class ShortcutsF10MenuTests
{
    private const string WaveformSetEndAndGoToNext = nameof(MainViewModel.WaveformSetEndAndGoToNextCommand);

    [Fact]
    public void DefaultShortcutsDoNotBindBareF10()
    {
        // GetDefaultShortcuts only uses the vm parameter for nameof() - never dereferenced.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!);

        Assert.DoesNotContain(defaults, s => s.Keys.Count == 1 &&
            s.Keys[0].Equals("F10", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MigrationClearsStaleDefaultF10Binding()
    {
        var settings = new Se();
        settings.Shortcuts.Add(new SeShortCut(WaveformSetEndAndGoToNext, ["F10"]));

        settings.MigrateShortcuts();

        Assert.Empty(settings.Shortcuts.Single(s => s.ActionName == WaveformSetEndAndGoToNext).Keys);
        Assert.Equal(Se.CurrentShortcutsMigrationVersion, settings.ShortcutsMigrationVersion);
    }

    [Fact]
    public void MigrationKeepsF10AssignedAfterMigrationRan()
    {
        // The user re-assigned F10 to the action after the one-time migration - it must stick.
        var settings = new Se
        {
            ShortcutsMigrationVersion = Se.CurrentShortcutsMigrationVersion,
        };
        settings.Shortcuts.Add(new SeShortCut(WaveformSetEndAndGoToNext, ["F10"]));

        settings.MigrateShortcuts();

        Assert.Equal(new[] { "F10" }, settings.Shortcuts.Single(s => s.ActionName == WaveformSetEndAndGoToNext).Keys);
    }

    [Fact]
    public void MigrationLeavesOtherBindingsAlone()
    {
        // F10 on any other action is a real user assignment; a modified binding on the
        // waveform action is too. Neither may be touched.
        var settings = new Se();
        settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.TogglePlayPauseCommand), ["F10"]));
        settings.Shortcuts.Add(new SeShortCut(WaveformSetEndAndGoToNext, ["Control", "F10"]));

        settings.MigrateShortcuts();

        Assert.Equal(new[] { "F10" }, settings.Shortcuts[0].Keys);
        Assert.Equal(new[] { "Control", "F10" }, settings.Shortcuts[1].Keys);
    }

    [Fact]
    public void MigrationRunsOnlyOnce()
    {
        var settings = new Se();

        settings.MigrateShortcuts();
        settings.Shortcuts.Add(new SeShortCut(WaveformSetEndAndGoToNext, ["F10"]));
        settings.MigrateShortcuts();

        Assert.Equal(new[] { "F10" }, settings.Shortcuts.Single(s => s.ActionName == WaveformSetEndAndGoToNext).Keys);
    }
}
