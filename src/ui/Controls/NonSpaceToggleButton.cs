using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="ToggleButton"/> that never reacts to the Space key (see <see cref="NonSpaceButton"/>)
/// and does not flip <see cref="ToggleButton.IsChecked"/> itself when clicked: the owner sets it
/// from the state the click's command changed, so a command that does nothing (e.g. a player
/// without subtitle support) leaves the button as it was.
/// </summary>
public class NonSpaceToggleButton : ToggleButton
{
    // Keep the default ToggleButton styling instead of looking for a NonSpaceToggleButton style.
    protected override Type StyleKeyOverride => typeof(ToggleButton);

    protected override void Toggle()
    {
    }

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
