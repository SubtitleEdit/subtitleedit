using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Avalonia 12.1.4 no longer resizes a pointer-placed popup after its first layout pass, so
/// context menus kept a too small first size (#15965). While open, the popup is placed with
/// anchor and gravity at the pointer instead, and its own placement comes back on close.
/// </summary>
public class PointerPopupPlacementTests
{
    [AvaloniaFact]
    public void ContextFlyout_OpenedAtPointer_IsAnchoredAtPointerWhileOpen()
    {
        PointerPopupPlacement.Initialize();

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = "Delete" });
        var host = new Border { Width = 300, Height = 200, Background = Brushes.Gray, ContextFlyout = flyout };
        var window = new Window { Width = 400, Height = 300, Content = host };
        window.Show();
        window.UpdateLayout();

        try
        {
            window.MouseMove(new Point(50, 60));
            flyout.ShowAt(host, showAtPointer: true);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            var popup = ((MenuItem)flyout.Items[0]!).FindLogicalAncestorOfType<Popup>();
            Assert.NotNull(popup);
            Assert.Equal(PlacementMode.AnchorAndGravity, popup!.Placement);
            Assert.Equal(PopupAnchor.TopLeft, popup.PlacementAnchor);
            Assert.Equal(PopupGravity.BottomRight, popup.PlacementGravity);
            Assert.Equal(new Rect(window.TranslatePoint(new Point(50, 60), host)!.Value, new Size(1, 1)), popup.PlacementRect);

            flyout.Hide();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(PlacementMode.Pointer, popup.Placement);
            Assert.Null(popup.PlacementRect);
        }
        finally
        {
            window.Close();
        }
    }
}
