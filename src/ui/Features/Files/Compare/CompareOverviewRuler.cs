using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

/// <summary>
/// A thin strip beside the merge view with a tick for every difference in the whole file and a
/// box for the part that is on screen - so the shape of the comparison shows at a glance, and a
/// click jumps there.
/// </summary>
public class CompareOverviewRuler : Control
{
    private static readonly IBrush ChangedBrush = new ImmutableSolidColorBrush(Color.FromRgb(70, 185, 105));
    private static readonly IBrush NumberBrush = new ImmutableSolidColorBrush(Color.FromRgb(215, 170, 45));
    private static readonly IBrush OnlyInOneBrush = new ImmutableSolidColorBrush(Color.FromRgb(225, 85, 85));

    private IReadOnlyList<CompareRow> _rows = Array.Empty<CompareRow>();
    private double _viewportTop;
    private double _viewportHeight;
    private bool _dragging;

    /// <summary>Raised with the fraction (0-1) of the list to centre on.</summary>
    public event EventHandler<double>? ScrollRequested;

    public CompareOverviewRuler()
    {
        Width = 14;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public void SetRows(IReadOnlyList<CompareRow> rows)
    {
        _rows = rows;
        InvalidateVisual();
    }

    /// <summary>The visible part of the list, as fractions of its full height.</summary>
    public void SetViewport(double top, double height)
    {
        _viewportTop = Math.Clamp(top, 0, 1);
        _viewportHeight = Math.Clamp(height, 0, 1);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        context.DrawRectangle(UiUtil.GetTextColor(0.05), null, bounds, 4, 4);

        var count = _rows.Count;
        if (count > 0)
        {
            var tick = Math.Max(2, bounds.Height / count);
            for (var i = 0; i < count; i++)
            {
                var row = _rows[i];
                var y = bounds.Height * i / count;
                var brush = GetBrush(row.Kind);
                if (brush != null)
                {
                    context.FillRectangle(brush, new Rect(3, y, bounds.Width - 6, tick));
                }

                if (row.IsEdited)
                {
                    context.FillRectangle(CompareColors.Edited, new Rect(0, y, 2, tick));
                }
            }
        }

        if (_viewportHeight is > 0 and < 1)
        {
            var viewport = new Rect(0.5, bounds.Height * _viewportTop, bounds.Width - 1, Math.Max(8, bounds.Height * _viewportHeight));
            context.DrawRectangle(UiUtil.GetTextColor(0.08), new Pen(UiUtil.GetTextColor(0.45), 1), viewport, 3, 3);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _dragging = true;
        e.Pointer.Capture(this);
        RequestScroll(e.GetPosition(this).Y);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging)
        {
            RequestScroll(e.GetPosition(this).Y);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void RequestScroll(double y)
    {
        if (Bounds.Height > 0)
        {
            ScrollRequested?.Invoke(this, Math.Clamp(y / Bounds.Height, 0, 1));
        }
    }

    private static IBrush? GetBrush(CompareRowKind kind) => kind switch
    {
        CompareRowKind.Changed => ChangedBrush,
        CompareRowKind.NumberOnly => NumberBrush,
        CompareRowKind.OnlyLeft or CompareRowKind.OnlyRight => OnlyInOneBrush,
        _ => null,
    };
}
