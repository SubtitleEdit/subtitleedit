using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Xunit;

namespace UITests.Features.Main;

/// <summary>
/// #15462: in a narrow window the right toolbar panel (format/encoding/frame rate) was drawn on
/// top of the toolbar icons. It now hides its items by rank - labels first - instead.
/// </summary>
public class ToolbarOverlapTests
{
    private const double IconsWidth = 500;

    private sealed class Toolbar
    {
        public required ToolbarPanel Panel { get; init; }
        public required Border LabelFormat { get; init; }
        public required Border ComboFormat { get; init; }
        public required Border LabelFrameRate { get; init; }
        public required Border ComboFrameRate { get; init; }
    }

    // Right panel: label 50 + format 200 + label 70 + frame rate 110 = 430 + 3 spacings = 433.
    private static Toolbar MakeToolbar()
    {
        var left = new StackPanel { Orientation = Orientation.Horizontal };
        left.Children.Add(new Border { Width = IconsWidth, Height = 30 });

        var right = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 1,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        var labelFormat = new Border { Width = 50, Height = 20 };
        var comboFormat = new Border { Width = 200, Height = 20 };
        var labelFrameRate = new Border { Width = 70, Height = 20 };
        var comboFrameRate = new Border { Width = 110, Height = 20 };
        right.Children.Add(labelFormat);
        right.Children.Add(comboFormat);
        right.Children.Add(labelFrameRate);
        right.Children.Add(comboFrameRate);

        var panel = new ToolbarPanel(left, right);
        panel.AddCollapsible(labelFormat, 1);
        panel.AddCollapsible(labelFrameRate, 1);
        panel.AddCollapsible(comboFrameRate, 2);
        panel.AddCollapsible(comboFormat, 3);

        return new Toolbar
        {
            Panel = panel,
            LabelFormat = labelFormat,
            ComboFormat = comboFormat,
            LabelFrameRate = labelFrameRate,
            ComboFrameRate = comboFrameRate,
        };
    }

    private static void Layout(ToolbarPanel panel, double width)
    {
        panel.Measure(new Size(width, 40));
        panel.Arrange(new Rect(0, 0, width, 40));
    }

    [AvaloniaFact]
    public void WideWindow_ShowsEverything()
    {
        var toolbar = MakeToolbar();

        Layout(toolbar.Panel, IconsWidth + ToolbarPanel.MinimumGap + 433);

        Assert.True(toolbar.LabelFormat.IsVisible);
        Assert.True(toolbar.LabelFrameRate.IsVisible);
        Assert.True(toolbar.ComboFrameRate.IsVisible);
        Assert.True(toolbar.ComboFormat.IsVisible);
    }

    [AvaloniaFact]
    public void SlightlyTooNarrow_HidesLabelsFirst()
    {
        var toolbar = MakeToolbar();

        Layout(toolbar.Panel, IconsWidth + ToolbarPanel.MinimumGap + 432);

        Assert.False(toolbar.LabelFormat.IsVisible);
        Assert.False(toolbar.LabelFrameRate.IsVisible);
        Assert.True(toolbar.ComboFrameRate.IsVisible);
        Assert.True(toolbar.ComboFormat.IsVisible);
        Assert.True(toolbar.ComboFormat.Bounds.Width > 0);
    }

    [AvaloniaFact]
    public void MuchTooNarrow_KeepsFormatComboLongest()
    {
        var toolbar = MakeToolbar();

        // Only the format combo (200) fits.
        Layout(toolbar.Panel, IconsWidth + ToolbarPanel.MinimumGap + 250);

        Assert.False(toolbar.LabelFormat.IsVisible);
        Assert.False(toolbar.LabelFrameRate.IsVisible);
        Assert.False(toolbar.ComboFrameRate.IsVisible);
        Assert.True(toolbar.ComboFormat.IsVisible);
    }

    [AvaloniaFact]
    public void TallAvailableHeight_IconsKeepTheirNaturalSize()
    {
        // The toolbar icons are Stretch.Uniform images; offered the window's height they would
        // scale up and turn the whole window into toolbar. A Viewbox scales the same way.
        var left = new StackPanel { Orientation = Orientation.Horizontal };
        var icon = new Viewbox { Child = new Border { Width = 32, Height = 32 } };
        left.Children.Add(icon);
        var panel = new ToolbarPanel(left, new StackPanel { Orientation = Orientation.Horizontal });

        panel.Measure(new Size(1000, 800));
        panel.Arrange(new Rect(0, 0, 1000, 800));

        Assert.Equal(32, panel.DesiredSize.Height);
        Assert.Equal(32, icon.Bounds.Height);
    }

    [AvaloniaFact]
    public void WideningAgain_RestoresHiddenItems()
    {
        var toolbar = MakeToolbar();
        Layout(toolbar.Panel, IconsWidth + 100);
        Assert.False(toolbar.ComboFormat.IsVisible);

        Layout(toolbar.Panel, IconsWidth + ToolbarPanel.MinimumGap + 433);

        Assert.True(toolbar.LabelFormat.IsVisible);
        Assert.True(toolbar.LabelFrameRate.IsVisible);
        Assert.True(toolbar.ComboFrameRate.IsVisible);
        Assert.True(toolbar.ComboFormat.IsVisible);
    }
}
