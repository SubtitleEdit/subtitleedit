using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Dialogs (Spell check, AI review, Review speech, Visual Sync) match main-window bindings via
/// MainShortcutKeys - an AltGr binding (#15618) must fire there too, and AltGr must not fire a
/// Ctrl+Alt binding, same as ShortcutManager.
/// </summary>
public class MainShortcutKeysAltGrTests
{
    private static KeyEventArgs KeyEvent(Key key, KeyModifiers modifiers)
        => new() { Key = key, KeyModifiers = modifiers };

    private static bool Matches(bool altGrHeld, Key key, KeyModifiers modifiers, params string[] keys)
    {
        var original = MainShortcutKeys.IsAltGrHeld;
        try
        {
            MainShortcutKeys.IsAltGrHeld = () => altGrHeld;
            return MainShortcutKeys.MatchesKeys(KeyEvent(key, modifiers), keys);
        }
        finally
        {
            MainShortcutKeys.IsAltGrHeld = original;
        }
    }

    [Fact]
    public void AltGrBindingMatchesAltGr()
    {
        Assert.True(Matches(true, Key.K, KeyModifiers.Control | KeyModifiers.Alt, ShortcutManager.AltGrToken, "K"));
    }

    [Fact]
    public void ShiftAltGrBindingMatchesShiftAltGr()
    {
        Assert.True(Matches(true, Key.K, KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift,
            ShortcutManager.AltGrToken, "Shift", "K"));
        Assert.False(Matches(true, Key.K, KeyModifiers.Control | KeyModifiers.Alt, ShortcutManager.AltGrToken, "Shift", "K"));
    }

    [Fact]
    public void AltGrBindingDoesNotMatchLeftCtrlAlt()
    {
        Assert.False(Matches(false, Key.K, KeyModifiers.Control | KeyModifiers.Alt, ShortcutManager.AltGrToken, "K"));
    }

    [Fact]
    public void AltGrBindingDoesNotMatchPlainKey()
    {
        Assert.False(Matches(false, Key.K, KeyModifiers.None, ShortcutManager.AltGrToken, "K"));
    }

    [Fact]
    public void CtrlAltBindingMatchesOnlyWithoutAltGr()
    {
        Assert.True(Matches(false, Key.K, KeyModifiers.Control | KeyModifiers.Alt, "Ctrl", "Alt", "K"));
        Assert.False(Matches(true, Key.K, KeyModifiers.Control | KeyModifiers.Alt, "Ctrl", "Alt", "K"));
    }

    [Fact]
    public void OtherBindingsUnaffected()
    {
        Assert.True(Matches(false, Key.F5, KeyModifiers.None, "F5"));
        Assert.True(Matches(true, Key.Space, KeyModifiers.Control, "Ctrl", "Space"));
        Assert.False(Matches(false, Key.K, KeyModifiers.Control, "Ctrl", "Alt", "K"));
    }
}
