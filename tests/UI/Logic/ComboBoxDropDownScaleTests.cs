using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// ComboBox dropdown popups render outside the LayoutTransformControl that scales window
/// content, so the UI scale setting never reached them (#13010). An app-level ComboBoxItem
/// style now bakes in the scaled font size. At 100% no style is added at all, so windows
/// that locally restyle their combos (e.g. the burn-in window's compact 12.5) keep their look.
/// </summary>
public class ComboBoxDropDownScaleTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private ComboBoxItem FirstDropDownItem(double scaleFactor)
    {
        UiTheme.SetLayoutScale(scaleFactor);

        var comboBox = new ComboBox { ItemsSource = new[] { "One", "Two" } };
        var window = new Window { Content = comboBox, Width = 300, Height = 200 };
        _windows.Add(window);
        window.Show();
        window.UpdateLayout();

        comboBox.IsDropDownOpen = true;
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();

        var item = (ComboBoxItem?)comboBox.ContainerFromIndex(0);
        Assert.NotNull(item);
        return item;
    }

    [AvaloniaTheory]
    [InlineData(0.9)]
    [InlineData(1.5)]
    public void DropDownItems_FollowLayoutScale(double factor)
    {
        try
        {
            var item = FirstDropDownItem(factor);
            Assert.Equal(14.0 * factor, item.FontSize, precision: 3);
        }
        finally
        {
            UiTheme.SetLayoutScale(1.0);
        }
    }

    [AvaloniaFact]
    public void DropDownItems_At100Percent_KeepInheritedFontSize()
    {
        try
        {
            var item = FirstDropDownItem(1.0);
            var comboBox = (ComboBox)item.Parent!;

            // No style override at 100%: items still inherit from the combo, so a window
            // that locally shrinks its combos keeps matching dropdowns.
            comboBox.FontSize = 12.5;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(12.5, item.FontSize, precision: 3);
        }
        finally
        {
            UiTheme.SetLayoutScale(1.0);
        }
    }
}
