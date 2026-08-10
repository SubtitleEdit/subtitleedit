using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Interaction handler for the main menu bar that makes the <see cref="Menu"/> raise Opened
/// when a top-level drop-down is opened with an access key (Alt+F etc.).
///
/// Avalonia's <see cref="DefaultMenuInteractionHandler.AccessKeyPressed"/> opens the item's
/// drop-down popup directly without calling Menu.Open() - unlike both the pointer path
/// (PointerPressed calls IMainMenu.Open() first) and the bare-Alt path (AccessKeyHandler
/// calls MainMenu.Open()). MenuBase.OnSubmenuOpened then flips IsOpen to true as a silent
/// side effect, so the menu ends up open with the Opened event never raised (and Closed
/// firing unpaired later). Everything keyed to Opened silently skips; most visibly, the
/// undocked tool windows kept their topmost and covered the drop-down and its cascaded
/// submenus (#13361 - still unfixed in upstream Avalonia master as of 12.1.1).
///
/// DefaultMenuInteractionHandler is marked [Unstable]; the access-key regression test in
/// MainMenuKeyboardActivationTests pins this behavior across Avalonia upgrades.
/// </summary>
public sealed class MainMenuInteractionHandler : DefaultMenuInteractionHandler
{
    public MainMenuInteractionHandler()
        : base(isContextMenu: false)
    {
    }

    protected override void AccessKeyPressed(object? sender, RoutedEventArgs e)
    {
        // Mirror the pointer path: open the Menu before the item's drop-down, so Opened fires
        // and the menu's open state is consistent no matter how the user got in. For an
        // ambiguous access key (two top-level items sharing a letter - none in SE's menu) the
        // base handler only moves focus; opening the bar then still matches bare-Alt semantics
        // (IsOpen with no drop-down). The ambiguity flag itself (AccessKeyEventArgs.IsMultiple)
        // is internal to Avalonia, so it cannot be consulted here.
        if (FindMenuItem(e.Source) is { IsTopLevel: true, HasSubMenu: true, IsEffectivelyEnabled: true } item &&
            item.Parent is MenuBase { IsOpen: false } menu)
        {
            menu.Open();
        }

        base.AccessKeyPressed(sender, e);
    }

    private static MenuItem? FindMenuItem(object? source)
    {
        var current = source as ILogical;
        while (current != null)
        {
            if (current is MenuItem item)
            {
                return item;
            }

            current = current.LogicalParent;
        }

        return null;
    }
}
