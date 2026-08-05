using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// A <see cref="TableViewColumn"/> with the extras SE's grids need beyond what the
/// control offers: a <see cref="Tag"/> for stable column keys (width persistence),
/// a <see cref="MinWidth"/> hint, and a bindable <see cref="IsVisible"/> property.
/// TableViewColumn has no visibility concept, so a <see cref="TableViewColumnManager"/>
/// watches <see cref="IsVisible"/> and adds/removes the column from the TableView,
/// keeping the original column order.
/// </summary>
public class SeTableViewColumn : TableViewColumn
{
    public SeTableViewColumn()
    {
        // TableViewColumn defaults to Left, which makes cell content shrink-wrap
        // horizontally - a cell template's colored background Border would then only
        // cover the text instead of the whole cell (the DataGrid stretched cell
        // content). Stretch restores full-cell backgrounds; templates that want
        // centering (e.g. the number column) set it on their own root.
        HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
    }

    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<SeTableViewColumn, bool>(nameof(IsVisible), defaultValue: true);

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public object? Tag { get; set; }

    public double MinWidth { get; set; }
}

/// <summary>
/// Click-to-sort for TableView column headers (TableView itself has no sorting).
/// Register the sortable columns with a key selector; clicking a registered header
/// sorts the grid's ItemsSource in place (stable, toggling ascending/descending on
/// repeated clicks) and shows an arrow in the header. Selection is preserved.
/// Note this reorders the backing collection - unlike the DataGrid, which sorted a
/// view - so only use it on grids where the collection order is presentation-only.
/// </summary>
public sealed class TableViewHeaderSorter
{
    private readonly TableView _tableView;
    private readonly Dictionary<TableViewColumn, Comparison<object>> _comparisons = new();
    private readonly Dictionary<TableViewColumn, object?> _originalHeaders = new();
    private TableViewColumn? _sortColumn;
    private bool _descending;

    public TableViewHeaderSorter(TableView tableView)
    {
        _tableView = tableView;
        tableView.AddHandler(InputElement.TappedEvent, OnTapped, Avalonia.Interactivity.RoutingStrategies.Bubble);
    }

    /// <summary>Makes <paramref name="column"/> sortable by <paramref name="key"/>.</summary>
    public TableViewHeaderSorter AddSortable<TItem, TKey>(TableViewColumn column, Func<TItem, TKey> key)
    {
        return AddSortable(column, (a, b) => Comparer<TKey>.Default.Compare(key((TItem)a), key((TItem)b)));
    }

    /// <summary>Makes <paramref name="column"/> sortable by a custom comparison of the row items.</summary>
    public TableViewHeaderSorter AddSortable(TableViewColumn column, Comparison<object> comparison)
    {
        _comparisons[column] = comparison;
        _originalHeaders[column] = column.Header;
        return this;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Visual source ||
            source.FindAncestorOfType<Thumb>(includeSelf: true) != null) // resize grip
        {
            return;
        }

        var header = source.FindAncestorOfType<TableViewColumnHeader>(includeSelf: true);
        if (header?.Column is not { } column || !_comparisons.TryGetValue(column, out var comparison))
        {
            return;
        }

        _descending = ReferenceEquals(_sortColumn, column) && !_descending;
        _sortColumn = column;
        Apply(comparison);
        UpdateHeaders();
        e.Handled = true;
    }

    private void Apply(Comparison<object> comparison)
    {
        if (_tableView.ItemsSource is not System.Collections.IList list || list.Count < 2)
        {
            return;
        }

        var comparer = Comparer<object>.Create(comparison);
        var items = list.Cast<object>();
        var sorted = (_descending
            ? items.OrderByDescending(x => x, comparer)
            : items.OrderBy(x => x, comparer)).ToList(); // OrderBy is stable in both directions

        var selectedSet = new HashSet<object>(_tableView.SelectedItems?.Cast<object>() ?? Enumerable.Empty<object>());
        var selectedItem = _tableView.SelectedItem;

        // Detach while refilling: per-item CollectionChanged handling on an attached
        // 10k-row grid is far slower than one full reset (measured headless).
        var itemsSource = _tableView.ItemsSource;
        _tableView.ItemsSource = null;
        list.Clear();
        foreach (var item in sorted)
        {
            list.Add(item);
        }

        _tableView.ItemsSource = itemsSource;

        // Restore selection by index through the selection model - SelectedItems.Add
        // does an O(n) IndexOf per item, which is quadratic on large selections.
        if (selectedSet.Count > 0)
        {
            _tableView.Selection.BeginBatchUpdate();
            for (var i = 0; i < sorted.Count && selectedSet.Count > 0; i++)
            {
                if (selectedSet.Remove(sorted[i]))
                {
                    _tableView.Selection.Select(i);
                }
            }

            _tableView.Selection.EndBatchUpdate();
        }

        _tableView.SelectedItem = selectedItem;
        if (selectedItem != null)
        {
            _tableView.ScrollIntoView(selectedItem);
        }
    }

    private void UpdateHeaders()
    {
        foreach (var (column, originalHeader) in _originalHeaders)
        {
            column.Header = ReferenceEquals(column, _sortColumn)
                ? $"{originalHeader} {(_descending ? "▼" : "▲")}"
                : originalHeader;
        }
    }
}

/// <summary>
/// Owns the full, ordered column list of a <see cref="TableView"/> and keeps the
/// control's live <see cref="TableView.Columns"/> in sync with each
/// <see cref="SeTableViewColumn.IsVisible"/>: hidden columns are removed from the
/// control (TableView renders every column it holds), visible ones are re-inserted
/// at their original position.
/// </summary>
public sealed class TableViewColumnManager
{
    private readonly TableView _tableView;
    private readonly List<TableViewColumn> _columns = new();

    public TableViewColumnManager(TableView tableView)
    {
        _tableView = tableView;
    }

    /// <summary>All managed columns in display order, including hidden ones.</summary>
    public IReadOnlyList<TableViewColumn> Columns => _columns;

    public void Add(TableViewColumn column)
    {
        _columns.Add(column);
        if (column is SeTableViewColumn seColumn)
        {
            seColumn.PropertyChanged += (_, e) =>
            {
                if (e.Property == SeTableViewColumn.IsVisibleProperty)
                {
                    Sync();
                }
            };
        }

        Sync();
    }

    private void Sync()
    {
        var target = _columns.Where(c => c is not SeTableViewColumn se || se.IsVisible).ToList();
        var live = _tableView.Columns;

        // Remove columns that should no longer be shown. After this, the live list is a
        // subsequence of the target list (both preserve the master order), so the missing
        // ones can simply be inserted at their target positions.
        for (var i = live.Count - 1; i >= 0; i--)
        {
            if (!target.Contains(live[i]))
            {
                live.RemoveAt(i);
            }
        }

        for (var i = 0; i < target.Count; i++)
        {
            if (i >= live.Count || !ReferenceEquals(live[i], target[i]))
            {
                live.Insert(i, target[i]);
            }
        }
    }
}

/// <summary>
/// Reusable behaviors for <see cref="TableView"/>-based grids: reliable scrolling
/// (center / fully-visible), row hit-testing, page size, batched selection updates and
/// per-row style bindings. These are the TableView counterparts of the hand-rolled
/// DataGrid machinery the main subtitle grid accumulated over time; use them for any
/// new TableView so the behavior stays consistent.
/// </summary>
public static class TableViewExtras
{
    private static readonly TextToFlowDirectionConverter TextToFlowDirection = new();

    /// <summary>
    /// Creates a TableView with SE's standard look and behavior (multi-select by
    /// default, resizable columns, tight row style).
    /// </summary>
    public static TableView MakeTableView(bool alwaysSelected = true, bool multiSelect = true)
    {
        var selectionMode = multiSelect ? SelectionMode.Multiple : SelectionMode.Single;
        if (alwaysSelected)
        {
            selectionMode |= SelectionMode.AlwaysSelected;
        }

        var tableView = new TableView
        {
            SelectionMode = selectionMode,
            CanUserResizeColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,

            // The DataGrid had no background of its own, so SE's panel background showed
            // through; TableView's theme paints SystemControlBackgroundChromeMediumLowBrush,
            // which in dark mode is within a shade or two of the alternating-row tint
            // (#2D2D2D) and swallowed it completely. Transparent restores the DataGrid
            // backdrop so row tints contrast against the app background again.
            Background = Brushes.Transparent,
        };

        UiUtil.ApplyTableViewRowStyle(tableView);

        // SelectionMode.AlwaysSelected picks row 0 the moment ItemsSource is assigned, but that
        // pick only reaches the internal selection model (and SelectedItem/SelectedIndex) - the
        // SelectedItems collection stays empty and no SelectionChanged is raised. Repair it
        // (#13230), see SyncSelectedItemsWithSelection.
        tableView.PropertyChanged += (_, e) =>
        {
            if (e.Property == ItemsControl.ItemsSourceProperty)
            {
                SyncSelectedItemsWithSelection(tableView);
            }
        };

        // Home/End/PageUp/PageDown (and Ctrl+Home/End) list navigation in every grid (#13194).
        // Handlers attached earlier in the routing path (window-level tunnel handlers, shortcut
        // dispatch) still win; this is the fallback when nothing else handled the key.
        AttachListNavigation(tableView);

        return tableView;
    }

    /// <summary>
    /// Puts the rows the control considers selected into its <see cref="SelectingItemsControl.SelectedItems"/>
    /// collection when that collection is empty but the selection model is not.
    /// <para>
    /// <see cref="SelectionMode.AlwaysSelected"/> selects row 0 as soon as ItemsSource is assigned
    /// to a populated collection. That selection reaches the selection model - the row is drawn
    /// highlighted and SelectedItem/SelectedIndex point at it - but SelectedItems is left empty and
    /// no SelectionChanged is raised, and neither a layout pass nor a Selection.Clear()/Select(0)
    /// repairs it; only moving the selection to a different row does. Everything in Subtitle Edit
    /// reads the selection through SelectedItems, so the grid showed row 1 highlighted while the
    /// app saw nothing selected: the edit box, Show and Duration went blank the moment anything
    /// re-read the selection, and a shift-selection starting at row 1 silently left row 1 out of
    /// every operation (issue #13230).
    /// </para>
    /// </summary>
    public static void SyncSelectedItemsWithSelection(TableView tableView)
    {
        var selectedItems = tableView.SelectedItems;
        if (selectedItems == null || selectedItems.Count > 0)
        {
            return;
        }

        var selection = tableView.Selection;
        if (selection.Count == 0)
        {
            return;
        }

        foreach (var item in selection.SelectedItems)
        {
            if (item != null)
            {
                selectedItems.Add(item);
            }
        }
    }

    /// <summary>
    /// A read-only text cell whose flow direction follows its own content, the way the
    /// main subtitle grid's text cells do. Use it for every column showing subtitle text
    /// instead of the column's plain <c>Binding</c>.
    /// <para>
    /// A plain binding leaves the cell with the window's left-to-right direction, and
    /// Avalonia then lays a right-to-left line out under a left-to-right base direction:
    /// the line is cut at every zero width non-joiner (U+200C, bidi class BN, which
    /// Avalonia leaves at the paragraph level instead of giving it the surrounding
    /// right-to-left level) and the resulting runs are placed left to right. Persian and
    /// Arabic words written with a ZWNJ are torn in half and the word order is reversed
    /// (issue #13160).
    /// </para>
    /// </summary>
    public static IDataTemplate MakeTextCellTemplate(string propertyPath)
    {
        return new FuncDataTemplate<object>((_, _) => new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(propertyPath) { Mode = BindingMode.OneWay },
            [!TextBlock.FlowDirectionProperty] = new Binding(propertyPath) { Converter = TextToFlowDirection, Mode = BindingMode.OneWay },
        });
    }

    /// <summary>
    /// Adds a style that binds <paramref name="property"/> on every row to
    /// <paramref name="binding"/> (evaluated against the row's DataContext, i.e. the
    /// item). Used e.g. to collapse hidden rows or to expose an accessible name.
    /// </summary>
    public static void BindRowProperty(TableView tableView, AvaloniaProperty property, BindingBase binding)
    {
        tableView.Styles.Add(new Style(x => x.OfType<TableViewRow>())
        {
            Setters = { new Setter(property, binding) },
        });
    }

    /// <summary>
    /// Tints every other row with <paramref name="brush"/>. Applied per container from
    /// the ContainerPrepared/ContainerIndexChanged events, so the tint stays correct
    /// when rows are inserted, removed or recycled. Selection and hover still win:
    /// the row theme's :selected/:pointerover styles set the template Border's
    /// background directly, overriding the row's own Background.
    /// </summary>
    public static void ApplyAlternatingRows(TableView tableView, IBrush brush)
    {
        static void Apply(Control container, int index, IBrush alternatingBrush)
        {
            if (container is not TableViewRow row)
            {
                return;
            }

            if (index % 2 == 1)
            {
                row.Background = alternatingBrush;
            }
            else
            {
                row.ClearValue(TemplatedControl.BackgroundProperty);
            }
        }

        tableView.ContainerPrepared += (_, e) => Apply(e.Container, e.Index, brush);
        tableView.ContainerIndexChanged += (_, e) => Apply(e.Container, e.NewIndex, brush);
    }

    /// <summary>
    /// Home/End jump to the first/last row and PageUp/PageDown move the selection a page,
    /// even when the TableView itself (not a row) has keyboard focus - ListBox's native
    /// handling only runs with focus on an item, and its virtualizing panel ignores the
    /// page keys entirely, so they only scrolled and left the selection behind (#13060).
    /// Skipped when the key originates in a TextBox (e.g. an in-cell editor).
    /// </summary>
    public static void AttachListNavigation(TableView tableView)
    {
        tableView.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is not (Key.Home or Key.End or Key.PageUp or Key.PageDown) ||
                e.KeyModifiers is not (KeyModifiers.None or KeyModifiers.Control) ||
                e.Source is TextBox)
            {
                return;
            }

            if (tableView.ItemsSource is not System.Collections.IList items || items.Count == 0)
            {
                return;
            }

            var targetIndex = e.Key switch
            {
                Key.Home => 0,
                Key.End => items.Count - 1,
                _ => GetPageTarget(tableView, tableView.SelectedIndex, e.Key == Key.PageDown),
            };

            if (targetIndex < 0 || targetIndex >= items.Count)
            {
                return;
            }

            var target = items[targetIndex];
            if (target == null)
            {
                return;
            }

            PrePositionScroll(tableView, targetIndex);
            tableView.SelectedItem = target;
            tableView.ScrollIntoView(target);
            EnsureRowFullyVisible(tableView, target);

            // The jump can scroll the currently focused row container out of the realized
            // range, dropping keyboard focus out of the TableView entirely (the next
            // Home/End would then never reach this handler). The key came through the
            // TableView, so focus belongs inside it - move it to the target row once
            // layout has realized the container.
            Dispatcher.UIThread.Post(() => FocusRow(tableView));

            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Space toggles the checkbox value of every selected row - all rows checked means
    /// uncheck all, otherwise check all. This is the piece of the DataGrid-era
    /// CheckboxMultiSelect helper that TableView does not provide natively (extended
    /// selection itself is native ListBox behavior).
    /// </summary>
    public static void AddSpaceToggle<TItem>(TableView tableView, Func<TItem, bool> getChecked, Action<TItem, bool> setChecked)
        where TItem : class
    {
        tableView.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key != Key.Space)
            {
                return;
            }

            var selected = tableView.SelectedItems?.OfType<TItem>().ToList();
            if (selected == null || selected.Count == 0)
            {
                return;
            }

            var newValue = !selected.All(getChecked);
            foreach (var item in selected)
            {
                setChecked(item, newValue);
            }

            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Two-way binds the TableView's selection to <paramref name="propertyName"/> on
    /// <paramref name="viewModel"/>, and pushes the row the control has already selected into
    /// that property when it is still null.
    ///
    /// <see cref="MakeTableView"/> keeps <see cref="SelectionMode.AlwaysSelected"/>, so the
    /// control selects row 0 the moment ItemsSource is assigned - before this binding exists -
    /// and a binding's first push goes source to target. For a list that was already populated
    /// (the dialog pattern, where the view model is initialized before the window is built) the
    /// grid therefore shows row 0 highlighted while the view model still sees null: previews
    /// stay empty, OK picks nothing and "is anything selected" flags stay false, until the user
    /// clicks another row and back again. Prefer this over binding SelectedItem by hand.
    /// </summary>
    public static void BindSelectedItem(TableView tableView, object viewModel, string propertyName)
    {
        tableView.Bind(TableView.SelectedItemProperty, new Binding(propertyName)
        {
            Source = viewModel,
            Mode = BindingMode.TwoWay,
        });

        if (tableView.SelectedItem is not { } selected)
        {
            return;
        }

        var property = viewModel.GetType().GetProperty(propertyName);
        if (property is { CanWrite: true } &&
            property.GetValue(viewModel) == null &&
            property.PropertyType.IsInstanceOfType(selected))
        {
            property.SetValue(viewModel, selected);
        }
    }

    /// <summary>
    /// Moves keyboard focus to the selected row's container when it is realized, falling
    /// back to the TableView itself. Focusing the row (not the list control) is what makes
    /// the current line visible to screen readers via UI Automation (issue #13015).
    /// </summary>
    public static void FocusRow(TableView tableView)
    {
        if (tableView.SelectedItem is { } item &&
            tableView.ContainerFromItem(item) is { } container)
        {
            container.Focus();
            return;
        }

        tableView.Focus();
    }

    /// <summary>
    /// Returns the item index of the row under <paramref name="position"/> (relative to
    /// the TableView), or -1 when the point is not over a row.
    /// </summary>
    public static int GetRowIndexFromPoint(TableView tableView, Point position)
    {
        var current = tableView.InputHitTest(position) as Control;
        while (current != null)
        {
            if (current is TableViewRow row)
            {
                return tableView.IndexFromContainer(row);
            }

            current = current.Parent as Control;
        }

        return -1;
    }

    /// <summary>True when the visual (from a hit test) is inside a column header.</summary>
    public static bool IsInColumnHeader(Visual? visual)
    {
        return visual.FindAncestorOfType<TableViewColumnHeader>(includeSelf: true) != null;
    }

    /// <summary>True when the visual (from a hit test) is inside a scrollbar.</summary>
    public static bool IsInScrollBar(Visual? visual)
    {
        return visual.FindAncestorOfType<ScrollBar>(includeSelf: true) != null;
    }

    /// <summary>
    /// Rough number of rows in a page, from the realized row count. Only a fallback for
    /// <see cref="GetPageTarget"/> - realization overshoots the viewport, so with variable
    /// row heights this over-counts; prefer the geometry-based target.
    /// </summary>
    public static int GetPageSize(TableView tableView)
    {
        var visibleRowCount = tableView.GetVisualDescendants()
            .OfType<TableViewRow>()
            .Count(r => r.IsVisible && r.Bounds.Height > 0);
        return Math.Max(1, visibleRowCount - 1);
    }

    /// <summary>
    /// The row a PageDown (<paramref name="down"/>) or PageUp from <paramref name="currentIndex"/>
    /// should move the selection to: the last (first) fully visible row, or - when the current row
    /// already is that row - the edge row one viewport further on. This is what Explorer and the
    /// WinForms/WPF list controls do, and unlike stepping a fixed row count it stays correct with
    /// the variable row heights of the subtitle grid (#13060): the step is read from the rows
    /// actually on screen, and the target row is on screen by construction.
    /// Note this may scroll the grid by one viewport - that is how the next edge row is located
    /// exactly rather than estimated; callers select and scroll to the returned index as usual.
    /// Returns -1 for an empty grid.
    /// </summary>
    public static int GetPageTarget(TableView tableView, int currentIndex, bool down)
    {
        var count = tableView.ItemCount;
        if (count == 0)
        {
            return -1;
        }

        currentIndex = Math.Clamp(currentIndex, 0, count - 1);
        var fallback = Math.Clamp(currentIndex + (down ? GetPageSize(tableView) : -GetPageSize(tableView)), 0, count - 1);

        var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer == null || scrollViewer.Viewport.Height <= 0)
        {
            return fallback;
        }

        // Without the current row on screen there is no edge to move from (the user scrolled
        // away with the wheel, or the row is taller than the viewport) - estimate instead.
        var visible = GetFullyVisibleRows(tableView, scrollViewer);
        if (visible.All(x => x.Index != currentIndex))
        {
            return fallback;
        }

        var edge = down ? visible[^1] : visible[0];
        if (edge.Index != currentIndex)
        {
            return edge.Index;
        }

        // Already at the edge: move on a viewport by putting the current row at the opposite
        // edge, then take whatever row now sits at this edge.
        var delta = down ? edge.Top : edge.Top + edge.Height - scrollViewer.Viewport.Height;
        var newY = Math.Max(0, Math.Min(scrollViewer.Offset.Y + delta, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
        if (Math.Abs(newY - scrollViewer.Offset.Y) < 1)
        {
            // Nowhere left to scroll - the list end is already on screen.
            return down ? visible[^1].Index : visible[0].Index;
        }

        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newY);
        tableView.UpdateLayout();

        visible = GetFullyVisibleRows(tableView, scrollViewer);
        if (visible.Count == 0)
        {
            return fallback;
        }

        var target = down ? visible[^1].Index : visible[0].Index;

        // A single row taller than half the viewport can leave the current row at the edge
        // again; always advance at least one row so the key is never a no-op.
        return target != currentIndex ? target : Math.Clamp(currentIndex + (down ? 1 : -1), 0, count - 1);
    }

    /// <summary>
    /// The realized rows that are completely inside the viewport, ordered by item index,
    /// with each row's top in viewport coordinates.
    /// </summary>
    private static List<(int Index, double Top, double Height)> GetFullyVisibleRows(TableView tableView, ScrollViewer scrollViewer)
    {
        var rows = new List<(int Index, double Top, double Height)>();
        foreach (var row in tableView.GetRealizedContainers().OfType<TableViewRow>())
        {
            var height = row.Bounds.Height;
            if (height <= 0)
            {
                continue;
            }

            var index = tableView.IndexFromContainer(row);
            if (index < 0)
            {
                continue;
            }

            var top = ((Visual)row).TranslatePoint(new Point(0, 0), scrollViewer)?.Y;
            if (top == null || top.Value < -0.5 || top.Value + height > scrollViewer.Viewport.Height + 0.5)
            {
                continue;
            }

            rows.Add((index, top.Value, height));
        }

        rows.Sort((a, b) => a.Index.CompareTo(b.Index));
        return rows;
    }

    /// <summary>Refinement steps <see cref="PrePositionScroll"/> is allowed to take.</summary>
    private const int PrePositionScrollPasses = 3;

    /// <summary>
    /// Moves the scroll offset next to <paramref name="index"/> before a
    /// <see cref="ItemsControl.ScrollIntoView(int)"/>, so the panel never has to travel there
    /// row by row.
    /// TableView's VirtualizingStackPanel locates an unrealized row by estimating its offset
    /// from the average row height, and with variable-height rows that estimate drifts. When it
    /// drifts far enough the panel falls back to walking - realizing every single row between
    /// where it is and where it is going. Measured on a 5000-line file: Home realized 78, 139,
    /// 184, ... 513 containers on successive Home/End round trips (~1 s per keypress, getting
    /// worse), and a jump to line 100 realized 4935 containers - the entire file - in every
    /// second attempt, taking >10 seconds. Lines 400 and 700 hit the same walk less often. End
    /// never did: the drift only bites when travelling towards the top.
    /// Setting the offset skips rows instead of realizing them, so this walks the estimate in
    /// instead: measure where the realized rows actually sit, jump, remeasure, at most three
    /// times. Each pass realizes one viewport, and ScrollIntoView then has less than half a
    /// viewport left to cover. Measured after: 16-77 containers for any target, no walks.
    /// </summary>
    public static void PrePositionScroll(TableView tableView, int index)
    {
        var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer == null || scrollViewer.Viewport.Height <= 0)
        {
            return;
        }

        // The top is the one position that needs no estimate at all.
        if (index == 0)
        {
            if (scrollViewer.Offset.Y > 0.5)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, 0);
            }

            return;
        }

        for (var pass = 0; pass < PrePositionScrollPasses; pass++)
        {
            var rows = tableView.GetRealizedContainers()
                .OfType<TableViewRow>()
                .Where(r => r.Bounds.Height > 0)
                .Select(r => (Index: tableView.IndexFromContainer(r), Row: r))
                .Where(x => x.Index >= 0)
                .OrderBy(x => x.Index)
                .ToList();

            // Nothing to measure from, or the target is realized already - ScrollIntoView is
            // cheap from here.
            if (rows.Count == 0 || rows.Any(x => x.Index == index))
            {
                return;
            }

            var first = rows[0];
            var firstTop = ((Visual)first.Row).TranslatePoint(new Point(0, 0), scrollViewer)?.Y;
            if (firstTop == null)
            {
                return;
            }

            // Where the target should be, from the heights actually on screen rather than from
            // the panel's own (drifting) average.
            var averageRowHeight = rows.Average(x => x.Row.Bounds.Height);
            var delta = ((index - first.Index) * averageRowHeight) - firstTop.Value;
            if (Math.Abs(delta) < scrollViewer.Viewport.Height / 2)
            {
                return;
            }

            var newY = Math.Max(0, Math.Min(scrollViewer.Offset.Y + delta, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
            if (Math.Abs(newY - scrollViewer.Offset.Y) < 1)
            {
                return;
            }

            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newY);

            // Realize the rows at the new offset so the next pass has something to measure
            // (and so the caller's ScrollIntoView starts from the corrected position).
            tableView.UpdateLayout();
        }
    }

    /// <summary>
    /// Scrolls so <paramref name="item"/>'s row is vertically centered in the viewport.
    /// Posted at Loaded priority so the built-in ScrollIntoView (which realizes the row)
    /// has taken effect first.
    /// </summary>
    public static void CenterRow(TableView tableView, object item)
    {
        AdjustScrollForRow(tableView, item, (rowTop, rowHeight, viewportHeight) =>
            rowTop - (viewportHeight - rowHeight) / 2.0);
    }

    /// <summary>
    /// Nudges the scroll offset so <paramref name="item"/>'s row is fully on screen -
    /// the non-centering counterpart of <see cref="CenterRow"/> for variable-height rows
    /// that ScrollIntoView leaves clipped at an edge.
    /// </summary>
    public static void EnsureRowFullyVisible(TableView tableView, object item)
    {
        AdjustScrollForRow(tableView, item, (rowTop, rowHeight, viewportHeight) =>
        {
            var rowBottom = rowTop + rowHeight;
            if (rowBottom > viewportHeight)
            {
                return rowBottom - viewportHeight; // pokes out the bottom -> scroll down
            }

            if (rowTop < 0)
            {
                return rowTop; // pokes out the top -> scroll up (negative)
            }

            return 0;
        });
    }

    private static void AdjustScrollForRow(TableView tableView, object item, Func<double, double, double, double> computeDelta)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (tableView.ContainerFromItem(item) is not { } row || row.Bounds.Height <= 0)
            {
                return;
            }

            var scrollViewer = tableView.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            if (scrollViewer == null || scrollViewer.Viewport.Height <= 0)
            {
                return;
            }

            // Row top in viewport coordinates.
            var rowTop = row.TranslatePoint(new Point(0, 0), scrollViewer)?.Y;
            if (rowTop == null)
            {
                return;
            }

            var delta = computeDelta(rowTop.Value, row.Bounds.Height, scrollViewer.Viewport.Height);
            if (Math.Abs(delta) < 1)
            {
                return;
            }

            var offset = scrollViewer.Offset;
            var newY = Math.Max(0, Math.Min(offset.Y + delta, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));
            if (Math.Abs(newY - offset.Y) < 0.5)
            {
                return;
            }

            scrollViewer.Offset = new Vector(offset.X, newY);
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Moves the selected rows within <paramref name="items"/> (see <see cref="ListReorder"/>)
    /// and leaves the same rows selected at their new positions, with the moved block
    /// scrolled into view.
    /// </summary>
    public static void MoveSelectedRows<TItem>(TableView tableView, ObservableCollection<TItem> items, ListMoveDirection direction)
        where TItem : class
    {
        var selected = tableView.SelectedItems?.OfType<TItem>().ToList();
        if (selected == null || selected.Count == 0)
        {
            return;
        }

        // The row the control treats as the selection anchor - restored first below so it
        // stays SelectedItem (view models bind their "current row" to that, not to SelectedItems).
        var anchor = tableView.SelectedItem as TItem;

        ListReorder.Move(items, selected.Select(items.IndexOf), direction);

        var remaining = new HashSet<TItem>(selected);
        var newIndices = new List<int>(selected.Count);
        for (var i = 0; i < items.Count && remaining.Count > 0; i++)
        {
            if (remaining.Remove(items[i]))
            {
                newIndices.Add(i);
            }
        }

        if (newIndices.Count == 0)
        {
            return;
        }

        // ObservableCollection.Move raises one CollectionChanged per row, and the selection
        // model's index bookkeeping does not survive a run of them intact - restore the
        // whole selection explicitly. The batch defers SelectionChanged to the end, so
        // SelectionMode.AlwaysSelected never sees the momentarily empty selection and
        // cannot force row 0 back in.
        tableView.Selection.BeginBatchUpdate();
        tableView.Selection.Clear();
        var anchorIndex = anchor != null ? items.IndexOf(anchor) : -1;
        if (anchorIndex >= 0)
        {
            tableView.Selection.Select(anchorIndex); // first Select after Clear sets SelectedIndex
        }

        foreach (var index in newIndices)
        {
            tableView.Selection.Select(index);
        }

        tableView.Selection.EndBatchUpdate();

        var edgeIndex = direction is ListMoveDirection.Up or ListMoveDirection.Top ? newIndices[0] : newIndices[^1];
        tableView.ScrollIntoView(items[edgeIndex]);
    }
}

/// <summary>
/// Drag-to-select for a TableView: press on a row and drag to select the range, with
/// accelerating auto-scroll when the pointer nears the top/bottom edge. The host owns
/// how a range becomes a selection (it may batch, keep its own anchor bookkeeping and
/// raise its own changed notifications) via <paramref name="applyRange"/>; this class
/// owns the pointer/timer state machine.
/// </summary>
public sealed class TableViewDragSelect
{
    private const double AutoScrollEdgeSize = 28;
    private const double AutoScrollAccelerationPixels = 18;
    private const int AutoScrollMaxStep = 16;

    private readonly TableView _tableView;

    /// <summary>(anchorIndex, currentIndex) - replace the selection with that range.</summary>
    private readonly Action<int, int> _applyRange;

    private int _startIndex = -1;
    private int _lastIndex = -1;
    private int _autoScrollDirection;
    private int _autoScrollStep = 1;
    private DispatcherTimer? _autoScrollTimer;

    public TableViewDragSelect(TableView tableView, Action<int, int> applyRange)
    {
        _tableView = tableView;
        _applyRange = applyRange;
    }

    /// <summary>True once the pointer has moved to another row during the current press.</summary>
    public bool HasMoved { get; private set; }

    /// <summary>Arms a potential drag-select from a plain left press on the given row.</summary>
    public void Arm(int rowIndex)
    {
        _startIndex = rowIndex;
        _lastIndex = rowIndex;
    }

    /// <summary>Resets all state (e.g. on pointer press, before re-arming).</summary>
    public void Reset()
    {
        StopAutoScroll();
        _startIndex = -1;
        _lastIndex = -1;
        HasMoved = false;
    }

    public void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_startIndex < 0)
        {
            return;
        }

        if (!e.GetCurrentPoint(_tableView).Properties.IsLeftButtonPressed)
        {
            End(e);
            return;
        }

        var position = e.GetPosition(_tableView);
        UpdateAutoScroll(position);

        var currentIndex = TableViewExtras.GetRowIndexFromPoint(_tableView, position);
        if (currentIndex < 0)
        {
            return;
        }

        var wasDragging = HasMoved;
        DragTo(currentIndex);
        if (HasMoved)
        {
            if (!wasDragging && sender is Control control)
            {
                e.Pointer.Capture(control);
            }

            e.Handled = true;
        }
    }

    public void End(PointerEventArgs e)
    {
        StopAutoScroll();
        if (_startIndex >= 0)
        {
            e.Pointer.Capture(null);
        }

        _startIndex = -1;
        _lastIndex = -1;
    }

    private void DragTo(int currentIndex)
    {
        var itemCount = _tableView.ItemCount;
        if (_startIndex < 0 || currentIndex < 0 || currentIndex >= itemCount)
        {
            return;
        }

        _lastIndex = currentIndex;

        if (currentIndex == _startIndex && !HasMoved)
        {
            return;
        }

        HasMoved = true;
        _applyRange(_startIndex, currentIndex);
    }

    private void UpdateAutoScroll(Point position)
    {
        if (_tableView.Bounds.Height <= 0)
        {
            StopAutoScroll();
            return;
        }

        if (position.Y < AutoScrollEdgeSize)
        {
            StartAutoScroll(-1, AutoScrollEdgeSize - position.Y);
        }
        else if (position.Y > _tableView.Bounds.Height - AutoScrollEdgeSize)
        {
            StartAutoScroll(1, position.Y - (_tableView.Bounds.Height - AutoScrollEdgeSize));
        }
        else
        {
            StopAutoScroll();
        }
    }

    private void StartAutoScroll(int direction, double distanceFromEdge)
    {
        if (_lastIndex < 0 || _tableView.ItemCount == 0)
        {
            return;
        }

        _autoScrollDirection = direction;
        var step = 1 + (int)Math.Floor(Math.Max(0, distanceFromEdge) / AutoScrollAccelerationPixels);
        _autoScrollStep = Math.Clamp(step, 1, AutoScrollMaxStep);

        if (_autoScrollTimer != null)
        {
            if (!_autoScrollTimer.IsEnabled)
            {
                _autoScrollTimer.Start();
            }

            return;
        }

        _autoScrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80),
        };
        _autoScrollTimer.Tick += (_, _) => AutoScrollTick();
        _autoScrollTimer.Start();
    }

    private void StopAutoScroll()
    {
        _autoScrollDirection = 0;
        _autoScrollStep = 1;
        _autoScrollTimer?.Stop();
    }

    private void AutoScrollTick()
    {
        if (_startIndex < 0 || _autoScrollDirection == 0)
        {
            StopAutoScroll();
            return;
        }

        var itemCount = _tableView.ItemCount;
        if (itemCount == 0)
        {
            StopAutoScroll();
            return;
        }

        var nextIndex = Math.Clamp(_lastIndex + _autoScrollDirection * _autoScrollStep, 0, itemCount - 1);
        if (nextIndex == _lastIndex)
        {
            StopAutoScroll();
            return;
        }

        DragTo(nextIndex);
        _tableView.ScrollIntoView(nextIndex);
    }
}
