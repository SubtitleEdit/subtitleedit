using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Video.Chapters;

public class ChaptersWindow : Window
{
    private readonly ChaptersViewModel _vm;

    public ChaptersWindow(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = language.Title;
        CanResize = true;
        Width = 940;
        Height = 640;
        MinWidth = 820;
        MinHeight = 480;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(370, GridUnitType.Pixel) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 12,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeHeader(vm), 0, 0, 1, 2);
        grid.Add(MakeChapterListPanel(vm), 1, 0);
        grid.Add(MakeSidePanel(vm), 1, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
    }

    /// <summary>
    /// Title row: the chapter glyph, the heading, a count badge, and the video the chapters belong
    /// to on the right.
    /// </summary>
    private static Grid MakeHeader(ChaptersViewModel vm)
    {
        var glyph = MakeGlyph(IconNames.Bookmark, 15);

        var title = new TextBlock
        {
            Text = Se.Language.Video.Chapters.Chapters,
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };

        var badge = MakeCountBadge(new Binding(nameof(vm.ChapterCountDisplay)));

        var videoIcon = new ContentControl
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.7,
        };
        Optris.Icons.Avalonia.Attached.SetIcon(videoIcon, IconNames.MovieOpenOutline);

        var videoName = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VideoFileNameDisplay)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0),
            Opacity = 0.7,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var videoPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { videoIcon, videoName },
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        headerGrid.Add(UiUtil.MakeHorizontalPanel(glyph, title, badge), 0, 0);
        headerGrid.Add(videoPanel, 0, 1);
        return headerGrid;
    }

    /// <summary>
    /// A section icon in the normal text color - no colored tile behind it, so the dialog carries
    /// no accent of its own and follows the theme.
    /// </summary>
    internal static ContentControl MakeGlyph(string iconName, double fontSize)
    {
        var icon = new ContentControl
        {
            FontSize = fontSize,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        Optris.Icons.Avalonia.Attached.SetIcon(icon, iconName);
        return icon;
    }

    /// <summary>
    /// Count pill. Grey rather than a brand color: it reads the same in both themes and does not
    /// compete with the text next to it.
    /// </summary>
    internal static Border MakeCountBadge(Binding textBinding)
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
                [!TextBlock.TextProperty] = textBinding,
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    private static Grid MakeChapterListPanel(ChaptersViewModel vm)
    {
        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            RowSpacing = 8,
        };

        panel.Add(MakeChapterGrid(vm), 0);
        panel.Add(MakeListToolbar(vm), 1);
        return panel;
    }

    private static Panel MakeListToolbar(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var buttonAdd = UiUtil.MakeButton(vm.AddChapterAtVideoPositionCommand, IconNames.RayStart, language.AddChapterAtVideoPosition)
            .WithBindIsEnabled(nameof(vm.IsVideoLoaded));
        var buttonAddPlain = UiUtil.MakeButton(vm.AddChapterCommand, IconNames.Plus, language.AddChapter);
        var buttonDelete = UiUtil.MakeButton(vm.DeleteSelectedChapterCommand, IconNames.Trash, Se.Language.General.Delete)
            .WithBindIsEnabled(nameof(vm.HasSelectedChapter));
        var buttonClear = UiUtil.MakeButton(vm.ClearChaptersCommand, IconNames.Close, Se.Language.General.Clear)
            .WithBindIsEnabled(nameof(vm.HasChapters));

        var buttonImportVideo = UiUtil.MakeButton(vm.ImportFromVideoCommand, IconNames.MovieOpenOutline, language.ImportFromVideo)
            .WithBindIsEnabled(nameof(vm.IsVideoLoaded));
        var buttonImportFile = UiUtil.MakeButton(vm.ImportFromFileCommand, IconNames.Import, language.ImportFromFile);
        // The format is chosen here rather than guessed from the file name: OGM and YouTube
        // chapters are both ".txt", so the extension cannot say which writer was meant.
        var comboExportFormat = UiUtil.MakeComboBox(vm.ExportFormats, vm, nameof(vm.SelectedExportFormat))
            .WithMinWidth(180);
        AutomationProperties.SetName(comboExportFormat, language.ExportToFile);

        var buttonExport = UiUtil.MakeButton(vm.ExportToFileCommand, IconNames.Export, language.ExportToFile)
            .WithBindIsEnabled(nameof(vm.HasChapters));

        var leftGroup = UiUtil.MakeHorizontalPanel(buttonAdd, buttonAddPlain, buttonDelete, buttonClear);
        var rightGroup = UiUtil.MakeHorizontalPanel(buttonImportVideo, buttonImportFile, comboExportFormat, buttonExport);
        rightGroup.HorizontalAlignment = HorizontalAlignment.Right;

        var toolbar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        toolbar.Add(leftGroup, 0, 0);
        toolbar.Add(rightGroup, 0, 1);
        return toolbar;
    }

    private static Border MakeChapterGrid(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var tableView = TableViewExtras.MakeTableView(multiSelect: false);
        tableView.Height = double.NaN; // auto size inside scroll viewer
        tableView.Margin = new Thickness(2);
        tableView.ItemsSource = vm.Chapters;
        tableView.DataContext = vm;

        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ChapterItem.Number)),
            Width = new GridLength(55),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = language.StartTime,
            Binding = new Binding(nameof(ChapterItem.StartTimeDisplay)),
            Width = new GridLength(130),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = language.ChapterTitle,
            Binding = new Binding(nameof(ChapterItem.Title)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        TableViewExtras.BindSelectedItem(tableView, vm, nameof(vm.SelectedChapter));
        tableView.DoubleTapped += vm.OnChapterGridDoubleTapped;
        tableView.KeyDown += (s, e) => vm.GridKeyDown(e);
        tableView.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && tableView.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                if (target != null)
                {
                    tableView.SelectedItem = target;
                    tableView.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem
        {
            Header = Se.Language.General.GoTo,
            Command = vm.GoToSelectedChapterCommand,
        });
        flyout.Items.Add(new MenuItem
        {
            Header = Se.Language.General.Delete,
            Command = vm.DeleteSelectedChapterCommand,
        });
        tableView.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(tableView);

        // Shown in place of an empty grid so a first-time user is told what to do next.
        var emptyHint = new TextBlock
        {
            Text = language.EmptyListCallToAction,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 320,
            Opacity = 0.6,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false,
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.HasChapters)) { Converter = new InverseBooleanConverter() },
        };

        return UiUtil.MakeBorderForControl(new Panel { Children = { tableView, emptyHint } });
    }

    private static StackPanel MakeSidePanel(ChaptersViewModel vm)
    {
        var panel = UiUtil.MakeVerticalPanel(MakeSelectedChapterSection(vm), MakeAdjustTimesSection(vm), MakeWriteToVideoSection(vm));

        // MakeVerticalPanel centers its content, which floated the first box below the top of the
        // chapter list next to it. Top-align so both columns start on the same line.
        panel.VerticalAlignment = VerticalAlignment.Top;
        panel.HorizontalAlignment = HorizontalAlignment.Stretch;
        return panel;
    }

    private static Border MakeSectionTitle(string title, string iconName)
    {
        var glyph = MakeGlyph(iconName, 13);

        var label = new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };

        return new Border
        {
            Margin = new Thickness(0, 0, 0, 8),
            Child = UiUtil.MakeHorizontalPanel(glyph, label),
        };
    }

    private static Border MakeSelectedChapterSection(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var labelTitle = UiUtil.MakeLabel(language.ChapterTitle);
        var textBoxTitle = new TextBox
        {
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 8),
            [!TextBox.TextProperty] = new Binding($"{nameof(vm.SelectedChapter)}.{nameof(ChapterItem.Title)}")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            },
        };
        textBoxTitle.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedChapter)));
        AutomationProperties.SetName(textBoxTitle, language.ChapterTitle);

        var labelStart = UiUtil.MakeLabel(language.StartTime);
        var timeCodeUpDown = new TimeCodeUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TimeCodeUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedChapter)}.{nameof(ChapterItem.StartTimeSpan)}")
            {
                Mode = BindingMode.TwoWay,
            },
        };
        timeCodeUpDown.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedChapter)));
        AutomationProperties.SetName(timeCodeUpDown, language.StartTime);

        var buttonSetToVideoPosition = UiUtil.MakeButton(vm.SetSelectedToVideoPositionCommand, IconNames.RayStart, language.AddChapterAtVideoPosition);
        buttonSetToVideoPosition.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedChapter)));

        var buttonGoTo = UiUtil.MakeButton(vm.GoToSelectedChapterCommand, IconNames.Play, Se.Language.General.GoTo);
        buttonGoTo.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedChapter)));

        var panelTime = UiUtil.MakeHorizontalPanel(timeCodeUpDown, buttonSetToVideoPosition, buttonGoTo);

        var content = UiUtil.MakeVerticalPanel(
            MakeSectionTitle(language.SelectedChapter, IconNames.Pencil),
            labelTitle,
            textBoxTitle,
            labelStart,
            panelTime);

        return UiUtil.MakeBorderForControl(content);
    }

    private static Border MakeAdjustTimesSection(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var shiftUpDown = new TimeCodeUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.ShiftTime)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(shiftUpDown, language.ShiftAllTimes);

        var buttonShift = UiUtil.MakeButton(language.Apply, vm.ApplyShiftCommand)
            .WithBindIsEnabled(nameof(vm.HasChapters));

        var comboFrom = UiUtil.MakeComboBox(vm.FromFrameRates, vm, nameof(vm.SelectedFromFrameRate));
        var comboTo = UiUtil.MakeComboBox(vm.ToFrameRates, vm, nameof(vm.SelectedToFrameRate));
        AutomationProperties.SetName(comboFrom, language.FromFrameRate);
        AutomationProperties.SetName(comboTo, language.ToFrameRate);

        foreach (var combo in new[] { comboFrom, comboTo })
        {
            // Star-sized below, so the box takes the space that is left instead of demanding a
            // fixed width that pushes the button off the row. The minimum keeps a rate like
            // "23,976" readable rather than clipped to "23,9".
            combo.Width = double.NaN;
            combo.MinWidth = 84;
            combo.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        var buttonScale = UiUtil.MakeButton(language.Apply, vm.ApplyFrameRateScaleCommand)
            .WithBindIsEnabled(nameof(vm.HasChapters));

        // One row: the two labels and the button take what they need, and the drop-downs share
        // what remains, so a longer translated label shrinks the boxes rather than wrapping.
        var frameRateGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 6,
        };
        frameRateGrid.Add(UiUtil.MakeLabel(language.FromFrameRate), 0, 0);
        frameRateGrid.Add(comboFrom, 0, 1);
        frameRateGrid.Add(UiUtil.MakeLabel(language.ToFrameRate), 0, 2);
        frameRateGrid.Add(comboTo, 0, 3);
        frameRateGrid.Add(buttonScale, 0, 4);

        var content = UiUtil.MakeVerticalPanel(
            MakeSectionTitle(language.AdjustTimes, IconNames.ArrowLeftRightBold),
            MakeHint(language.ShiftAllTimesDescription),
            UiUtil.MakeHorizontalPanel(shiftUpDown, buttonShift),
            MakeHint(language.ScaleTimesDescription),
            frameRateGrid);

        return UiUtil.MakeBorderForControl(content);
    }

    private static Border MakeWriteToVideoSection(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var buttonWrite = UiUtil.MakeButton(language.WriteToVideo, vm.WriteToVideoCommand)
            .WithIconLeft(IconNames.MovieOpenOutline)
            .WithBindIsEnabled(nameof(vm.CanWriteToVideo));

        var content = UiUtil.MakeVerticalPanel(
            MakeSectionTitle(language.WriteToVideo, IconNames.Export),
            MakeHint(language.WriteToVideoDescription),
            buttonWrite);

        return UiUtil.MakeBorderForControl(content);
    }

    private static TextBlock MakeHint(string text)
    {
        return new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.65,
            FontSize = 11,
            Margin = new Thickness(0, 0, 0, 8),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
