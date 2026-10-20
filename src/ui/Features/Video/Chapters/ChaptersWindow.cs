using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Video.Chapters;

public class ChaptersWindow : Window
{
    private readonly ChaptersViewModel _vm;

    private const double LabelWidth = 84;

    public ChaptersWindow(ChaptersViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Chapters;
        CanResize = true;
        Width = 980;
        Height = 660;
        MinWidth = 840;
        MinHeight = 480;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // The side panel scrolls on its own and the list gives up height first, so the button bar
        // stays reachable on a short screen.
        var sideScroller = new ScrollViewer
        {
            Content = MakeSidePanel(vm),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var side = new Border
        {
            Child = sideScroller,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(370)),
            },
        };
        content.Add(MakeChapterListPanel(vm), 0, 0);
        content.Add(side, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        grid.Add(MakeToolbar(vm), 0, 0);
        grid.Add(content, 1, 0);
        grid.Add(MakeButtonBar(vm, buttonOk, buttonCancel), 2, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // hack to make OnKeyDown work
    }

    /// <summary>
    /// Compact combo boxes, text boxes and time code fields.
    /// </summary>
    private void AddStyles()
    {
        Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
        Styles.Add(new Style(x => x.OfType<TextBox>().Class("text"))
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 4)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
            },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
            },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<TextBox>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<ButtonSpinner>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });
    }

    /// <summary>
    /// Icon button with its label beside it, for the actions bar.
    /// </summary>
    private static Button MakeAction(string icon, string text, System.Windows.Input.ICommand command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(10, 0) };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Height = 30,
            Command = command,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        AutomationProperties.SetName(button, text);
        return button;
    }

    /// <summary>
    /// Import from the video or a file, and export in the chosen format.
    /// </summary>
    private static Border MakeToolbar(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var buttonImportVideo = MakeAction("fa-solid fa-film", language.ImportFromVideo, vm.ImportFromVideoCommand)
            .WithBindIsEnabled(nameof(vm.IsVideoLoaded));
        var buttonImportFile = MakeAction("fa-solid fa-file-import", language.ImportFromFile, vm.ImportFromFileCommand);

        // The format is chosen here rather than guessed from the file name: OGM and YouTube
        // chapters are both ".txt", so the extension cannot say which writer was meant.
        var comboExportFormat = UiUtil.MakeComboBox(vm.ExportFormats, vm, nameof(vm.SelectedExportFormat))
            .WithMinWidth(180);
        comboExportFormat.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboExportFormat, language.ExportToFile);

        var buttonExport = MakeAction("fa-solid fa-file-export", language.ExportToFile, vm.ExportToFileCommand)
            .WithBindIsEnabled(nameof(vm.HasChapters));

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(buttonImportVideo);
        panel.Children.Add(buttonImportFile);
        panel.Children.Add(EditorChrome.MakeToolbarSeparator());
        panel.Children.Add(comboExportFormat);
        panel.Children.Add(buttonExport);

        return new Border
        {
            Child = panel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    /// <summary>
    /// The video the chapters belong to on the left, Cancel and OK on the right.
    /// </summary>
    private static Border MakeButtonBar(ChaptersViewModel vm, Button buttonOk, Button buttonCancel)
    {
        var videoIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-file-video",
            FontSize = 12,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        };
        var videoName = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VideoFileNameDisplay)),
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(videoIcon, 0, 0);
        grid.Add(videoName, 0, 1);
        grid.Add(buttons, 0, 2);
        return EditorChrome.WrapButtonBar(grid);
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
                FontSize = UiUtil.ScaledFontSize(10),
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
    }

    /// <summary>
    /// The chapter list: a header with the count and the add/delete/clear actions, the grid under it.
    /// </summary>
    private static Grid MakeChapterListPanel(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var buttonAdd = EditorChrome.CreateToolButton("fa-solid fa-location-crosshairs", language.AddChapterAtVideoPosition, vm.AddChapterAtVideoPositionCommand, 26)
            .WithBindIsEnabled(nameof(vm.IsVideoLoaded));
        var buttonAddPlain = EditorChrome.CreateToolButton("fa-solid fa-plus", language.AddChapter, vm.AddChapterCommand, 26);
        var buttonDelete = EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Delete, vm.DeleteSelectedChapterCommand, 26)
            .WithBindIsEnabled(nameof(vm.HasSelectedChapter));
        var buttonClear = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Clear, vm.ClearChaptersCommand, 26)
            .WithBindIsEnabled(nameof(vm.HasChapters));

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        actions.Children.Add(buttonAdd);
        actions.Children.Add(buttonAddPlain);
        actions.Children.Add(EditorChrome.MakeToolbarSeparator());
        actions.Children.Add(buttonDelete);
        actions.Children.Add(buttonClear);

        var title = EditorChrome.MakeSectionHeader(Se.Language.General.Chapters);
        var badge = MakeCountBadge(new Binding(nameof(vm.ChapterCountDisplay)));
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(UiUtil.MakeHorizontalPanel(title, badge));

        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        panel.Add(header, 0);
        panel.Add(MakeChapterGrid(vm), 1);
        return panel;
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
            FontSize = 12.5,
            Foreground = EditorChrome.FaintTextBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false,
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.HasChapters)) { Converter = InverseBooleanConverter.Instance },
        };

        return new Border
        {
            Child = new Panel { Children = { tableView, emptyHint } },
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    private static StackPanel MakeSidePanel(ChaptersViewModel vm)
    {
        var panel = new StackPanel();
        panel.Children.Add(MakeSelectedChapterSection(vm));
        panel.Children.Add(MakeAdjustTimesSection(vm));
        panel.Children.Add(MakeWriteToVideoSection(vm));
        return panel;
    }

    /// <summary>
    /// A label of fixed width and the control(s) after it, so the rows line up.
    /// </summary>
    private static Grid Row(string label, Control content)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    private static Border MakeSelectedChapterSection(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var textBoxTitle = new TextBox
        {
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding($"{nameof(vm.SelectedChapter)}.{nameof(ChapterItem.Title)}")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            },
        };
        textBoxTitle.Classes.Add("text");
        textBoxTitle.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedChapter)));
        AutomationProperties.SetName(textBoxTitle, language.ChapterTitle);

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

        var buttonSetToVideoPosition = EditorChrome.CreateToolButton("fa-solid fa-location-crosshairs", language.AddChapterAtVideoPosition, vm.SetSelectedToVideoPositionCommand, 28);
        buttonSetToVideoPosition.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedChapter)));

        var buttonGoTo = EditorChrome.CreateToolButton("fa-solid fa-play", Se.Language.General.GoTo, vm.GoToSelectedChapterCommand, 28);
        buttonGoTo.Bind(IsEnabledProperty, new Binding(nameof(vm.HasSelectedChapter)));

        var panelTime = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Children = { timeCodeUpDown, buttonSetToVideoPosition, buttonGoTo } };

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(Row(language.ChapterTitle, textBoxTitle));
        rows.Children.Add(Row(language.StartTime, panelTime));
        return EditorChrome.MakeEditorSection(language.SelectedChapter, rows, first: true);
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

        var buttonShift = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyShiftCommand)
            .WithBindIsEnabled(nameof(vm.HasChapters));

        var comboFrom = UiUtil.MakeComboBox(vm.FromFrameRates, vm, nameof(vm.SelectedFromFrameRate)).WithFrameRateDisplay();
        var comboTo = UiUtil.MakeComboBox(vm.ToFrameRates, vm, nameof(vm.SelectedToFrameRate)).WithFrameRateDisplay();
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

        var buttonScale = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyFrameRateScaleCommand)
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
        frameRateGrid.Add(EditorChrome.MakeDimLabel(language.FromFrameRate, new Thickness(0)), 0, 0);
        frameRateGrid.Add(comboFrom, 0, 1);
        frameRateGrid.Add(EditorChrome.MakeDimLabel(language.ToFrameRate, new Thickness(0)), 0, 2);
        frameRateGrid.Add(comboTo, 0, 3);
        frameRateGrid.Add(buttonScale, 0, 4);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(MakeHint(language.ShiftAllTimesDescription));
        rows.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Children = { shiftUpDown, buttonShift } });
        rows.Children.Add(MakeHint(language.ScaleTimesDescription, new Thickness(0, 6, 0, 0)));
        rows.Children.Add(frameRateGrid);
        return EditorChrome.MakeEditorSection(language.AdjustTimes, rows);
    }

    private static Border MakeWriteToVideoSection(ChaptersViewModel vm)
    {
        var language = Se.Language.Video.Chapters;

        var buttonWrite = UiUtil.MakeButton(language.WriteToVideo, vm.WriteToVideoCommand)
            .WithIconLeft(IconNames.MovieOpenOutline)
            .WithBindIsEnabled(nameof(vm.CanWriteToVideo));
        buttonWrite.HorizontalAlignment = HorizontalAlignment.Left;

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(MakeHint(language.WriteToVideoDescription));
        rows.Children.Add(buttonWrite);
        return EditorChrome.MakeEditorSection(language.WriteToVideo.TrimEnd('.', '…'), rows);
    }

    private static TextBlock MakeHint(string text, Thickness? margin = null)
    {
        return new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 11.5,
            Margin = margin ?? new Thickness(0),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
