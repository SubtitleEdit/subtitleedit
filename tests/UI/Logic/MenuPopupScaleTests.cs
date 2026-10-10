using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// Submenu and context menu items render in popups outside the window's LayoutTransformControl,
/// but are logical descendants of it. The "already scaled by the transform" reset style once
/// matched them too, so at startup (before any per-item sizes were set) popups ignored the UI
/// scale: at 70% UI / 150% font they came out at 21 px instead of 14.7 (PR #14818 comment).
/// Only the top-level menu bar items sit inside the transform.
/// </summary>
public class MenuPopupScaleTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
        Se.Settings.Appearance.FontScale = 1.0;
        UiTheme.SetLayoutScale(1.0);
    }

    [AvaloniaTheory]
    [InlineData(0.7, 1.5)]
    [InlineData(1.5, 1.0)]
    public void PopupMenuItems_FollowLayoutAndFontScale(double layoutScale, double fontScale)
    {
        Se.Settings.Appearance.FontScale = fontScale;
        UiTheme.SetLayoutScale(layoutScale);

        var subItem = new MenuItem { Header = "Open" };
        var topItem = new MenuItem { Header = "File" };
        topItem.Items.Add(subItem);
        var menu = new Menu();
        menu.Items.Add(topItem);

        var flyoutItem = new MenuItem { Header = "Delete" };
        var flyout = new MenuFlyout();
        flyout.Items.Add(flyoutItem);
        var host = new Border { Width = 200, Height = 100, Background = Brushes.Gray, ContextFlyout = flyout };

        var panel = new StackPanel();
        panel.Children.Add(menu);
        panel.Children.Add(host);

        // Same shape as the main window: content pre-wrapped in a LayoutTransformControl,
        // created after the menu style was applied (as at startup).
        var window = new Window { Width = 400, Height = 300, Content = new LayoutTransformControl { Child = panel } };
        _windows.Add(window);
        UiTheme.ApplyScaleToWindow(window);
        window.Show();
        window.UpdateLayout();

        topItem.Open();
        flyout.ShowAt(host);
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        // Top-level item: scaled by the transform, so only the font scale is baked in.
        Assert.Equal(14.0 * fontScale, topItem.FontSize, precision: 3);
        Assert.Equal(32.0, topItem.MinHeight, precision: 3);

        // Popup items: outside the transform, so both factors are baked in.
        Assert.Equal(14.0 * layoutScale * fontScale, subItem.FontSize, precision: 3);
        Assert.Equal(32.0 * layoutScale, subItem.MinHeight, precision: 3);
        Assert.Equal(14.0 * layoutScale * fontScale, flyoutItem.FontSize, precision: 3);
        Assert.Equal(32.0 * layoutScale, flyoutItem.MinHeight, precision: 3);

        topItem.Close();
        flyout.Hide();
    }

    /// <summary>
    /// The main menu carries its own denser style, which beats the application-wide one for
    /// its submenu items - so it must bake the UI scale in itself, both at startup and after
    /// a scale change (second PR #14818 comment: dropdowns oversized until scale reapplied).
    /// </summary>
    [AvaloniaTheory]
    [InlineData(0.7, 1.5)]
    [InlineData(1.5, 1.0)]
    public void DenseMenuSubItems_FollowLayoutScale_AtStartupAndAfterChange(double layoutScale, double fontScale)
    {
        Se.Settings.Appearance.FontScale = fontScale;
        UiTheme.SetLayoutScale(layoutScale);

        var subItem = new MenuItem { Header = "Open" };
        var topItem = new MenuItem { Header = "File" };
        topItem.Items.Add(subItem);
        var menu = new Menu();
        menu.Items.Add(topItem);
        UiTheme.ApplyDenseMenuStyle(menu, 13.0);

        var window = new Window { Width = 400, Height = 300, Content = new LayoutTransformControl { Child = menu } };
        _windows.Add(window);
        UiTheme.ApplyScaleToWindow(window);
        window.Show();
        window.UpdateLayout();

        void AssertSizes(double scale)
        {
            topItem.Open();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(13.0 * fontScale, topItem.FontSize, precision: 3);
            Assert.Equal(23.0, topItem.MinHeight, precision: 3);
            Assert.Equal(13.0 * fontScale * scale, subItem.FontSize, precision: 3);
            Assert.Equal(23.0 * scale, subItem.MinHeight, precision: 3);

            topItem.Close();
        }

        AssertSizes(layoutScale);

        UiTheme.SetLayoutScale(1.2);
        AssertSizes(1.2);
    }

    /// <summary>
    /// Scale changes reach existing popup items through the re-created application style alone;
    /// nothing walks the windows to set per-item sizes any more.
    /// </summary>
    [AvaloniaFact]
    public void ExistingFlyoutItems_FollowLayoutScaleChange()
    {
        UiTheme.SetLayoutScale(1.0);

        var flyoutItem = new MenuItem { Header = "Delete" };
        var flyout = new MenuFlyout();
        flyout.Items.Add(flyoutItem);
        var host = new Border { Width = 200, Height = 100, Background = Brushes.Gray, ContextFlyout = flyout };
        var window = new Window { Width = 400, Height = 300, Content = new LayoutTransformControl { Child = host } };
        _windows.Add(window);
        UiTheme.ApplyScaleToWindow(window);
        window.Show();

        flyout.ShowAt(host);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(14.0, flyoutItem.FontSize, precision: 3);
        flyout.Hide();

        UiTheme.SetLayoutScale(1.3);
        flyout.ShowAt(host);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(14.0 * 1.3, flyoutItem.FontSize, precision: 3);
        Assert.Equal(32.0 * 1.3, flyoutItem.MinHeight, precision: 3);
        flyout.Hide();
    }
}
