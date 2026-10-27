using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Progress;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

public class ImproveTimeCodesWindow : Window
{
    // Original cues in the usual blue, aligned ones in green, and the line being looked at in
    // amber in both - so the eye can hop between the two waveforms and find the same cue.
    private static readonly Color OriginalTint = Color.FromArgb(140, 70, 110, 180);
    private static readonly Color AlignedTint = Color.FromArgb(140, 50, 160, 110);
    private static readonly Color CurrentTint = Color.FromArgb(210, 230, 160, 40);

    public ImproveTimeCodesWindow(ImproveTimeCodesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.ImproveTimeCodes.Title);
        CanResize = true;
        Width = 1150;
        Height = 760;
        MinWidth = 900;
        MinHeight = 560;
        // The Edit dialogs' styles: compact fields, a dimmed disabled primary (Align while aligning)
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        AddStyles(Styles);
        vm.Window = this;
        DataContext = vm;

        // The waveforms and the list give up height first (both have a minimum), so the progress
        // strip and the button bar stay reachable on a short screen.
        var middle = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(5, GridUnitType.Star)) { MinHeight = 160 }, // waveforms
                new RowDefinition(GridLength.Auto), // change navigation
                new RowDefinition(new GridLength(4, GridUnitType.Star)) { MinHeight = 140 }, // rows + video
            },
        };
        middle.Add(BuildVisualizerArea(vm), 0, 0);
        middle.Add(BuildChangeNavBar(vm), 1, 0);
        middle.Add(BuildRowsAndVideo(vm), 2, 0);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        grid.Add(BuildSetupBar(vm), 0, 0);
        grid.Add(EditorChrome.MakePanel(middle), 1, 0);
        grid.Add(BuildProgressStrip(vm), 2, 0);
        grid.Add(BuildButtonBar(vm), 3, 0);
        Content = grid;

        // Space is taken on the way down, so it plays/pauses wherever the focus is - the list
        // and the buttons would otherwise keep it for themselves. Everything else waits its turn,
        // so Escape still closes an open drop-down before it closes the window.
        AddHandler(KeyDownEvent, (_, e) => vm.OnSpaceKeyDown(e), RoutingStrategies.Tunnel);
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += (_, _) => vm.Dispose();
    }

    /// <summary>
    /// A switch row whose label fades with it while it is disabled.
    /// </summary>
    private static void AddStyles(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<Grid>().Class("switch-row").Class(":disabled"))
        {
            Setters = { new Setter(Visual.OpacityProperty, 0.5) },
        });
    }

    private static Grid SwitchRow(ImproveTimeCodesViewModel vm, string property, string text)
    {
        var toggle = EditorChrome.MakeSwitch(property, text);
        toggle.DataContext = vm;
        var row = EditorChrome.MakeSwitchRow(toggle, text);
        row.Classes.Add("switch-row");
        return row;
    }

    private static Control BuildSetupBar(ImproveTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;

        var engineDot = new Ellipse { Width = 10, Height = 10, VerticalAlignment = VerticalAlignment.Center };
        engineDot.Bind(Shape.FillProperty, new Binding(nameof(vm.EngineDotBrush)) { Source = vm });

        var engineStatus = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = EditorChrome.FieldFontSize,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.EngineStatus)) { Source = vm },
        };

        var engineSettingsName = $"{Se.Language.General.Engine} - {Se.Language.General.Settings}";
        var buttonEngineSettings = EditorChrome.CreateToolButton(IconNames.Settings, engineSettingsName, vm.ShowEngineSettingsCommand, 28)
            .BindIsEnabled(vm, nameof(vm.IsIdle));

        var comboAligner = new ComboBox
        {
            Width = 290,
            VerticalAlignment = VerticalAlignment.Center,
            ItemsSource = vm.Aligners,
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedAligner)) { Source = vm, Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<ForcedAlignerOption>((_, _) => new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding(nameof(ForcedAlignerOption.Display)),
            }),
        }.BindIsEnabled(vm, nameof(vm.IsIdle));
        AutomationProperties.SetName(comboAligner, l.Aligner);

        var numericMaxShift = UiUtil.MakeNumericUpDownOneDecimal(0.1m, 10m, 70, vm, nameof(vm.MaxShiftSeconds), defaultValue: 0.5m);
        numericMaxShift.Classes.Add("field"); // compact, no spinner - the arrow keys and wheel still step it
        AutomationProperties.SetName(numericMaxShift, l.MaxShift);

        var checkStart = SwitchRow(vm, nameof(vm.AdjustStart), l.AdjustStartTimes);
        var checkEnd = SwitchRow(vm, nameof(vm.AdjustEnd), l.AdjustEndTimes);
        var checkIsolate = SwitchRow(vm, nameof(vm.IsolateSpeech), l.IsolateSpeech);
        checkIsolate.Bind(IsEnabledProperty, new Binding(nameof(vm.IsIdle)) { Source = vm });
        var checkSpeechToText = SwitchRow(vm, nameof(vm.CheckWithSpeechToText), l.CheckWithSpeechToText);
        checkSpeechToText.Bind(IsEnabledProperty, new Binding(nameof(vm.CanCheckWithSpeechToText)) { Source = vm });

        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(comboAligner, l.AlignerHint);
            ToolTip.SetTip(numericMaxShift, l.MaxShiftHint);
            ToolTip.SetTip(checkIsolate, l.IsolateSpeechHint);
            checkSpeechToText.Bind(ToolTip.TipProperty, new Binding(nameof(vm.CheckWithSpeechToTextHint)) { Source = vm });
            ToolTip.SetShowOnDisabled(checkSpeechToText, true); // says why, for a language it cannot hear
        }

        // Engine and aligner settings on the first line, the on/off options on the second - the
        // wrap panels let them flow onto more lines in a narrow window.
        var settings = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Group(engineDot, engineStatus, buttonEngineSettings),
                Group(Label(l.Aligner), comboAligner),
                Group(Label(l.MaxShift), numericMaxShift),
            },
        };
        var options = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Group(checkStart),
                Group(checkEnd),
                Group(checkIsolate),
                Group(checkSpeechToText),
            },
        };

        return new Border
        {
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 6),
            Child = new StackPanel { Children = { settings, options } },
        };
    }

    private static TextBlock Label(string text) => EditorChrome.MakeDimLabel(text, new Thickness(0));

    private static StackPanel Group(params Control[] controls)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 4, 22, 4),
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.AddRange(controls);
        return panel;
    }

    private static Control BuildVisualizerArea(ImproveTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;

        var (labelOriginal, borderOriginal, avOriginal) = BuildLabeledVisualizer(Se.Language.General.Original, OriginalTint);
        var (labelAligned, borderAligned, avAligned) = BuildLabeledVisualizer(l.Aligned, AlignedTint);

        vm.AudioVisualizerOriginal = avOriginal;
        vm.AudioVisualizerAligned = avAligned;

        // Either waveform can be scrolled or zoomed; the other follows. Setting a property to
        // the value it already has raises nothing, so the two handlers do not ping-pong.
        Mirror(avOriginal, avAligned);
        Mirror(avAligned, avOriginal);
        ReloadParagraphsWhenViewMoves(avOriginal, vm);
        ReloadParagraphsWhenViewMoves(avAligned, vm);
        MakeEditable(avAligned, vm);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(labelAligned, l.AlignedWaveformHint);
        }

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Thickness(14, 4, 14, 10),
        };
        var checkSpeechOnly = SwitchRow(vm, nameof(vm.ShowSpeechOnly), l.ShowSpeechOnly);
        checkSpeechOnly.HorizontalAlignment = HorizontalAlignment.Right;
        checkSpeechOnly.Bind(IsEnabledProperty, new Binding(nameof(vm.IsSpeechOnlyAvailable)) { Source = vm });
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(checkSpeechOnly, l.ShowSpeechOnlyHint);
            ToolTip.SetShowOnDisabled(checkSpeechOnly, true); // the hint says how to make it available
        }

        grid.Add(labelOriginal, 0, 0);
        grid.Add(checkSpeechOnly, 2, 0); // on the Aligned label row - it is that waveform it changes
        grid.Add(borderOriginal, 1, 0);
        grid.Add(labelAligned, 2, 0);
        grid.Add(borderAligned, 3, 0);
        return grid;
    }

    private static void Mirror(AudioVisualizer source, AudioVisualizer target)
    {
        source.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name == nameof(AudioVisualizer.StartPositionSeconds))
            {
                target.StartPositionSeconds = source.StartPositionSeconds;
            }
            else if (e.Property.Name == nameof(AudioVisualizer.ZoomFactor))
            {
                target.ZoomFactor = source.ZoomFactor;
            }
            else if (e.Property.Name == nameof(AudioVisualizer.VerticalZoomFactor))
            {
                target.VerticalZoomFactor = source.VerticalZoomFactor;
            }
        };
    }

    // Each waveform reloads its own blocks: the mirrored one gets the same property change a
    // moment later and reloads itself then.
    private static void ReloadParagraphsWhenViewMoves(AudioVisualizer av, ImproveTimeCodesViewModel vm)
    {
        av.PropertyChanged += (_, e) =>
        {
            if (e.Property == AudioVisualizer.StartPositionSecondsProperty ||
                e.Property == AudioVisualizer.ZoomFactorProperty ||
                e.Property == BoundsProperty)
            {
                vm.ReloadWaveformParagraphs(av);
            }
        };
    }

    /// <summary>
    /// The aligned waveform takes the user's word over the aligner's: a line dragged or resized
    /// there gets those times. The original waveform stays read-only - it is the reference.
    /// </summary>
    private static void MakeEditable(AudioVisualizer av, ImproveTimeCodesViewModel vm)
    {
        av.IsReadOnly = false;
        av.MinGapSeconds = Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / 1000.0;
        av.OnDragStarted += (_, e) => vm.OnAlignedDragStarted(e.Paragraph);
        av.OnDragEnded += (_, _) => vm.OnAlignedDragEnded();

        var l = Se.Language.Tools.ImproveTimeCodes;
        var menuPlay = new Avalonia.Controls.MenuItem { Header = Se.Language.General.PlayLine, Command = vm.PlayRowCommand };
        var menuUndo = new Avalonia.Controls.MenuItem { Header = l.UndoAdjustment, Command = vm.UndoAdjustmentCommand };
        var flyout = new MenuFlyout();
        flyout.Items.Add(menuPlay);
        flyout.Items.Add(menuUndo);
        av.MenuFlyout = flyout;
        av.FlyoutMenuOpening += (_, e) =>
        {
            // A right-drag over empty waveform marks a selection; there is nothing here to do with one.
            av.NewSelectionParagraph = null;
            var row = vm.SelectRowAtAligned(e.PositionInSeconds);
            menuPlay.CommandParameter = row;
            menuUndo.CommandParameter = row;
            menuPlay.IsEnabled = row != null;
            menuUndo.IsEnabled = row is { IsAdjustedByHand: true };
        };
    }

    private static (Control label, Border border, AudioVisualizer av) BuildLabeledVisualizer(string title, Color tint)
    {
        var swatch = new Border
        {
            Width = 12,
            Height = 12,
            CornerRadius = new CornerRadius(3),
            Background = new SolidColorBrush(Color.FromArgb(255, tint.R, tint.G, tint.B)),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var label = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            Margin = new Thickness(0, 8, 5, 6),
            Background = Brushes.Transparent, // so the hint shows over the whole label
            Children =
            {
                swatch,
                EditorChrome.MakeSectionHeader(title),
            },
        };

        var av = new AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
            WaveformGridColor = Se.Settings.Waveform.WaveformGridColor.FromHexToColor(),
            ParagraphBackground = tint,
            ParagraphSelectedBackground = CurrentTint,
        };

        // A plain frame, like the video player's: no rounded, clipping border around a waveform.
        // The black is the waveform's own canvas, not window chrome.
        var border = new Border
        {
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
            Background = Brushes.Black,
            Child = av,
        };
        return (label, border, av);
    }

    private static Control BuildChangeNavBar(ImproveTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;

        var buttonPrev = EditorChrome.CreateToolButton(IconNames.ArrowUpThin, $"{Se.Language.General.PreviousChange} (F7)", vm.PreviousChangeCommand, 28);
        buttonPrev.Bind(IsEnabledProperty, new Binding(nameof(vm.CanGoPrevious)) { Source = vm });

        var buttonNext = EditorChrome.CreateToolButton(IconNames.ArrowDownThin, $"{Se.Language.General.NextChange} (F8)", vm.NextChangeCommand, 28);
        buttonNext.Bind(IsEnabledProperty, new Binding(nameof(vm.CanGoNext)) { Source = vm });

        var labelPosition = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            FontSize = EditorChrome.FieldFontSize,
            MinWidth = 120,
            TextAlignment = TextAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ChangePositionLabel)) { Source = vm },
        };

        var nav = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonPrev, labelPosition, buttonNext },
        };

        var buttonPlayPause = EditorChrome.CreateToolButton("mdi-play-pause", $"{l.PlayPause} (Space)", vm.TogglePlayPauseCommand, 28);
        var buttonPlayAligned = EditorChrome.MakeActionButton(IconNames.Play, l.PlayAligned, vm.PlaySelectedAlignedCommand);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonPlayAligned, $"{l.PlayAlignedHint} (F5)");
        }

        nav.Children.Add(EditorChrome.MakeToolbarSeparator());
        nav.Children.Add(buttonPlayPause);
        nav.Children.Add(buttonPlayAligned);

        var summary = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = EditorChrome.FieldFontSize,
            // With the speech-to-text counts it can be longer than the room beside the buttons.
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Right,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SummaryLine)) { Source = vm },
        };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), ColumnSpacing = 14 };
        grid.Add(nav, 0, 0);
        grid.Add(summary, 0, 1);

        return new Border
        {
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 1),
            Padding = new Thickness(10, 5, 14, 5),
            Child = grid,
        };
    }

    private static Control BuildRowsAndVideo(ImproveTimeCodesViewModel vm)
    {
        vm.VideoPlayer = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayer.FullScreenIsVisible = false;

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,340"), ColumnSpacing = 4, Margin = new Thickness(14, 10, 14, 12) };
        grid.Add(BuildRowsView(vm), 0, 0);
        grid.Add(new GridSplitter { Width = 4, ResizeDirection = GridResizeDirection.Columns, Background = Brushes.Transparent }, 0, 1);
        // A plain border: the native video surface on macOS disappears under a rounded, clipping one
        grid.Add(EditorChrome.MakeVideoFrame(vm.VideoPlayer), 0, 2);
        return grid;
    }

    private static Control BuildRowsView(ImproveTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;

        var tableView = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        tableView.Width = double.NaN;
        tableView.Height = double.NaN;
        tableView.DataContext = vm;
        tableView.ItemsSource = vm.Rows;
        tableView.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedRow)) { Source = vm, Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(tableView, Se.Language.General.Lines);

        vm.ScrollRowIntoView = row => tableView.ScrollIntoView(row);
        tableView.DoubleTapped += (_, _) => vm.PlaySelectedAlignedCommand.Execute(null);

        var applyColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Apply,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<ImproveTimeCodesRow>(nameof(ImproveTimeCodesRow.Apply), nameof(ImproveTimeCodesRow.Text),
                // Disabled rather than hidden: a hidden box collapses the row, and the rows
                // would all change height the moment the results arrive.
                configure: cb => cb[!IsEnabledProperty] = new Binding(nameof(ImproveTimeCodesRow.IsChanged))),
            Width = new GridLength(70),
        };

        var statusColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ImproveTimeCodesRow>((_, _) =>
            {
                var dot = new Ellipse { Width = 9, Height = 9, VerticalAlignment = VerticalAlignment.Center };
                dot.Bind(Shape.FillProperty, new Binding(nameof(ImproveTimeCodesRow.StatusBrush)));
                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 7,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        dot,
                        new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            [!TextBlock.TextProperty] = new Binding(nameof(ImproveTimeCodesRow.StatusText)),
                        },
                    },
                };
            }),
            Width = new GridLength(190),
        };

        // Only there once speech-to-text has been run.
        var heardColumn = MakeTextColumn(l.Heard, new Binding(nameof(ImproveTimeCodesRow.Heard)), new GridLength(60), TextAlignment.Right);
        heardColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.HasHeard)) { Source = vm, Mode = BindingMode.OneWay });

        // A column with a bound visibility needs every column to go through a manager.
        var columnManager = new TableViewColumnManager(tableView);
        foreach (var column in new[]
                 {
                     applyColumn,
                     MakeTextColumn(Se.Language.General.NumberSymbol, new Binding(nameof(ImproveTimeCodesRow.Number)), new GridLength(50)),
                     MakeTextColumn(Se.Language.General.Show, new Binding(nameof(ImproveTimeCodesRow.StartTime)) { Converter = new TimeSpanToDisplayFullConverter() }, new GridLength(115)),
                     MakeTextColumn(l.StartShift, new Binding(nameof(ImproveTimeCodesRow.StartShift)), new GridLength(95), TextAlignment.Right),
                     MakeTextColumn(l.EndShift, new Binding(nameof(ImproveTimeCodesRow.EndShift)), new GridLength(95), TextAlignment.Right),
                     MakeTextColumn(Se.Language.General.Text, new Binding(nameof(ImproveTimeCodesRow.Text)), new GridLength(1, GridUnitType.Star)),
                     heardColumn,
                     statusColumn,
                 })
        {
            columnManager.Add(column);
        }

        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(tableView, $"{l.ApplyHint}{System.Environment.NewLine}{System.Environment.NewLine}{l.Heard}: {l.HeardHint}");
        }

        return EditorChrome.MakeListFrame(tableView);
    }

    /// <summary>
    /// Every column is templated so all cells centre on the row - the check box makes the rows
    /// taller than a line of text, and the stock text cell would hang from the top of them.
    /// </summary>
    private static SeTableViewColumn MakeTextColumn(string header, Binding binding, GridLength width, TextAlignment alignment = TextAlignment.Left) => new()
    {
        Header = header,
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        CellTemplate = new FuncDataTemplate<ImproveTimeCodesRow>((_, _) => new TextBlock
        {
            TextAlignment = alignment,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = alignment == TextAlignment.Right ? new Thickness(0, 0, 10, 0) : default,
            // Tabular figures, so a column of shifts reads as a column of numbers.
            FontFeatures = new FontFeatureCollection { FontFeature.Parse("tnum") },
            [!TextBlock.TextProperty] = binding,
        }),
        Width = width,
    };

    private static Control BuildButtonBar(ImproveTimeCodesViewModel vm)
    {
        var buttonAlign = UiUtil.MakeButton(Se.Language.Tools.ImproveTimeCodes.Align, vm.AlignCommand)
            .WithIconLeft("fa-solid fa-wand-magic-sparkles")
            .BindIsEnabled(vm, nameof(vm.IsIdle));
        buttonAlign.MinWidth = 130;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Bind(IsEnabledProperty, new Binding(nameof(vm.HasResult)) { Source = vm });
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // The accent marks the next step: Align until there is something to apply, then OK.
        void UpdateAccent()
        {
            buttonAlign.Classes.Set("primary", !vm.HasResult);
            buttonOk.Classes.Set("primary", vm.HasResult);
        }

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.HasResult))
            {
                UpdateAccent();
            }
        };
        UpdateAccent();

        // The status (the intro, then the result) sits left of the buttons; while aligning it
        // moves up into the progress strip, next to the bar.
        var status = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxLines = 2,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)) { Source = vm },
            [!IsVisibleProperty] = new Binding("!" + nameof(vm.IsAligning)) { Source = vm },
        };

        return EditorChrome.MakeButtonBar(status, buttonAlign, buttonCancel, buttonOk);
    }

    /// <summary>
    /// Progress bar and status while aligning, on the status color above the button bar.
    /// </summary>
    private Control BuildProgressStrip(ImproveTimeCodesViewModel vm)
    {
        var progress = UiUtil.MakeProgressBar(6);
        progress.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)) { Source = vm });
        progress.Bind(ProgressBar.IsIndeterminateProperty, new Binding(nameof(vm.IsProgressIndeterminate)) { Source = vm });
        progress.WithPlatformProgress(this, nameof(vm.IsAligning));

        var strip = EditorChrome.MakeProgressStrip(progress, nameof(vm.StatusText), nameof(vm.IsAligning));
        strip.DataContext = vm;
        return strip;
    }
}
