using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls;

namespace UITests.Controls;

/// <summary>
/// The spell checker's "Use once / Use always" buttons sat in a horizontal StackPanel in a
/// narrow fixed-width column, so Russian/Bulgarian/Ukrainian labels ran past the window edge
/// (#15901). ButtonPairPanel keeps them side by side when they fit and stacks them otherwise.
/// </summary>
public class ButtonPairPanelTests
{
    private static (ButtonPairPanel Panel, Button First, Button Second) Layout(string first, string second, double width)
    {
        var buttonFirst = new Button { Content = first, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        var buttonSecond = new Button { Content = second, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        var panel = new ButtonPairPanel
        {
            Children = { buttonFirst, buttonSecond },
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };

        // Hosted in a window so the buttons get their theme template (and real padding).
        var window = new Window { Width = width, Height = 300, Content = panel };
        window.Show();
        window.UpdateLayout();
        window.Close();
        return (panel, buttonFirst, buttonSecond);
    }

    [AvaloniaFact]
    public void ShortLabels_SideBySide_EqualWidth()
    {
        var (_, first, second) = Layout("Use once", "Use always", 240);

        Assert.Equal(first.Bounds.Y, second.Bounds.Y);
        Assert.True(second.Bounds.X > first.Bounds.Right);
        Assert.Equal(first.Bounds.Width, second.Bounds.Width, 3);
        Assert.True(second.Bounds.Right <= 240);
    }

    [AvaloniaFact]
    public void LongLabels_Stacked_FullWidth_InsideBounds()
    {
        var (panel, first, second) = Layout("Використовувати один раз", "Використовувати завжди", 240);

        Assert.True(second.Bounds.Y >= first.Bounds.Bottom);
        Assert.Equal(240, first.Bounds.Width, 3);
        Assert.Equal(240, second.Bounds.Width, 3);
        Assert.True(panel.DesiredSize.Width <= 240);
    }
}
