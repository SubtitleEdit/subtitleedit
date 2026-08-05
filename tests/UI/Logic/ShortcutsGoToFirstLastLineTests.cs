using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Ctrl+Home / Ctrl+End default to "go to first/last line", which also move the video
/// position unless turned off (#13194). The reporter uses the numpad Home key, so a plain
/// "Ctrl+Home" binding must fire from the NumLock-off numpad Home too - via the last-resort
/// collapse stage in CheckShortcuts - while a numpad-specific binding keeps winning (#10934).
/// </summary>
public class ShortcutsGoToFirstLastLineTests
{
    private static KeyEventArgs KeyEvent(Key key, PhysicalKey physicalKey, KeyModifiers modifiers)
    {
        return new KeyEventArgs
        {
            Key = key,
            PhysicalKey = physicalKey,
            KeyModifiers = modifiers,
        };
    }

    [Fact]
    public void DefaultsBindCtrlHomeAndCtrlEndToGoToFirstAndLastLine()
    {
        // GetDefaultShortcuts only uses the vm parameter for nameof() - never dereferenced.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!);

        var first = defaults.Single(s => s.ActionName == nameof(MainViewModel.GoToFirstLineCommand));
        var last = defaults.Single(s => s.ActionName == nameof(MainViewModel.GoToLastLineCommand));

        Assert.Contains("Home", first.Keys);
        Assert.Contains("End", last.Keys);
        Assert.Equal(2, first.Keys.Count); // modifier (Ctrl/Cmd) + Home
        Assert.Equal(2, last.Keys.Count);
    }

    [Fact]
    public void PlainCtrlHomeBindingFiresFromNumpadHome()
    {
        var manager = new ShortcutManager();
        var category = ShortcutCategory.General;
        var command = new RelayCommand(() => { });
        manager.RegisterShortcut(new ShortCut("Go to first line", ["Ctrl", "Home"], category, command));

        var numpadHome = KeyEvent(Key.Home, PhysicalKey.NumPad7, KeyModifiers.Control);
        manager.OnKeyPressed(null, numpadHome);

        Assert.Same(command, manager.CheckShortcuts(numpadHome, category.ToString()));
    }

    [Fact]
    public void NumpadSpecificBindingStillWinsOverPlainBinding()
    {
        var manager = new ShortcutManager();
        var category = ShortcutCategory.General;
        var plainCommand = new RelayCommand(() => { });
        var numpadCommand = new RelayCommand(() => { });
        manager.RegisterShortcut(new ShortCut("Plain", ["Ctrl", "Home"], category, plainCommand));
        manager.RegisterShortcut(new ShortCut("Numpad", ["Ctrl", "NumPadHome"], category, numpadCommand));

        var numpadHome = KeyEvent(Key.Home, PhysicalKey.NumPad7, KeyModifiers.Control);
        manager.OnKeyPressed(null, numpadHome);
        Assert.Same(numpadCommand, manager.CheckShortcuts(numpadHome, category.ToString()));

        manager.ClearKeys();
        var mainHome = KeyEvent(Key.Home, PhysicalKey.Home, KeyModifiers.Control);
        manager.OnKeyPressed(null, mainHome);
        Assert.Same(plainCommand, manager.CheckShortcuts(mainHome, category.ToString()));
    }

    [Fact]
    public void MainHomeDoesNotFireNumpadOnlyBinding()
    {
        // The reverse direction stays independent (#10934): a binding placed
        // specifically on the numpad key must not react to the main-keyboard key.
        var manager = new ShortcutManager();
        var category = ShortcutCategory.General;
        var command = new RelayCommand(() => { });
        manager.RegisterShortcut(new ShortCut("Numpad only", ["Ctrl", "NumPadHome"], category, command));

        var mainHome = KeyEvent(Key.Home, PhysicalKey.Home, KeyModifiers.Control);
        manager.OnKeyPressed(null, mainHome);

        Assert.Null(manager.CheckShortcuts(mainHome, category.ToString()));
    }

    [Theory]
    [InlineData("NumPadHome", "Home")]
    [InlineData("NumPadEnd", "End")]
    [InlineData("NumPadDelete", "Delete")]
    [InlineData("NumPadInsert", "Insert")]
    // Digits, Decimal and Enter stay fully distinct across NumLock states.
    [InlineData("NumPad7", "NumPad7")]
    [InlineData("NumPadDecimal", "NumPadDecimal")]
    [InlineData("NumPadReturn", "NumPadReturn")]
    [InlineData("Home", "Home")]
    public void CollapseNumPadNavigationTokenMapsOnlyNavigationKeys(string token, string expected)
    {
        Assert.Equal(expected, ShortcutManager.CollapseNumPadNavigationToken(token));
    }
}
