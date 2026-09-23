using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A <see cref="SplitButton"/> whose primary part clicks on Enter key-<em>down</em>.
/// <para>
/// Avalonia's <see cref="SplitButton"/> clicks its primary part on Enter key-<em>up</em> and
/// leaves the key-down unhandled. Every dialog here has an <see cref="Button.IsDefault"/> OK or
/// Done button, which listens for unhandled Enter key-downs on the window - so Enter on a focused
/// "Remux"/"Generate"/"Convert" split button ran Done first and closed the window before the
/// split button ever saw its key-up (#15197). Space was unaffected, which is why it looked like
/// Enter alone was broken.
/// </para>
/// <para>
/// Handling Enter on key-down keeps it away from the default button; the matching key-up is
/// swallowed so the base class does not click a second time. Alt+Down still opens the flyout.
/// </para>
/// </summary>
public class SeSplitButton : SplitButton
{
    // Keep the default SplitButton styling instead of looking for a SeSplitButton style.
    protected override Type StyleKeyOverride => typeof(SplitButton);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.Handled)
        {
            e.Handled = true;
            if (IsEffectivelyEnabled)
            {
                OnClickPrimary(null);
            }

            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true; // already clicked on key-down
            return;
        }

        base.OnKeyUp(e);
    }
}
