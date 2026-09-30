using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// Lays out the main toolbar: the icons on the left and the format/encoding/frame rate panel
/// right-aligned on top of the same row. When the window gets too narrow for both, the right
/// panel's collapsible items are hidden - lowest rank first, all items of a rank together - so
/// they never draw on top of the icons (#15462).
/// </summary>
public class ToolbarPanel : Panel
{
    public const double MinimumGap = 8;

    private readonly Control _left;
    private readonly Panel _right;
    private readonly List<(Control Control, int Rank)> _collapsible = new();

    // Width of each collapsible item from the last time it was visible - a hidden control
    // measures as zero, but we need its real width to know when it fits again.
    private readonly Dictionary<Control, double> _widths = new();

    public ToolbarPanel(Control left, Panel right)
    {
        _left = left;
        _right = right;
        Children.Add(left);
        Children.Add(right);
    }

    /// <summary>
    /// Registers a direct child of the right panel that may be hidden when space runs out.
    /// Rank 1 hides first. Don't register a control whose IsVisible is bound elsewhere - wrap
    /// it in a container and register that instead.
    /// </summary>
    public void AddCollapsible(Control control, int rank)
    {
        _collapsible.Add((control, rank));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Unbounded height too (like the Auto grid row this replaces): the icons are
        // Stretch.Uniform images and would otherwise scale up to fill the offered height.
        var unbounded = new Size(double.PositiveInfinity, double.PositiveInfinity);
        _left.Measure(unbounded);
        _right.Measure(unbounded);

        foreach (var (control, _) in _collapsible)
        {
            if (control.IsVisible)
            {
                _widths[control] = control.DesiredSize.Width;
            }
        }

        var budget = availableSize.Width - _left.DesiredSize.Width - MinimumGap;
        var hideUpToRank = 0;
        foreach (var rank in _collapsible.Select(c => c.Rank).Distinct().OrderBy(r => r))
        {
            if (RightWidthWhenHiding(hideUpToRank) <= budget)
            {
                break;
            }

            hideUpToRank = rank;
        }

        var changed = false;
        foreach (var (control, rank) in _collapsible)
        {
            var visible = rank > hideUpToRank;
            if (control.IsVisible != visible)
            {
                control.IsVisible = visible;
                changed = true;
            }
        }

        if (changed)
        {
            _right.Measure(unbounded);
        }

        var desiredWidth = _left.DesiredSize.Width + MinimumGap + _right.DesiredSize.Width;
        if (!double.IsInfinity(availableSize.Width))
        {
            desiredWidth = Math.Min(desiredWidth, availableSize.Width);
        }

        return new Size(desiredWidth, Math.Max(_left.DesiredSize.Height, _right.DesiredSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Same as the single-cell grid this replaces: both panels get the whole row and
        // position themselves through their own alignment.
        // Never taller than measured, for the same Stretch.Uniform reason.
        var rect = new Rect(0, 0, finalSize.Width, Math.Min(finalSize.Height, DesiredSize.Height));
        _left.Arrange(rect);
        _right.Arrange(rect);
        return finalSize;
    }

    /// <summary>
    /// Width the (already measured) right panel would want with every collapsible item of rank
    /// up to <paramref name="hideUpToRank"/> hidden and the rest shown.
    /// </summary>
    private double RightWidthWhenHiding(int hideUpToRank)
    {
        var spacing = _right is StackPanel stackPanel ? stackPanel.Spacing : 0;
        var visibleNow = _right.Children.Where(c => c.IsVisible).ToList();
        var chrome = _right.DesiredSize.Width - visibleNow.Sum(c => c.DesiredSize.Width) -
                     spacing * Math.Max(0, visibleNow.Count - 1);

        var widths = new List<double>();
        foreach (var child in _right.Children)
        {
            var index = _collapsible.FindIndex(c => c.Control == child);
            if (index < 0)
            {
                if (child.IsVisible)
                {
                    widths.Add(child.DesiredSize.Width);
                }
            }
            else if (_collapsible[index].Rank > hideUpToRank)
            {
                widths.Add(_widths.TryGetValue(child, out var width) ? width : 0);
            }
        }

        return chrome + widths.Sum() + spacing * Math.Max(0, widths.Count - 1);
    }
}
