using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using System;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync;
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
        Width = 1200;
        Height = 640;
        MinWidth = 900;
        MinHeight = 480;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        DataContext = vm;

        var subtitleViewView = MakeSubtitleView(vm);
        var controlView = MakeControlView(vm);
        var subtitleOtherView = MakeSubtitleOtherView(vm);

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindIsEnabled(nameof(vm.IsOkEnabled));
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(MakeGapLegend(), buttonApply, buttonCancel, buttonOk);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        // Bitmaps scale down to the column width, so an image based other subtitle gets the
        // bigger part of a wider window to stay readable (#15911).
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(vm.IsOtherImageBased))
            {
                return;
            }

            content.ColumnDefinitions[2].Width = new GridLength(vm.IsOtherImageBased ? 1.5 : 1, GridUnitType.Star);
            if (vm.IsOtherImageBased && WindowState == WindowState.Normal && Width < ImageModeMinWindowWidth)
            {
                var maxWidth = Screens.ScreenFromWindow(this)?.WorkingArea.Width / (Screens.ScreenFromWindow(this)?.Scaling ?? 1);
                Width = maxWidth is > 0 ? Math.Min(ImageModeMinWindowWidth, maxWidth.Value) : ImageModeMinWindowWidth;
            }
        };

        content.Add(subtitleViewView, 0, 0);
        content.Add(controlView, 0, 1);
        content.Add(subtitleOtherView, 0, 2);

        // The lists give up height first, so the button bar stays reachable on a short screen.
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(content, 0, 0);
        grid.Add(panelButtons, 1, 0);

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
    /// Explains the gap badges - shown once, in the button bar under the grids, instead of a tooltip on every
    /// badge: one sample badge per strength level, then the explanation.
    /// </summary>
    private static Control MakeGapLegend()
    {
        var badges = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
        };

        for (var i = 0; i < SyncCandidateLevelMs.Length; i++)
        {
            var seconds = (int)(SyncCandidateLevelMs[i] / 1000.0);
            var label = i == SyncCandidateLevelMs.Length - 1 ? $"{seconds}s+" : $"{seconds}s";
            badges.Children.Add(MakeBadge(i + 1, new TextBlock { Text = label, FontSize = EditorChrome.ScaledFont(11) }));
        }

        // Badges first, then the explanation, which wraps in what is left of the button bar.
        var panel = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        panel.Add(badges, 0, 0);
        panel.Add(new TextBlock
        {
            Text = string.Format(Se.Language.Sync.SyncPointCandidateAfterInfo, (int)(SyncCandidateGapMs / 1000.0)),
            FontSize = EditorChrome.ScaledFont(11.5),
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        }, 0, 1);

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

    private const double ImageModeMinWindowWidth = 1400;
    private static SeTableViewColumn MakeImageColumn(PointSyncViaOtherViewModel vm)
    {
        // One brush for every image cell, as in the OCR window.
        var previewBackground = ImagePreviewBackground.CreateBrush();

        return new SeTableViewColumn
        {
            Header = Se.Language.General.Image,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
            {
                var image = new Image
                {
                    Stretch = Stretch.Uniform,
                };
                // Ctrl+Plus/Minus zoom, like the OCR window.
                image.Bind(Image.MaxWidthProperty, new Binding(nameof(vm.ImageMaxWidth)) { Source = vm });
                image.Bind(Image.MaxHeightProperty, new Binding(nameof(vm.ImageMaxHeight)) { Source = vm });
                var container = new Border
                {
                    Background = previewBackground,
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(3),
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Child = image,
                };

                // Cells are recycled, so the bitmap follows the data context. Each cell owns its
                // bitmap: decoded fresh, and the one it showed before is freed.
                container.DataContextChanged += (_, _) =>
                {
                    var previous = image.Source as IDisposable;
                    image.Source = container.DataContext is PointSyncOtherImageLine line
                        ? line.ImageItem.GetBitmapCroppedUncached()
                        : null;
                    previous?.Dispose();
                    container.IsVisible = image.Source != null;
                };

                return container;
            }),
        };
    }

    private static Control MakeControlView(PointSyncViaOtherViewModel vm)
    {
        var syncPointList = PointSyncChrome.MakeSyncPointList(vm, vm.SyncPoints, nameof(vm.SelectedSyncPoint),
            vm.DeleteSelectedPointSyncCommand, () => vm.SelectedSyncPoint != null);

        var buttonSetSyncPoint = PointSyncChrome.MakeWideAction(IconNames.ArrowLeftRightBold, Se.Language.Sync.SetSyncPoint, vm.SetSyncPointCommand);

        // The other subtitle is the usual source for a sync point here, but a line it does not
        // cover still has to be pinnable - so allow picking the time off the video (issue #13341).
        var buttonSetSyncPointViaVideo = PointSyncChrome.MakeWideAction(IconNames.MovieOpenOutline, Se.Language.Sync.SetSyncPointViaVideo, vm.SetSyncPointViaVideoCommand);

        return PointSyncChrome.MakeSidePanel(syncPointList, $"{nameof(vm.SyncPoints)}.Count", new Thickness(1, 0, 1, 0),
            buttonSetSyncPoint, buttonSetSyncPointViaVideo);
    }

    private static Grid MakeSubtitleView(PointSyncViaOtherViewModel vm)
    {
        // TextBlock in a star column so a long file name shrinks with an ellipsis
        // instead of pushing under the "Find text" button.
        var labelFileName = PointSyncChrome.MakeFileNameLabel(vm, nameof(vm.FileName));
        var buttonFindText = EditorChrome.MakeActionButton(IconNames.Find, Se.Language.Sync.FindText, vm.FindTextLeftCommand);
        var fileNamePanel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        fileNamePanel.Add(PointSyncChrome.MakeFileIcon(), 0, 0);
        fileNamePanel.Add(labelFileName, 0, 1);
        var panelHeader = PointSyncChrome.MakeHeaderRow(fileNamePanel, buttonFindText);

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

        // Ctrl+G goes to a line in the grid used last.
        dataGrid.GotFocus += (_, _) => vm.IsOtherGridActive = false;
        vm.GoToLeftLine = line =>
        {
            dataGrid.ScrollIntoView(line);
            TableViewExtras.CenterRow(dataGrid, line);
            TableViewExtras.FocusRow(dataGrid);
        };

        return PointSyncChrome.MakeListPane(panelHeader, EditorChrome.MakeListFrame(dataGrid));
    }

    private static Grid MakeSubtitleOtherView(PointSyncViaOtherViewModel vm)
    {
        var buttonBrowseOther = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.OpenSubtitleFileTitle, vm.BrowseOtherCommand, 28);
        // TextBlock in a star column so a long file name shrinks with an ellipsis
        // instead of pushing under the "Find text" button.
        var labelOtherFileName = PointSyncChrome.MakeFileNameLabel(vm, nameof(vm.FileNameOther));

        // Until the other subtitle is picked, say what the folder button is for.
        var labelOpenHint = new TextBlock
        {
            Text = Se.Language.General.OpenSubtitleFileTitle,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            IsHitTestVisible = false,
        };
        labelOpenHint.Bind(IsVisibleProperty, new Binding(nameof(vm.FileNameOther)) { Source = vm, Converter = StringConverters.IsNullOrEmpty });

        var panelOtherBrowse = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panelOtherBrowse.Add(buttonBrowseOther, 0, 0);
        panelOtherBrowse.Add(new Panel { Children = { labelOtherFileName, labelOpenHint } }, 0, 1);
        var buttonFindTextOther = EditorChrome.MakeActionButton(IconNames.Find, Se.Language.Sync.FindText, vm.FindTextOtherCommand)
            .WithBindIsEnabled(nameof(vm.IsOtherTextBased));
        var panelOtherHeader = PointSyncChrome.MakeHeaderRow(panelOtherBrowse, buttonFindTextOther);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var dataGridSubtitle = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Othersubtitles;

        var textColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
            NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        textColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.IsOtherTextBased)) { Source = vm, Mode = BindingMode.OneWay });

        // Image based other subtitle (sup, PGS/VobSub track in mkv/mp4): the bitmaps, shown like
        // the OCR window shows them (#15911).
        var imageColumn = MakeImageColumn(vm);
        imageColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.IsOtherImageBased)) { Source = vm, Mode = BindingMode.OneWay });

        // A column with a bound visibility needs every column to go through a manager.
        var columnManager = new TableViewColumnManager(dataGridSubtitle);
        foreach (var column in new TableViewColumn[]
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
                     textColumn,
                     imageColumn,
                 })
        {
            columnManager.Add(column);
        }

        dataGridSubtitle.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedOtherSubtitle)));
        dataGridSubtitle.WithLabeledBy(labelOtherFileName); // the file name above the list is its heading (#12087)
        TableViewExtras.AttachListNavigation(dataGridSubtitle);

        // Clicking a line in the left grid scrolls this grid to the matching time (#12529)
        // without touching its selection.
        vm.ScrollOtherToLine = line => dataGridSubtitle.ScrollIntoView(line);
        dataGridSubtitle.GotFocus += (_, _) => vm.IsOtherGridActive = true;
        vm.GoToOtherLine = line =>
        {
            dataGridSubtitle.ScrollIntoView(line);
            TableViewExtras.CenterRow(dataGridSubtitle, line);
            TableViewExtras.FocusRow(dataGridSubtitle);
        };

        return PointSyncChrome.MakeListPane(panelOtherHeader, EditorChrome.MakeListFrame(dataGridSubtitle));
    }

}