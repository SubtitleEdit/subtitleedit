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
using Avalonia.Styling;
using System.Collections;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

public class PointSyncViaOtherWindow : Window
{
    /// <summary>
    /// A line followed by at least this much silence is a likely reliable sync point -
    /// two independently made subtitle files tend to truly align there (issue #10175).
    /// </summary>
    private const double SyncCandidateGapMs = 3000;

    /// <summary>
    /// Silence thresholds for the gap badge's strength levels - the longer the silence, the
    /// stronger the badge, so the long gaps worth syncing around stand out (issue #15695).
    /// </summary>
    internal static readonly double[] SyncCandidateLevelMs = { SyncCandidateGapMs, 6000, 12000, 25000 };

    // Levels 1-2 are translucent tints over the row (the theme's text color reads on both light
    // and dark), levels 3-4 solid greens with white text so the strongest candidates pop out.
    // "Stronger" means darker on a light background but brighter on a dark one.
    private static readonly IBrush[] BadgeBackgroundsLight =
    {
        Brushes.Transparent,
        new ImmutableSolidColorBrush(Color.FromRgb(76, 175, 80), 0.22),
        new ImmutableSolidColorBrush(Color.FromRgb(76, 175, 80), 0.48),
        new ImmutableSolidColorBrush(Color.FromRgb(56, 142, 60)),
        new ImmutableSolidColorBrush(Color.FromRgb(27, 94, 32)),
    };

    private static readonly IBrush[] BadgeBackgroundsDark =
    {
        Brushes.Transparent,
        new ImmutableSolidColorBrush(Color.FromRgb(76, 175, 80), 0.22),
        new ImmutableSolidColorBrush(Color.FromRgb(76, 175, 80), 0.45),
        new ImmutableSolidColorBrush(Color.FromRgb(46, 125, 50)),
        new ImmutableSolidColorBrush(Color.FromRgb(67, 176, 71)),
    };

    private static readonly IBrush[] BadgeBorders =
    {
        Brushes.Transparent,
        new ImmutableSolidColorBrush(Color.FromRgb(76, 175, 80), 0.45),
        new ImmutableSolidColorBrush(Color.FromRgb(76, 175, 80), 0.75),
        Brushes.Transparent,
        Brushes.Transparent,
    };

    /// <summary>
    /// 0 = no badge, 1-4 = badge strength, by the silence before the line.
    /// </summary>
    internal static int GetSyncCandidateLevel(double gapMs)
    {
        if (double.IsNaN(gapMs) || gapMs == double.MaxValue)
        {
            return 0;
        }

        var level = 0;
        while (level < SyncCandidateLevelMs.Length && gapMs >= SyncCandidateLevelMs[level])
        {
            level++;
        }

        return level;
    }

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
    /// Explains the gap badges - shown once, under the grids, instead of a tooltip on every
    /// badge: one sample badge per strength level, then the explanation.
    /// </summary>
    private static Control MakeGapLegend()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(0, 8, 0, 0),
        };

        for (var i = 0; i < SyncCandidateLevelMs.Length; i++)
        {
            var seconds = (int)(SyncCandidateLevelMs[i] / 1000.0);
            var label = i == SyncCandidateLevelMs.Length - 1 ? $"{seconds}s+" : $"{seconds}s";
            panel.Children.Add(MakeBadge(i + 1, new TextBlock { Text = label, FontSize = 11 }));
        }

        panel.Children.Add(new TextBlock
        {
            Text = string.Format(Se.Language.Sync.SyncPointCandidateAfterInfo, (int)(SyncCandidateGapMs / 1000.0)),
            Opacity = 0.75,
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        });

        return panel;
    }

    private static Border MakeBadge(int level, TextBlock text)
    {
        var badge = new Border
        {
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(7, 1),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = text,
        };

        // Tag holds the current level, so a theme switch re-colors cells at their own level.
        badge.ActualThemeVariantChanged += (_, _) => ApplyBadgeLevel(badge, badge.Tag is int current ? current : level);
        ApplyBadgeLevel(badge, level);
        return badge;
    }

    private static void ApplyBadgeLevel(Border badge, int level)
    {
        badge.Tag = level;
        var dark = badge.ActualThemeVariant == ThemeVariant.Dark;
        badge.Background = (dark ? BadgeBackgroundsDark : BadgeBackgroundsLight)[level];
        badge.BorderBrush = BadgeBorders[level];

        if (badge.Child is not TextBlock text)
        {
            return;
        }

        if (level >= 3)
        {
            text.Foreground = Brushes.White;
        }
        else
        {
            text.ClearValue(TextBlock.ForegroundProperty);
        }

        text.FontWeight = level >= 4 ? FontWeight.Bold : level >= 3 ? FontWeight.SemiBold : FontWeight.Normal;
    }

    /// <summary>
    /// The gap after a line (same as the main grid's "Gap"), shown as a badge when big enough
    /// to mark the line as a sync point candidate (issues #10175, #15695).
    /// </summary>
    private static SeTableViewColumn MakeGapColumn()
    {
        var gapConverter = new DoubleToDisplayShortConverter();

        return new SeTableViewColumn
        {
            Header = Se.Language.Sync.GapAfter,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(90),
            NameBinding = new Binding(nameof(SubtitleLineViewModel.Gap)) { Converter = gapConverter, Mode = BindingMode.OneWay },
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
            {
                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Gap)) { Converter = gapConverter, Mode = BindingMode.OneWay },
                };

                // Same padding and border for every level, so badge and plain values line up.
                var badge = MakeBadge(0, textBlock);
                badge.HorizontalAlignment = HorizontalAlignment.Left;
                badge.Margin = new Thickness(-4, 0, 0, 0);

                var cell = new Border { Padding = new Thickness(4, 2), Child = badge };
                cell.DataContextChanged += (_, _) => UpdateBadge();
                textBlock.PropertyChanged += (_, e) =>
                {
                    if (e.Property == TextBlock.TextProperty)
                    {
                        UpdateBadge();
                    }
                };

                void UpdateBadge()
                {
                    var level = cell.DataContext is SubtitleLineViewModel line ? GetSyncCandidateLevel(line.Gap) : 0;
                    ApplyBadgeLevel(badge, level);
                }

                return cell;
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
        dataGrid.WithAccessibleName(Se.Language.Sync.SyncPoints);
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
                NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)));
        dataGrid.WithLabeledBy(labelFileName); // the file name above the list is its heading (#12087)
        TableViewExtras.AttachListNavigation(dataGrid);

        // Bring the initially selected line (the main window's selection) into view.
        dataGrid.Loaded += (_, _) =>
        {
            if (vm.SelectedSubtitle is { } selected)
            {
                dataGrid.ScrollIntoView(selected);
                TableViewExtras.CenterRow(dataGrid, selected);
            }
        };

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
                NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGridSubtitle.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedOtherSubtitle)));
        dataGridSubtitle.WithLabeledBy(labelOtherFileName); // the file name above the list is its heading (#12087)
        TableViewExtras.AttachListNavigation(dataGridSubtitle);

        // Clicking a line in the left grid scrolls this grid to the matching time (#12529)
        // without touching its selection.
        vm.ScrollOtherToLine = line => dataGridSubtitle.ScrollIntoView(line);

        grid.Add(panelOtherHeader, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle), 1);

        return grid;
    }

}