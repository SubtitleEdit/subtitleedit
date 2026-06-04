using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class DataGridCheckboxMultiSelect<TItem> where TItem : class
{
    private readonly DataGrid _grid;
    private readonly Func<TItem, bool> _getChecked;
    private readonly Action<TItem, bool> _setChecked;
    private readonly Func<TItem, bool>? _canToggle;
    private readonly Action<TItem?>? _onFocusedItemChanged;
    private int _shiftAnchorIndex = -1;
    private int _shiftCurrentIndex = -1;
    private bool _selectionChangedSkip;

    // The caller does not need to store the returned instance.
    // Event subscriptions on dataGrid keep it alive for the lifetime of the grid.
    public DataGridCheckboxMultiSelect(
        DataGrid dataGrid,
        Func<TItem, bool> getChecked,
        Action<TItem, bool> setChecked,
        Func<TItem, bool>? canToggle = null,
        Action<TItem?>? onFocusedItemChanged = null)
    {
        _grid = dataGrid;
        _getChecked = getChecked;
        _setChecked = setChecked;
        _canToggle = canToggle;
        _onFocusedItemChanged = onFocusedItemChanged;

        dataGrid.SelectionMode = DataGridSelectionMode.Extended;

        dataGrid.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        dataGrid.PointerReleased += (_, _) => Dispatcher.UIThread.Post(() => dataGrid.Focus());

        dataGrid.SelectionChanged += (_, e) =>
        {
            if (_selectionChangedSkip)
            {
                return;
            }

            _shiftAnchorIndex = -1;
            _shiftCurrentIndex = -1;

            _onFocusedItemChanged?.Invoke(dataGrid.SelectedItem as TItem);
        };
    }

    private IList? GetItems() => _grid.ItemsSource as IList;

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (e.Key == Key.Space)
        {
            ToggleCheckboxForSelectedRows();
            e.Handled = true;
        }
        else if (isShift && e.Key is Key.Up or Key.Down or Key.PageUp or Key.PageDown or Key.Home or Key.End)
        {
            var items = GetItems();
            if (items == null)
            {
                return;
            }

            var direction = e.Key switch
            {
                Key.Up => -1,
                Key.Down => 1,
                Key.PageUp => -GetPageSize(),
                Key.PageDown => GetPageSize(),
                Key.Home => -items.Count,
                Key.End => items.Count,
                _ => 0
            };
            HandleShiftArrowSelection(direction);
            e.Handled = true;
        }
        else if (e.Key is Key.Home or Key.End)
        {
            var items = GetItems();
            if (items == null || items.Count == 0)
            {
                return;
            }

            _shiftAnchorIndex = -1;
            _shiftCurrentIndex = -1;
            var target = e.Key == Key.Home ? items[0] : items[^1];
            _grid.SelectedItem = target;
            _grid.ScrollIntoView(target, null);
            e.Handled = true;
        }
        else if (e.Key is Key.Up or Key.Down)
        {
            _shiftAnchorIndex = -1;
            _shiftCurrentIndex = -1;
        }
    }

    private void HandleShiftArrowSelection(int direction)
    {
        var items = GetItems();
        if (items == null || items.Count == 0)
        {
            return;
        }

        if (_shiftAnchorIndex < 0)
        {
            var anchor = _grid.SelectedItem != null
                ? items.IndexOf(_grid.SelectedItem)
                : -1;
            if (anchor < 0)
            {
                return;
            }

            _shiftAnchorIndex = anchor;
            _shiftCurrentIndex = anchor;
        }

        var newCurrent = Math.Clamp(_shiftCurrentIndex + direction, 0, items.Count - 1);
        if (newCurrent == _shiftCurrentIndex)
        {
            return;
        }

        _shiftCurrentIndex = newCurrent;

        var startIdx = Math.Min(_shiftAnchorIndex, _shiftCurrentIndex);
        var endIdx = Math.Max(_shiftAnchorIndex, _shiftCurrentIndex);

        _selectionChangedSkip = true;
        try
        {
            _grid.SelectedItems.Clear();
            for (var i = startIdx; i <= endIdx; i++)
            {
                _grid.SelectedItems.Add(items[i]);
            }
        }
        finally
        {
            _selectionChangedSkip = false;
        }

        _onFocusedItemChanged?.Invoke(items[_shiftCurrentIndex] as TItem);
        _grid.ScrollIntoView(items[_shiftCurrentIndex], null);
    }

    private void ToggleCheckboxForSelectedRows()
    {
        var selected = _grid.SelectedItems.OfType<TItem>().ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var toggleable = _canToggle != null ? selected.Where(_canToggle).ToList() : selected;
        if (toggleable.Count == 0)
        {
            return;
        }

        var allChecked = toggleable.All(_getChecked);
        var newValue = !allChecked;
        foreach (var item in toggleable)
        {
            _setChecked(item, newValue);
        }
    }

    private int GetPageSize()
    {
        var rowsPresenter = _grid.GetVisualDescendants()
            .OfType<DataGridRowsPresenter>()
            .FirstOrDefault();
        var rowHeight = _grid.RowHeight;
        if (rowsPresenter != null && rowsPresenter.Bounds.Height > 0 && !double.IsNaN(rowHeight) && rowHeight > 0)
        {
            return Math.Max(1, (int)Math.Ceiling(rowsPresenter.Bounds.Height / rowHeight) - 1);
        }

        var visibleRows = _grid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .Count(r => r.IsVisible && r.Bounds.Height > 0);
        return Math.Max(1, visibleRows - 1);
    }
}
