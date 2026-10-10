using System;
using Avalonia;
using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// Lays out its children side by side in equal-width columns when all of them fit on one row at
/// their natural width, and otherwise stacks them full width - so a long translation (e.g.
/// Ukrainian "Використовувати один раз") gets its own row instead of running past the window edge
/// or breaking mid-word (#15901).
/// </summary>
public class ButtonPairPanel : Panel
{
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<ButtonPairPanel, double>(nameof(Spacing), 5);

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    private bool _stacked;

    protected override Size MeasureOverride(Size availableSize)
    {
        var count = 0;
        var maxWidth = 0.0;
        var maxHeight = 0.0;
        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            child.Measure(Size.Infinity);
            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            count++;
        }

        if (count == 0)
        {
            return default;
        }

        // Equal columns, so each one must fit the widest child.
        var rowWidth = maxWidth * count + Spacing * (count - 1);
        _stacked = count > 1 && !double.IsInfinity(availableSize.Width) && rowWidth > availableSize.Width;
        if (_stacked)
        {
            return new Size(Math.Min(maxWidth, availableSize.Width), maxHeight * count + Spacing * (count - 1));
        }

        return new Size(rowWidth, maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visible = 0;
        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                visible++;
            }
        }

        if (visible == 0)
        {
            return finalSize;
        }

        var index = 0;
        if (_stacked)
        {
            var height = (finalSize.Height - Spacing * (visible - 1)) / visible;
            foreach (var child in Children)
            {
                if (child.IsVisible)
                {
                    child.Arrange(new Rect(0, index * (height + Spacing), finalSize.Width, height));
                    index++;
                }
            }
        }
        else
        {
            var width = (finalSize.Width - Spacing * (visible - 1)) / visible;
            foreach (var child in Children)
            {
                if (child.IsVisible)
                {
                    child.Arrange(new Rect(index * (width + Spacing), 0, width, finalSize.Height));
                    index++;
                }
            }
        }

        return finalSize;
    }
}
