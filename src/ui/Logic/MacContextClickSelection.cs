using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Makes a macOS Ctrl+click on a selected row of a ListBox/TableView keep the selection, like a
/// right-click does and like Finder - the context menu then acts on all the selected rows.
///
/// Cmd is the toggle modifier on macOS, so Avalonia treats a Ctrl+left press as a plain click that
/// collapses the selection to the clicked row. Only a real right press gets the "keep the selection
/// when the row is already selected" path. Ctrl+click on an unselected row needs nothing here: the
/// plain click path selects just that row, same as a right-click.
/// </summary>
public static class MacContextClickSelection
{
    private static bool _isRegistered;

    /// <summary>
    /// Registers the handler for all list rows, once at application startup. No-op on non-macOS
    /// platforms or if already registered.
    /// </summary>
    public static void Register()
    {
        if (!OperatingSystem.IsMacOS() || _isRegistered)
        {
            return;
        }

        _isRegistered = true;

        // Tunnel phase, so the press is stopped before ListBoxItem selects from it.
        InputElement.PointerPressedEvent.AddClassHandler<ListBoxItem>(OnRowPointerPressed, RoutingStrategies.Tunnel);
    }

    internal static void OnRowPointerPressed(ListBoxItem row, PointerPressedEventArgs e)
    {
        if (row.IsSelected &&
            e.GetCurrentPoint(row).Properties.IsLeftButtonPressed &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;

            // Handling the press here also stops FocusManager, which focuses on the event source
            // further down the tunnel. Not when focus is already in the row (inline cell editor).
            if (!row.IsKeyboardFocusWithin)
            {
                row.Focus(NavigationMethod.Pointer, e.KeyModifiers);
            }
        }
    }
}
