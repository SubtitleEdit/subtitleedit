using Avalonia.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Matches a key event against the user's main-window shortcut bindings, so a dialog can offer the
/// same keys as the main window without going through the full ShortcutManager.
/// </summary>
public static partial class MainShortcutKeys
{
    private const KeyModifiers ControlAlt = KeyModifiers.Control | KeyModifiers.Alt;
    private const int VkRightMenu = 0xA5; // physical right Alt

    [LibraryImport("user32.dll")]
    private static partial short GetKeyState(int keyCode);

    /// <summary>
    /// True while the physical right Alt (AltGr) is down. Windows reports AltGr as Ctrl+Alt, so
    /// like ShortcutManager this tells the two apart by the physical key; AltGr is Windows-only.
    /// Replaceable for tests.
    /// </summary>
    internal static Func<bool> IsAltGrHeld { get; set; } =
        () => OperatingSystem.IsWindows() && GetKeyState(VkRightMenu) < 0;

    /// <summary>The Ctrl/Cmd token as it is stored in the settings shortcut key lists.</summary>
    public static string CtrlOrCmd => OperatingSystem.IsMacOS() ? "Win" : "Ctrl";

    /// <summary>
    /// True when the pressed keys match the main-window binding of <paramref name="actionName"/>
    /// (a MainViewModel command name), falling back to the built-in default keys when the user
    /// has no binding stored for it.
    /// </summary>
    public static bool Matches(KeyEventArgs e, string actionName, IReadOnlyList<string> defaultKeys)
    {
        var keys = Se.Settings.Shortcuts.FirstOrDefault(s => s.ActionName == actionName)?.Keys;
        return MatchesKeys(e, keys ?? defaultKeys);
    }

    /// <summary>
    /// Matches a stored shortcut key list (modifier tokens + one main key) against a key event.
    /// Multi-key non-modifier chords are not supported here - the full ShortcutManager handles
    /// those in the main window; a dialog only needs the simple form.
    /// </summary>
    public static bool MatchesKeys(KeyEventArgs e, IReadOnlyList<string> keys)
    {
        var modifiers = KeyModifiers.None;
        var wantsAltGr = false;
        Key? mainKey = null;
        foreach (var token in keys)
        {
            if (token is "Ctrl" or "Control" or "LeftCtrl" or "RightCtrl")
            {
                modifiers |= KeyModifiers.Control;
            }
            else if (token is "Alt" or "LeftAlt" or "RightAlt")
            {
                modifiers |= KeyModifiers.Alt;
            }
            else if (token == ShortcutManager.AltGrToken)
            {
                wantsAltGr = true;
            }
            else if (token is "Shift" or "LeftShift" or "RightShift")
            {
                modifiers |= KeyModifiers.Shift;
            }
            else if (token is "Win" or "Meta" or "LWin" or "RWin" or "Cmd" or "Command")
            {
                modifiers |= KeyModifiers.Meta;
            }
            else if (Enum.TryParse<Key>(token, out var key) && mainKey == null)
            {
                mainKey = key;
            }
            else
            {
                return false;
            }
        }

        if (mainKey == null || mainKey != e.Key)
        {
            return false;
        }

        // Windows AltGr arrives as Ctrl+Alt: it matches only an AltGr binding, never a Ctrl+Alt
        // one, so the character AltGr types wins unless AltGr+key is bound (same as ShortcutManager).
        var isAltGr = (e.KeyModifiers & ControlAlt) == ControlAlt && IsAltGrHeld();
        if (wantsAltGr != isAltGr)
        {
            return false;
        }

        if (wantsAltGr)
        {
            modifiers |= ControlAlt;
        }

        return e.KeyModifiers == modifiers;
    }
}
