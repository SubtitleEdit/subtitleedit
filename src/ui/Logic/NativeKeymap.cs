using Avalonia;
using Avalonia.Input;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// App-wide additions to Avalonia's platform keymap.
/// </summary>
public static class NativeKeymap
{
    /// <summary>
    /// Registers Shift+Delete as a native cut gesture. Avalonia's keymap ships Ctrl+Insert copy
    /// and Shift+Insert paste on every platform, but not the equally classic Shift+Delete cut, so
    /// the gesture only worked in the two main-window edit boxes wired through the shortcut
    /// manager - every other text box silently deleted the selection without copying it (#13711).
    /// TextBox gates the keymap cut on CanCut, so read-only and password boxes stay untouched.
    /// Idempotent - safe to call again (e.g. from tests sharing one application).
    /// </summary>
    public static void AddShiftDeleteCut()
    {
        var hotkeys = Application.Current?.PlatformSettings?.HotkeyConfiguration;
        if (hotkeys == null)
        {
            return;
        }

        if (!hotkeys.Cut.Any(g => g.Key == Key.Delete && g.KeyModifiers == KeyModifiers.Shift))
        {
            hotkeys.Cut.Add(new KeyGesture(Key.Delete, KeyModifiers.Shift));
        }
    }
}
