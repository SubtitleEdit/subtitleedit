using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync;

/// <summary>
/// The pieces only the point sync windows use, on top of <see cref="EditorChrome"/>: the sync point
/// side panel, list headers with a count, file name labels and wide action buttons.
/// </summary>
internal static class PointSyncChrome
{
    /// <summary>
    /// Width of the sync point side panel.
    /// </summary>
    internal const double SidePanelWidth = 300;

    /// <summary>
    /// Count pill, grey so it reads the same in both themes.
    /// </summary>
    internal static Border MakeCountBadge(string countPath)
    {
        return new Border
        {
            Background = new SolidColorBrush(Colors.Gray, 0.22),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 3, 9, 2),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(countPath),
                FontSize = UiUtil.ScaledFontSize(10),
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    /// <summary>
    /// Section header (small caps) with a count badge after it.
    /// </summary>
    internal static StackPanel MakeHeaderWithCount(string title, string countPath)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(EditorChrome.MakeSectionHeader(title));
        panel.Children.Add(MakeCountBadge(countPath));
        return panel;
    }

    /// <summary>
    /// File name, dim and cut with an ellipsis so a long name never pushes the buttons beside it away.
    /// </summary>
    internal static TextBlock MakeFileNameLabel(object source, string fileNamePath)
    {
        var label = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        label.Bind(TextBlock.TextProperty, new Binding(fileNamePath) { Source = source });
        return label;
    }

    /// <summary>
    /// Small file icon placed before a file name.
    /// </summary>
    internal static Optris.Icons.Avalonia.Icon MakeFileIcon(string icon = "fa-solid fa-file-lines")
    {
        return new Optris.Icons.Avalonia.Icon
        {
            Value = icon,
            FontSize = 12,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        };
    }

    /// <summary>
    /// Icon button with its label beside it, stretched over the side panel's width.
    /// </summary>
    internal static Button MakeWideAction(string icon, string text, ICommand command)
    {
        var button = EditorChrome.MakeActionButton(icon, text, command);
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.HorizontalContentAlignment = HorizontalAlignment.Left;
        button.BorderThickness = new Thickness(1);
        button.BorderBrush = EditorChrome.FieldBorderBrush;
        return button;
    }

    /// <summary>
    /// The sync point list both point sync windows have: one star column, a Delete context menu
    /// and Delete/Backspace to remove the selected point.
    /// </summary>
    internal static Border MakeSyncPointList(object vm, IEnumerable items, string selectedSyncPointPath, ICommand deleteCommand, Func<bool> hasSelectedSyncPoint)
    {
        // The DataGrid this replaces hid its header (HeadersVisibility.None); TableView
        // has no such switch, so the single column's header now doubles as the panel title.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.CanUserResizeColumns = false; // single star column, nothing to resize
        // Fixed width: this panel sits in an Auto-sized outer column, and a TableView
        // with a star column measured without a width constraint demands more than the
        // whole window (star columns have no content-based size), squeezing the
        // subtitle grids to slivers and overflowing the right edge.
        dataGrid.Width = SidePanelWidth - 30;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = items;
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Sync.SyncPoints,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SyncPoint.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(selectedSyncPointPath));
        dataGrid.WithAccessibleName(Se.Language.Sync.SyncPoints);
        TableViewExtras.AttachListNavigation(dataGrid);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = deleteCommand,
        };
        var flyout = new MenuFlyout { Items = { menuItemDelete } };
        flyout.Opening += (_, _) => menuItemDelete.IsEnabled = hasSelectedSyncPoint();
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);
        dataGrid.KeyDown += (_, e) =>
        {
            if (e.Key is Key.Delete or Key.Back)
            {
                e.Handled = true;
                deleteCommand.Execute(null);
            }
        };

        return EditorChrome.MakeListFrame(dataGrid);
    }

    /// <summary>
    /// The sync point side panel: the "set sync point" actions on top, the list under them.
    /// </summary>
    internal static Border MakeSidePanel(Border syncPointList, string countPath, Thickness lines, params Button[] actions)
    {
        var actionPanel = new StackPanel { Spacing = 6 };
        foreach (var action in actions)
        {
            actionPanel.Children.Add(action);
        }

        var header = MakeHeaderWithCount(Se.Language.Sync.SyncPoints, countPath);
        header.Margin = new Thickness(0, 6, 0, 0);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };
        grid.Add(actionPanel, 0);
        grid.Add(header, 1);
        grid.Add(syncPointList, 2);

        return new Border
        {
            Child = grid,
            Width = SidePanelWidth,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = lines,
        };
    }

    /// <summary>
    /// A subtitle list pane: header row on top, the framed list under it.
    /// </summary>
    internal static Grid MakeListPane(Control header, Control list)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            RowSpacing = 8,
            Margin = new Thickness(14, 12, 14, 14),
        };
        grid.Add(header, 0);
        grid.Add(list, 1);
        return grid;
    }

    /// <summary>
    /// A header row: <paramref name="left"/> takes the space, <paramref name="right"/> sits at the end.
    /// </summary>
    internal static Grid MakeHeaderRow(Control left, Control? right)
    {
        var grid = new Grid
        {
            MinHeight = EditorChrome.ScaledSize(30),
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        left.VerticalAlignment = VerticalAlignment.Center;
        grid.Add(left, 0, 0);
        if (right != null)
        {
            right.VerticalAlignment = VerticalAlignment.Center;
            grid.Add(right, 0, 1);
        }

        return grid;
    }

    /// <summary>
    /// Color of the "synced" marker - violet, so it stands apart from the green gap badges and
    /// from the accent colored row selection (issue #17053).
    /// </summary>
    internal static readonly IBrush SyncedBrush = new ImmutableSolidColorBrush(Color.FromRgb(142, 84, 233));

    /// <summary>
    /// "Synced line" sample for the button bar legend.
    /// </summary>
    internal static Control MakeSyncedLineLegend()
    {
        var synced = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        synced.Children.Add(MakeSyncedPill(new TextBlock { Text = "#", FontSize = EditorChrome.ScaledFont(11) }));
        synced.Children.Add(new TextBlock
        {
            Text = Se.Language.Sync.SyncedLine,
            FontSize = EditorChrome.ScaledFont(11.5),
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        });
        return synced;
    }

    /// <summary>
    /// Violet pill with a link icon, holding the line number of a synced line.
    /// </summary>
    internal static Border MakeSyncedPill(TextBlock number)
    {
        number.Foreground = Brushes.White;
        number.FontWeight = FontWeight.SemiBold;
        number.VerticalAlignment = VerticalAlignment.Center;

        return new Border
        {
            Background = SyncedBrush,
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(6, 1, 7, 1),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon
                    {
                        Value = "fa-solid fa-link",
                        FontSize = EditorChrome.ScaledFont(9),
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    number,
                },
            },
        };
    }

    /// <summary>
    /// The "#" column: a plain number, or - for a line that is part of a sync point - the number
    /// in a violet pill with a link icon, kept until the sync point is removed (issue #17053).
    /// </summary>
    internal static SeTableViewColumn MakeSyncedNumberColumn(ISyncedLineMarker vm)
    {
        return new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            NameBinding = new Binding(nameof(SubtitleLineViewModel.Number)) { Mode = BindingMode.OneWay },
            Width = new GridLength(70), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
            {
                var plain = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Number)) { Mode = BindingMode.OneWay },
                };
                var pillText = new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Number)) { Mode = BindingMode.OneWay },
                };
                var pill = MakeSyncedPill(pillText);
                pill.Margin = new Thickness(-4, 0, 0, 0);

                // Accent bar on the row's left edge, so a synced row also stands out when scrolling fast.
                var bar = new Border
                {
                    Width = 3,
                    Background = SyncedBrush,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                var cell = new Grid();
                cell.Children.Add(bar);
                cell.Children.Add(new Border { Padding = new Thickness(8, 2, 4, 2), Child = new Panel { Children = { plain, pill } } });

                void Update()
                {
                    var isSynced = cell.DataContext is SubtitleLineViewModel line && vm.IsSynced(line);
                    plain.IsVisible = !isSynced;
                    pill.IsVisible = isSynced;
                    bar.IsVisible = isSynced;
                }

                // Cells are recycled, so re-mark on a new line and whenever sync points change -
                // only while the cell is shown, so a dropped cell does not stay subscribed.
                cell.DataContextChanged += (_, _) => Update();
                cell.AttachedToVisualTree += (_, _) =>
                {
                    vm.SyncedLinesChanged += Update;
                    Update();
                };
                cell.DetachedFromVisualTree += (_, _) => vm.SyncedLinesChanged -= Update;
                Update();

                return cell;
            }),
        };
    }
}
