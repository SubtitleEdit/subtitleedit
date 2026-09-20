using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Alt+Space opens the Windows system menu by default, but a shortcut the user bound to that
/// chord must win (#14536) - the same rule bare F10 follows for the menu bar (#12504).
/// </summary>
public class ShortcutsAltSpaceSystemMenuTests
{
    private static ShortcutManager ManagerWith(params string[] keys)
    {
        var manager = new ShortcutManager();
        manager.RegisterShortcut(new ShortCut("AutoBreak", [.. keys], ShortcutCategory.General, new RelayCommand(() => { })));
        return manager;
    }

    [Fact]
    public void HasShortcutMatchesAltSpaceRegardlessOfOrderAndCase()
    {
        Assert.True(ManagerWith("Alt", "Space").HasShortcut("Alt", nameof(Key.Space)));
        Assert.True(ManagerWith("space", "alt").HasShortcut("Alt", nameof(Key.Space)));
    }

    [Fact]
    public void HasShortcutRejectsSupersetsSubsetsAndOtherChords()
    {
        Assert.False(ManagerWith("Control", "Alt", "Space").HasShortcut("Alt", nameof(Key.Space)));
        Assert.False(ManagerWith("Space").HasShortcut("Alt", nameof(Key.Space)));
        Assert.False(ManagerWith("Alt", "B").HasShortcut("Alt", nameof(Key.Space)));
        Assert.False(new ShortcutManager().HasShortcut("Alt", nameof(Key.Space)));
    }

    [Fact]
    public void DefaultShortcutsDoNotBindAltSpace()
    {
        // The system menu must keep working for everyone who never remapped Alt+Space.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!);

        Assert.DoesNotContain(defaults, s => s.Keys.Count == 2 &&
            s.Keys.Contains("Alt", StringComparer.OrdinalIgnoreCase) &&
            s.Keys.Contains("Space", StringComparer.OrdinalIgnoreCase));
    }
}
