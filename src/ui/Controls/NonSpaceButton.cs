using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="Button"/> that never reacts to the Space key.
/// <para>
/// Avalonia's built-in <see cref="Button"/> both marks Space as handled on key-down and raises
/// <see cref="Button.Click"/> on Space key-up whenever it is focused (it does not require that the
/// same button also saw the key-down). Once such a button gains focus - e.g. after a mouse click -
/// it swallows and/or duplicates the global "toggle play/pause" Space shortcut, so Space stops
/// behaving as a toggle (issue #12759, same root cause as #12093).
/// </para>
/// <para>
/// Using this button for player/waveform transport buttons keeps Space owned exclusively by the
/// global shortcut, no matter which of those buttons currently has keyboard focus.
/// </para>
/// </summary>
public class NonSpaceButton : Button
{
    // Keep the default Button styling instead of looking for a NonSpaceButton style.
    protected override Type StyleKeyOverride => typeof(Button);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
            return;
        }

        base.OnKeyUp(e);
    }
}
