using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

/// <summary>
/// Cut video, in the editor look (see <see cref="EditorChrome"/>): output, transition and fade
/// settings on the left; the video, the waveform and the segment list on the right; progress and
/// Generate at the bottom.
/// </summary>
public class CutVideoWindow : Window
{
    private const double LabelWidth = 84;

    public CutVideoWindow(CutVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.CutVideoTitle;
        CanResize = true;
        Width = 1100;
        Height = 800;
        MinWidth = 860;
        MinHeight = 600;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();
        vm.Window = this;
        DataContext = vm;

        // The settings column scrolls on its own and the video gives up height first, so the
        // progress bar and the button bar are always reachable - even on a short screen.
        var settingsScroller = new ScrollViewer
        {
            Content = MakeSettingsPanel(vm),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var left = new Border
        {
            Child = settingsScroller,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
        };

        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(3, GridUnitType.Star)) { MinHeight = 120 }, // video player
                new RowDefinition(GridLength.Auto), // audio visualizer
                new RowDefinition(new GridLength(2, GridUnitType.Star)) { MinHeight = 150 }, // segments
            },
        };
        right.Add(MakeVideoPlayerView(vm), 0, 0);
        right.Add(MakeAudioVisualizerView(vm), 1, 0);
        right.Add(MakeSegmentsView(vm), 2, 0);

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(320)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        content.Add(left, 0, 0);
        content.Add(right, 0, 1);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto), // progress, while generating or previewing
                new RowDefinition(GridLength.Auto), // source info, buttons
            },
        };
        root.Add(content, 0, 0);
        root.Add(MakeProgressView(vm), 1, 0);
        root.Add(MakeButtonBar(vm), 2, 0);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            // initial focus on an input, not an action button - a focused button clicks on bare Space
            if (vm.SegmentGrid != null)
            {
                TableViewExtras.FocusRow(vm.SegmentGrid);
            }
        });
        Loaded += (s, e) => vm.OnLoaded();
        Closing += (s, e) => vm.OnClosing();
        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        AddHandler(KeyUpEvent, vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
    }

    /// <summary>
    /// Compact combo boxes and number fields, and the accent Import/Generate split buttons.
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

        // A disabled Generate (while generating) must not keep the accent - it looked clickable
        Styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(Avalonia.Controls.Presenters.ContentPresenter.ForegroundProperty, EditorChrome.FaintTextBrush),
            },
        });

        foreach (var state in new[] { string.Empty, ":pointerover" })
        {
            var hover = state.Length > 0;
            Styles.Add(new Style(x =>
            {
                var selector = x.OfType<SplitButton>().Class("primary").Template().OfType<Button>();
                return hover ? selector.Class(":pointerover").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>() : selector.Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>();
            })
            {
                Setters =
                {
                    new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, hover ? EditorChrome.AccentHoverBrush : EditorChrome.AccentBrush),
                    new Setter(Avalonia.Controls.Presenters.ContentPresenter.ForegroundProperty, Brushes.White),
                },
            });
        }
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

    private static ComboBox Combo<T>(System.Collections.ObjectModel.ObservableCollection<T> items, CutVideoViewModel vm, string property, string name)
    {
        var comboBox = UiUtil.MakeComboBox(items, vm, property);
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBox, name);
        return comboBox;
    }

    /// <summary>
    /// Seconds with two decimals; a cleared field falls back to the default.
    /// </summary>
    private static NumericUpDown Seconds(CutVideoViewModel vm, string property, decimal max, decimal defaultValue, string name)
    {
        var field = UiUtil.MakeNumericUpDownTwoDecimals(0.05m, max, 92, vm, property, defaultValue: defaultValue);
        field.Classes.Add("field");
        field.InnerLeftContent = new TextBlock
        {
            Text = "s",
            Foreground = EditorChrome.FaintTextBrush,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };
        AutomationProperties.SetName(field, name);
        return field;
    }

    private static StackPanel MakeSettingsPanel(CutVideoViewModel vm)
    {
        var panel = new StackPanel();
        panel.Children.Add(MakeOutputSection(vm));
        panel.Children.Add(MakeTransitionsSection(vm));
        panel.Children.Add(MakeFadesSection(vm));
        return panel;
    }

    private static Border MakeOutputSection(CutVideoViewModel vm)
    {
        var cutType = Combo(vm.CutTypes, vm, nameof(vm.SelectedCutType), Se.Language.Video.CutVideoCutType);
        var extension = Combo(vm.VideoExtensions, vm, nameof(vm.SelectedVideoExtension), Se.Language.General.VideoExtension);
        var encoding = Row(Se.Language.General.Encoding, Combo(vm.VideoEncodings, vm, nameof(vm.SelectedVideoEncoding), Se.Language.General.Encoding));
        encoding.Bind(IsVisibleProperty, new Binding(nameof(vm.IsVideoEncodingVisible)));
        var cutSubtitle = EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(nameof(vm.CutSubtitleToo), Se.Language.Video.CutVideoAlsoCutSubtitle), Se.Language.Video.CutVideoAlsoCutSubtitle);
        cutSubtitle.Bind(IsVisibleProperty, new Binding(nameof(vm.IsCutSubtitleVisible)));

        var rows = new StackPanel { Spacing = 7 };
        rows.Children.Add(Row(Se.Language.Video.CutVideoCutType, cutType));
        rows.Children.Add(Row(Se.Language.General.Format, extension));
        rows.Children.Add(encoding);
        rows.Children.Add(cutSubtitle);
        return EditorChrome.MakeEditorSection(Se.Language.General.Output, rows, first: true);
    }

    private static Border MakeTransitionsSection(CutVideoViewModel vm)
    {
        var transition = Combo(vm.Transitions, vm, nameof(vm.SelectedTransition), Se.Language.Video.CutVideoTransition);
        var duration = Seconds(vm, nameof(vm.TransitionDuration), 10m, 0.5m, Se.Language.Video.CutVideoTransitionDurationSeconds);

        var preview = new Button
        {
            Content = EditorChrome.MakeInlineRow(
                new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-play", FontSize = 11, VerticalAlignment = VerticalAlignment.Center },
                new TextBlock { Text = Se.Language.Video.CutVideoPreviewTransition, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center }),
            Command = vm.PreviewTransitionCommand,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!Button.IsEnabledProperty] = new Binding(nameof(vm.IsPreviewTransitionEnabled)),
        };
        AutomationProperties.SetName(preview, Se.Language.Video.CutVideoPreviewTransition);

        var info = EditorChrome.MakeBoundHint(nameof(vm.TransitionInfo));
        info.TextWrapping = TextWrapping.Wrap;
        info.TextTrimming = TextTrimming.None;

        // The transition settings only show while transitions are on
        var settings = new StackPanel { Spacing = 7 };
        settings.Children.Add(Row(Se.Language.General.Type, transition));
        settings.Children.Add(Row(Se.Language.General.Duration, duration));
        settings.Children.Add(preview);
        settings.Children.Add(info);
        settings.Bind(IsVisibleProperty, new Binding(nameof(vm.TransitionEnabled)));

        var rows = new StackPanel { Spacing = 9 };
        rows.Children.Add(EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(nameof(vm.TransitionEnabled), Se.Language.Video.CutVideoTransition), Se.Language.Video.CutVideoTransition));
        rows.Children.Add(settings);
        return EditorChrome.MakeEditorSection(Se.Language.Video.CutVideoTransitions, rows);
    }

    /// <summary>
    /// Fade from black at the very start of the output and to black at its very end - its own
    /// section, as it has nothing to do with the joins.
    /// </summary>
    private static Border MakeFadesSection(CutVideoViewModel vm)
    {
        Grid FadeRow(string enabledProperty, string durationProperty, string text)
        {
            var field = Seconds(vm, durationProperty, 60m, 1m, text);
            field.Bind(IsEnabledProperty, new Binding(enabledProperty));
            var row = new Grid
            {
                ColumnSpacing = 8,
                ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
            };
            row.Add(EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(enabledProperty, text), text), 0, 0);
            row.Add(field, 0, 1);
            return row;
        }

        var rows = new StackPanel { Spacing = 9 };
        rows.Children.Add(FadeRow(nameof(vm.FadeInEnabled), nameof(vm.FadeInDuration), Se.Language.Video.CutVideoFadeInSeconds));
        rows.Children.Add(FadeRow(nameof(vm.FadeOutEnabled), nameof(vm.FadeOutDuration), Se.Language.Video.CutVideoFadeOutSeconds));
        return EditorChrome.MakeEditorSection(Se.Language.General.Fade, rows);
    }

    private static Border MakeVideoPlayerView(CutVideoViewModel vm)
    {
        vm.VideoPlayer = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayer.FullScreenIsVisible = false;
        vm.VideoPlayer.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.VideoPlayer.VerticalAlignment = VerticalAlignment.Stretch;
        AutomationProperties.SetName(vm.VideoPlayer, Se.Language.General.Preview);

        // A plain border: the native video surface on macOS disappears under a rounded, clipping one
        return new Border
        {
            Child = vm.VideoPlayer,
            Margin = new Thickness(14, 12, 14, 8),
        };
    }

    private static Border MakeAudioVisualizerView(CutVideoViewModel vm)
    {
        vm.AudioVisualizer = new AudioVisualizer { Height = 80, Width = double.NaN, IsReadOnly = false };
        vm.AudioVisualizer.OnVideoPositionChanged += vm.AudioVisualizerPositionChanged;
        vm.AudioVisualizer.OnNewSelectionInsert += vm.AudioVisualizerOnNewSelectionInsert;
        vm.AudioVisualizer.DrawGridLines = Se.Settings.Waveform.DrawGridLines;
        vm.AudioVisualizer.WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
        vm.AudioVisualizer.WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
        vm.AudioVisualizer.WaveformGridColor = Se.Settings.Waveform.WaveformGridColor.FromHexToColor();
        vm.AudioVisualizer.OnSelectRequested += vm.AudioVisualizerSelectRequested;
        vm.AudioVisualizer.OnPrimarySingleClicked += vm.AudioVisualizerOnPrimarySingleClicked;
        vm.AudioVisualizer.OnPrimaryDoubleClicked += vm.AudioVisualizerOnPrimaryDoubleClicked;
        AutomationProperties.SetName(vm.AudioVisualizer, Se.Language.General.Waveform);

        return new Border
        {
            Child = vm.AudioVisualizer,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(14, 0, 14, 12),
        };
    }

    private static Border MakeSegmentsView(CutVideoViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // No header sorting: the segment list is kept in time order (segments are
        // inserted in position and feed the cut output), so reordering it would be wrong.
        var dataGridSubtitle = TableViewExtras.MakeTableView();
        dataGridSubtitle.Width = double.NaN;
        dataGridSubtitle.Height = double.NaN;
        dataGridSubtitle.DataContext = vm;
        dataGridSubtitle.ItemsSource = vm.Segments;
        AutomationProperties.SetName(dataGridSubtitle, Se.Language.Video.CutVideoSegments);
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new GridLength(60), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120), // was content-sized (Auto) on the DataGrid
        });
        dataGridSubtitle.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGridSubtitle.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSegment)) { Source = vm });
        dataGridSubtitle.SelectionChanged += vm.SegmentsGridChanged;
        dataGridSubtitle.DoubleTapped += vm.SegmentsGridDoubleTapped;
        vm.SegmentGrid = dataGridSubtitle;

        Button Action(string icon, string name, System.Windows.Input.ICommand command, string? isEnabledProperty = null)
        {
            var button = EditorChrome.CreateToolButton(icon, name, command, 28);
            if (isEnabledProperty != null)
            {
                button.Bind(IsEnabledProperty, new Binding(isEnabledProperty));
            }

            return button;
        }

        var buttonImport = new SeSplitButton
        {
            Content = Se.Language.General.ImportDotDotDot,
            Command = vm.ImportCommand,
            Height = 28,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.ImportCurrentSubtitle,
                        Command = vm.ImportCurrentCommand,
                    },
                }
            }
        };
        AutomationProperties.SetName(buttonImport, Se.Language.General.ImportDotDotDot);

        var actions = EditorChrome.MakeInlineRow(
            Action("fa-solid fa-plus", Se.Language.General.Add, vm.AddCommand),
            Action("fa-solid fa-arrow-right-to-bracket", Se.Language.General.SetStart, vm.SetStartCommand, nameof(vm.IsSetStartEnabled)),
            Action("fa-solid fa-arrow-right-from-bracket", Se.Language.General.SetEnd, vm.SetEndCommand, nameof(vm.IsSetEndEnabled)),
            Action("fa-solid fa-trash", Se.Language.General.Delete, vm.DeleteCommand, nameof(vm.IsDeleteEnabled)),
            EditorChrome.MakeToolbarSeparator(),
            buttonImport);
        actions.Spacing = 2;

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.Video.CutVideoSegments));

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(header, 0, 0);
        grid.Add(dataGridSubtitle, 1, 0);

        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 10, 14, 12),
        };
    }

    private static Border MakeProgressView(CutVideoViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var statusText = EditorChrome.MakeBoundHint(nameof(vm.ProgressText));
        statusText.FontSize = 12;

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 1);

        // Shown while generating, and while a transition preview is made (no progress bar then)
        return new Border
        {
            Child = grid,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
            MinHeight = 38,
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsStatusTextVisible)),
        };
    }

    private static Border MakeButtonBar(CutVideoViewModel vm)
    {
        // The source video
        var fileName = UiUtil.MakeFilePathLabel(vm, nameof(vm.VideoFileName));
        fileName.Foreground = EditorChrome.DimTextBrush;
        fileName.FontSize = 12;
        fileName.VerticalAlignment = VerticalAlignment.Center;
        var source = EditorChrome.MakeInlineRow(
            new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-file-video", FontSize = 12, Foreground = EditorChrome.FaintTextBrush, VerticalAlignment = VerticalAlignment.Center },
            fileName,
            EditorChrome.MakeBoundHint(nameof(vm.VideoFileSize)));
        source.Bind(IsVisibleProperty, new Binding(nameof(vm.VideoFileName)) { Converter = Avalonia.Data.Converters.StringConverters.IsNotNullOrEmpty });

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);
        buttonGenerate.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = EditorChrome.MakeInlineRow(buttonCancel, buttonGenerate);
        buttons.Spacing = 10;

        var bar = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Right);
        bar.Children.Add(buttons);
        bar.Children.Add(source);

        return EditorChrome.WrapButtonBar(bar);
    }
}
