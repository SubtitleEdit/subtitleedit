using Avalonia.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Matches a key event against the user's main-window shortcut bindings, so a dialog can offer the
/// same keys as the main window without going through the full ShortcutManager.
/// </summary>
public static class MainShortcutKeys
{
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

        return mainKey != null && mainKey == e.Key && e.KeyModifiers == modifiers;
    }
}
