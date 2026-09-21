using Avalonia;
using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// An invisible WrapPanel child that forces a line break: it asks for the whole available width
/// at zero height, so it never fits beside the items before it and leaves no room for the items
/// after it - those start a new row.
/// </summary>
public class WrapPanelLineBreak : Control
{
    public WrapPanelLineBreak()
    {
        IsHitTestVisible = false;
        Focusable = false;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, 0);
    }
}
