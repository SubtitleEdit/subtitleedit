using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System.Collections;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

public class PointSyncViaOtherWindow : Window
{
    /// <summary>
    /// A line starting after at least this much silence is a likely reliable sync point -
    /// two independently made subtitle files tend to truly align there (issue #10175).
    /// </summary>
    private const double SyncCandidateGapMs = 3000;

    // A translucent tint over the row background, so it reads in both light and dark theme
    // and the selection highlight still shows through.
    private static readonly IBrush SyncCandidateBrush = new ImmutableSolidColorBrush(Color.FromRgb(76, 175, 80), 0.3);

    public PointSyncViaOtherWindow(PointSyncViaOtherViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Sync.PointSyncViaOther);
        Width = 1100;
        Height = 600;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var subtitleViewView = MakeSubtitleView(vm);
        var controlView = MakeControlView(vm);
        var subtitleOtherView = MakeSubtitleOtherView(vm);

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonApply, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(subtitleViewView, 0);
        grid.Add(controlView, 0, 1);
        grid.Add(subtitleOtherView, 0, 2);
        grid.Add(MakeGapLegend(), 1, 0, 1, 3);
        grid.Add(panelButtons, 2, 0, 1, 3);

        Content = grid;

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// Explains the green tint in the "Gap" columns - shown once, under the grids, instead of a
    /// tooltip on every highlighted cell.
    /// </summary>
    private static Control MakeGapLegend()
    {
        var swatch = new Border
        {
            Width = 12,
            Height = 12,
            CornerRadius = new CornerRadius(2),
            Background = SyncCandidateBrush,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var text = new TextBlock
        {
            Text = string.Format(Se.Language.Sync.SyncPointCandidateInfo, (int)(SyncCandidateGapMs / 1000.0)),
            Opacity = 0.75,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 8, 0, 0),
            Children = { swatch, text },
        };
    }

    /// <summary>
    /// The gap before a line's start (unlike the main grid's gap-to-next), tinted when big
    /// enough to mark the line as a sync point candidate (issue #10175).
    /// </summary>
    private static SeTableViewColumn MakeGapColumn()
    {
        var gapConverter = new DoubleToDisplayShortConverter();
        var candidateBrushConverter = new FuncValueConverter<double, IBrush>(gapMs =>
            gapMs >= SyncCandidateGapMs ? SyncCandidateBrush : Brushes.Transparent);

        return new SeTableViewColumn
        {
            Header = Se.Language.General.Gap,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(90),
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.PreviousGap)) { Converter = candidateBrushConverter, Mode = BindingMode.OneWay },
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.PreviousGap)) { Converter = gapConverter, Mode = BindingMode.OneWay },
                };

                border.Child = textBlock;
                return border;
            }),
        };
    }

    private static Control MakeControlView(PointSyncViaOtherViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 60, 0, 0),
            RowSpacing = 10,
        };

        // The DataGrid this replaces hid its header (HeadersVisibility.None); TableView
        // has no such switch, so the single column's header now doubles as the panel title.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.CanUserResizeColumns = false; // single star column, nothing to resize
        // Fixed width: this panel sits in an Auto-sized outer column, and a TableView
        // with a star column measured without a width constraint demands more than the
        // whole window (star columns have no content-based size), squeezing the
        // subtitle grids to slivers and overflowing the right edge.
        dataGrid.Width = 280;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.SyncPoints;
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Sync.SyncPoints,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SyncPoint.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSyncPoint)));
        TableViewExtras.AttachListNavigation(dataGrid);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteSelectedPointSyncCommand,
        };
        var flyout = new MenuFlyout { Items = { menuItemDelete } };
        flyout.Opening += (_, _) => menuItemDelete.IsEnabled = vm.SelectedSyncPoint != null;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);
        dataGrid.KeyDown += (_, e) =>
        {
            if (e.Key is Key.Delete or Key.Back)
            {
                e.Handled = true;
                vm.DeleteSelectedPointSyncCommand.Execute(null);
            }
        };

        var buttonSetSyncPoint = UiUtil.MakeButton(Se.Language.Sync.SetSyncPoint, vm.SetSyncPointCommand)
            .WithIconLeft(IconNames.ArrowLeftRightBold);

        // The other subtitle is the usual source for a sync point here, but a line it does not
        // cover still has to be pinnable - so allow picking the time off the video (issue #13341).
        var buttonSetSyncPointViaVideo = UiUtil.MakeButton(Se.Language.Sync.SetSyncPointViaVideo, vm.SetSyncPointViaVideoCommand)
            .WithIconLeft(IconNames.MovieOpenOutline);

        var panelSetSyncPoint = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Children =
            {
                buttonSetSyncPoint,
                buttonSetSyncPointViaVideo,
            }
        };

        grid.Add(panelSetSyncPoint, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
    }

    private static Grid MakeSubtitleView(PointSyncViaOtherViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(50, GridUnitType.Pixel) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // TextBlock in a star column so a long file name shrinks with an ellipsis
        // instead of pushing under the "Find text" button.
        var labelFileName = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(5, 0, 5, 0),
        };
        labelFileName.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FileName)) { Source = vm });
        var buttonFindText = UiUtil.MakeButton(Se.Language.Sync.FindText, vm.FindTextLeftCommand)
            .WithIconLeft(IconNames.Find);
        buttonFindText.HorizontalAlignment = HorizontalAlignment.Right;
        var panelHeader = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        panelHeader.Add(labelFileName, 0, 0);
        panelHeader.Add(buttonFindText, 0, 1);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): both grids show lines in timeline order, which the sync-point
        // matching relies on.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Subtitles;
        dataGrid.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                Width = new GridLength(60), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                Width = new GridLength(115),
            },
            MakeGapColumn(),
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)));
        TableViewExtras.AttachListNavigation(dataGrid);

        grid.Add(panelHeader, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 1);

        return grid;
    }

    private static Grid MakeSubtitleOtherView(PointSyncViaOtherViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(50, GridUnitType.Pixel) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var buttonBrowseOther = UiUtil.MakeButtonBrowse(vm.BrowseOtherCommand, accessibleName: Se.Language.General.OpenSubtitleFileTitle);
        // TextBlock in a star column so a long file name shrinks with an ellipsis
        // instead of pushing under the "Find text" button.
        var labelOtherFileName = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(5, 0, 5, 0),
        };
        labelOtherFileName.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FileNameOther)) { Source = vm });
        var panelOtherBrowse = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        panelOtherBrowse.Add(buttonBrowseOther, 0, 0);
        panelOtherBrowse.Add(labelOtherFileName, 0, 1);
        var buttonFindTextOther = UiUtil.MakeButton(Se.Language.Sync.FindText, vm.FindTextOtherCommand)
            .WithIconLeft(IconNames.Find);
        buttonFindTextOther.HorizontalAlignment = HorizontalAlignment.Right;
        var panelOtherHeader = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        panelOtherHeader.Add(panelOtherBrowse, 0, 0);
        panelOtherHeader.Add(buttonFindTextOther, 0, 1);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Othersubtitles;
        dataGridSubtitle.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                Width = new GridLength(60), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                Width = new GridLength(115),
            },
            MakeGapColumn(),
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGridSubtitle.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedOtherSubtitle)));
        TableViewExtras.AttachListNavigation(dataGridSubtitle);

        // Clicking a line in the left grid scrolls this grid to the matching time (#12529)
        // without touching its selection.
        vm.ScrollOtherToLine = line => dataGridSubtitle.ScrollIntoView(line);

        grid.Add(panelOtherHeader, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle), 1);

        return grid;
    }

}