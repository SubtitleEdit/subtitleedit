using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace UITests.Logic;

public class ShortcutManagerTests
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

    [Theory]
    [InlineData(Key.Home, PhysicalKey.NumPad7, Key.NumPad7)]
    [InlineData(Key.Left, PhysicalKey.NumPad4, Key.NumPad4)]
    [InlineData(Key.Right, PhysicalKey.NumPad6, Key.NumPad6)]
    [InlineData(Key.End, PhysicalKey.NumPad1, Key.NumPad1)]
    [InlineData(Key.Home, PhysicalKey.Home, Key.Home)]
    public void GetShortcutKeyUsesPhysicalKeyForNumpad(Key key, PhysicalKey physicalKey, Key expected)
    {
        var result = ShortcutManager.GetShortcutKey(key, physicalKey);

        Assert.Equal(expected, result);
    }

    // GetShortcutKeyName must produce *distinct* tokens for the same physical
    // numpad key across NumLock states, and for numpad keys vs. their
    // main-keyboard counterparts. This is what lets users bind numpad-Delete
    // (NumLock off) independently from main Delete and from numpad-Decimal
    // (NumLock on) — the bug behind #10934.
    [Theory]
    // NumLock-off keys on the numpad
    [InlineData(Key.Delete, PhysicalKey.NumPadDecimal, "NumPadDelete")]
    [InlineData(Key.Insert, PhysicalKey.NumPad0, "NumPadInsert")]
    [InlineData(Key.End, PhysicalKey.NumPad1, "NumPadEnd")]
    [InlineData(Key.Down, PhysicalKey.NumPad2, "NumPadDown")]
    [InlineData(Key.PageDown, PhysicalKey.NumPad3, "NumPadPageDown")]
    [InlineData(Key.Left, PhysicalKey.NumPad4, "NumPadLeft")]
    [InlineData(Key.Right, PhysicalKey.NumPad6, "NumPadRight")]
    [InlineData(Key.Home, PhysicalKey.NumPad7, "NumPadHome")]
    [InlineData(Key.Up, PhysicalKey.NumPad8, "NumPadUp")]
    [InlineData(Key.PageUp, PhysicalKey.NumPad9, "NumPadPageUp")]
    // NumLock-on numpad keys (Key.ToString() already starts with "NumPad")
    [InlineData(Key.NumPad0, PhysicalKey.NumPad0, "NumPad0")]
    [InlineData(Key.NumPad9, PhysicalKey.NumPad9, "NumPad9")]
    [InlineData(Key.Decimal, PhysicalKey.NumPadDecimal, "NumPadDecimal")]
    // Numpad arithmetic operators (+ - * /) intentionally keep their bare Key names:
    // they're unaffected by NumLock and have no main-keyboard equivalent, and prefixing
    // them would break matching against the Avalonia Key names used by default shortcuts
    // (e.g. Shift+Add waveform zoom). See ShortcutManager.GetShortcutKeyName.
    [InlineData(Key.Add, PhysicalKey.NumPadAdd, "Add")]
    [InlineData(Key.Subtract, PhysicalKey.NumPadSubtract, "Subtract")]
    [InlineData(Key.Divide, PhysicalKey.NumPadDivide, "Divide")]
    [InlineData(Key.Multiply, PhysicalKey.NumPadMultiply, "Multiply")]
    // Main-keyboard keys keep their plain names
    [InlineData(Key.Delete, PhysicalKey.Delete, "Delete")]
    [InlineData(Key.Home, PhysicalKey.Home, "Home")]
    [InlineData(Key.A, PhysicalKey.A, "A")]
    [InlineData(Key.F1, PhysicalKey.F1, "F1")]
    public void GetShortcutKeyNameDifferentiatesNumpad(Key key, PhysicalKey physicalKey, string expected)
    {
        var result = ShortcutManager.GetShortcutKeyName(key, physicalKey);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetShortcutKeyNameNumPadDeleteIsNotMainDelete()
    {
        // Same Avalonia Key value (Delete) but different physical keys must
        // produce different tokens so they can be bound independently.
        var numpad = ShortcutManager.GetShortcutKeyName(Key.Delete, PhysicalKey.NumPadDecimal);
        var main = ShortcutManager.GetShortcutKeyName(Key.Delete, PhysicalKey.Delete);

        Assert.NotEqual(numpad, main);
    }

    // Bug #11082: on a non-US layout (e.g. Swedish) Avalonia's Key enum is
    // derived from the produced character mapped against a US-keyboard table,
    // so Shift+. ('=>:') reports Key.OemSemicolon and Shift+, ('=>;') also
    // reports Key.OemSemicolon. Falling back to PhysicalKey for any Oem* key
    // gives each physical key a unique, layout-independent token.
    [Theory]
    [InlineData(Key.OemPeriod, PhysicalKey.Period, "Period")]
    [InlineData(Key.OemComma, PhysicalKey.Comma, "Comma")]
    [InlineData(Key.OemSemicolon, PhysicalKey.Period, "Period")]      // Swedish Shift+.
    [InlineData(Key.OemSemicolon, PhysicalKey.Comma, "Comma")]        // Swedish Shift+,
    [InlineData(Key.OemComma, PhysicalKey.IntlBackslash, "IntlBackslash")] // Swedish '<' next to Z
    [InlineData(Key.OemMinus, PhysicalKey.Minus, "Minus")]
    [InlineData(Key.OemPlus, PhysicalKey.Equal, "Equal")]
    [InlineData(Key.OemQuestion, PhysicalKey.Slash, "Slash")]
    [InlineData(Key.OemTilde, PhysicalKey.Backquote, "Backquote")]
    public void GetShortcutKeyNameUsesPhysicalKeyForOemKeys(Key key, PhysicalKey physicalKey, string expected)
    {
        var result = ShortcutManager.GetShortcutKeyName(key, physicalKey);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetShortcutKeyNameSwedishShiftPeriodNotEqualSwedishShiftComma()
    {
        // The collision behind #11082: both produce Key.OemSemicolon on Swedish,
        // so before the fix they shared a token and second-bound shortcut won.
        var shiftPeriod = ShortcutManager.GetShortcutKeyName(Key.OemSemicolon, PhysicalKey.Period);
        var shiftComma = ShortcutManager.GetShortcutKeyName(Key.OemSemicolon, PhysicalKey.Comma);

        Assert.NotEqual(shiftPeriod, shiftComma);
    }

    [Fact]
    public void MigrateLegacyOemKeysRewritesKnownTokensInPlace()
    {
        var keys = new List<string> { "Shift", "OemPeriod", "OemComma", "Oem1", "A" };

        ShortcutManager.MigrateLegacyOemKeys(keys);

        Assert.Equal(new[] { "Shift", "Period", "Comma", "Semicolon", "A" }, keys);
    }

    [Fact]
    public void MigrateLegacyOemKeysLeavesUnknownTokensUnchanged()
    {
        var keys = new List<string> { "Control", "F5", "NumPad7", "Period" };

        ShortcutManager.MigrateLegacyOemKeys(keys);

        Assert.Equal(new[] { "Control", "F5", "NumPad7", "Period" }, keys);
    }

    [Fact]
    public void AltGrTypingDoesNotCompleteShortcuts()
    {
        var manager = new ShortcutManager();
        var category = ShortcutCategory.SubtitleGridAndTextBox;
        var command = new RelayCommand(() => { });
        manager.RegisterShortcut(new ShortCut("Italic", ["Control", "I"], category, command));
        manager.RegisterShortcut(new ShortCut("AltGr E", ["Control", "Alt", "E"], category, command));

        var i = KeyEvent(Key.I, PhysicalKey.I, KeyModifiers.None);
        manager.OnKeyPressed(null, i);

        var syntheticControl = KeyEvent(Key.LeftCtrl, PhysicalKey.ControlLeft, KeyModifiers.Control);
        manager.OnKeyPressed(null, syntheticControl);
        Assert.Null(manager.CheckShortcuts(syntheticControl, category.ToString()));

        var altGr = KeyEvent(Key.RightAlt, PhysicalKey.AltRight, KeyModifiers.Control | KeyModifiers.Alt);
        manager.OnKeyPressed(null, altGr);
        Assert.Null(manager.CheckShortcuts(altGr, category.ToString()));

        manager.OnKeyReleased(null, i);
        var e = KeyEvent(Key.E, PhysicalKey.E, KeyModifiers.Control | KeyModifiers.Alt);
        manager.OnKeyPressed(null, e);
        Assert.Null(manager.CheckShortcuts(e, category.ToString()));

        manager.ClearKeys();
        manager.OnKeyPressed(null, e);
        Assert.Same(command, manager.CheckShortcuts(e, category.ToString()));
    }
}
