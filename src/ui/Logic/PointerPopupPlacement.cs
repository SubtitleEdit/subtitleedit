using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System.Runtime.CompilerServices;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Avalonia 12.1.4 (AvaloniaUI/Avalonia#22366) stopped updating a popup placed at the pointer
/// after its first layout pass, and that update is also what resizes the popup window. A context
/// menu still grows after its first layout pass the first time it is shown, so it kept a too
/// small size: the submenu arrows ended up outside the menu (#15965). While such a popup is open,
/// its pointer placement is swapped for an anchor-and-gravity placement at the same point, which
/// still resizes, and restored when it closes.
/// </summary>
public static class PointerPopupPlacement
{
    private sealed record SavedPlacement(PlacementMode Placement, Rect? PlacementRect, PopupAnchor Anchor, PopupGravity Gravity);

    private static readonly ConditionalWeakTable<Popup, SavedPlacement> Saved = new();
    private static PixelPoint? _lastPointerScreenPosition;
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        // TopLevel.LastPointerPosition, which pointer placement uses, is internal
        InputElement.PointerMovedEvent.AddClassHandler<TopLevel>(TrackPointer, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerPressedEvent.AddClassHandler<TopLevel>(TrackPointer, RoutingStrategies.Tunnel, handledEventsToo: true);

        // Runs after Popup's own handler, so the popup has been shown and positioned at the pointer
        Popup.IsOpenProperty.Changed.AddClassHandler<Popup>((popup, e) =>
        {
            if (e.NewValue is true)
            {
                ReplacePointerPlacement(popup);
            }
            else
            {
                RestorePlacement(popup);
            }
        });
    }

    private static void TrackPointer(TopLevel topLevel, PointerEventArgs e)
    {
        _lastPointerScreenPosition = topLevel.PointToScreen(e.GetPosition(topLevel));
    }

    private static void ReplacePointerPlacement(Popup popup)
    {
        if (popup.Placement != PlacementMode.Pointer ||
            _lastPointerScreenPosition is not { } screenPosition ||
            (popup.PlacementTarget ?? popup.FindLogicalAncestorOfType<Control>()) is not { } target ||
            TopLevel.GetTopLevel(target) == null)
        {
            return;
        }

        Saved.AddOrUpdate(popup, new SavedPlacement(popup.Placement, popup.PlacementRect, popup.PlacementAnchor, popup.PlacementGravity));

        // Same anchor and gravity as pointer placement uses
        popup.PlacementRect = new Rect(target.PointToClient(screenPosition), new Size(1, 1));
        popup.PlacementAnchor = PopupAnchor.TopLeft;
        popup.PlacementGravity = PopupGravity.BottomRight;
        popup.Placement = PlacementMode.AnchorAndGravity;
    }

    private static void RestorePlacement(Popup popup)
    {
        if (!Saved.TryGetValue(popup, out var saved))
        {
            return;
        }

        Saved.Remove(popup);
        popup.Placement = saved.Placement;
        popup.PlacementRect = saved.PlacementRect;
        popup.PlacementAnchor = saved.Anchor;
        popup.PlacementGravity = saved.Gravity;
    }
}
