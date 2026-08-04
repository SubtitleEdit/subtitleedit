using Avalonia.Controls;
using Avalonia.Input;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Cancels the menu-bar activation that Avalonia arms on Alt down when Alt turned out to be a mouse
/// modifier instead - e.g. Alt+drag in the waveform to adjust a time code (discussion #11744).
///
/// Avalonia's AccessKeyHandler arms the activation on Alt down and only disarms it when another
/// <em>key</em> is pressed; its pointer handler clears the access-key underlines but not the internal
/// "showing access keys" flag, so releasing Alt after an Alt+click still runs MainMenu.Open(). The
/// menu bar then takes keyboard focus and the window key handler hands every following key to the
/// menu - which the user sees as "all shortcuts stopped working until I click something".
///
/// Windows itself cancels menu activation when Alt is combined with a mouse click; this restores that
/// behavior. The guard is pure state: the owner wires it to a tunnelling PointerPressed handler and to
/// its key-up handler, and undoes the activation itself when <see cref="TryConsumeAltRelease"/> says so.
/// </summary>
public sealed class AltMenuActivationGuard
{
    private bool _isArmed;
    private Control? _focusAfterPointerPress;

    /// <summary>
    /// Call from a tunnelling PointerPressed handler on the window content.
    /// </summary>
    /// <param name="isAltDown">True when Alt was held during the press.</param>
    /// <param name="isMenuPress">
    /// True when the press belongs to the menu (the menu bar is already open, or the press landed on
    /// it) - such a press should open the menu as usual, so no activation is cancelled.
    /// </param>
    public void NotifyPointerPressed(bool isAltDown, bool isMenuPress)
    {
        if (!isAltDown || isMenuPress)
        {
            return;
        }

        _isArmed = true;
        _focusAfterPointerPress = null;
    }

    /// <summary>
    /// Records the control that ended up with keyboard focus because of the press, so focus can be
    /// returned there when the menu activation is undone. Read after the press has been handled (the
    /// clicked control takes focus during the press, not before it).
    /// </summary>
    public void NotifyFocusAfterPointerPress(Control? focused)
    {
        if (_isArmed)
        {
            _focusAfterPointerPress = focused;
        }
    }

    /// <summary>
    /// True when <paramref name="key"/> ends an Alt+pointer gesture, meaning the caller should undo any
    /// menu activation Avalonia just performed. <paramref name="focusToRestore"/> is the control that
    /// had focus after the press, or null when it is unknown.
    /// </summary>
    public bool TryConsumeAltRelease(Key key, out Control? focusToRestore)
    {
        focusToRestore = null;

        if (!_isArmed || key is not (Key.LeftAlt or Key.RightAlt))
        {
            return false;
        }

        focusToRestore = _focusAfterPointerPress;
        Reset();
        return true;
    }

    /// <summary>
    /// Drops any armed state, e.g. when the window is deactivated by a task switch and the Alt release
    /// never arrives.
    /// </summary>
    public void Reset()
    {
        _isArmed = false;
        _focusAfterPointerPress = null;
    }
}
