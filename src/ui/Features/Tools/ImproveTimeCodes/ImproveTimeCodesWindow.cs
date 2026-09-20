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
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;

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
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,5*,Auto,4*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };
        grid.Add(BuildSetupBar(vm), 0, 0);
        grid.Add(BuildVisualizerArea(vm), 1, 0);
        grid.Add(BuildChangeNavBar(vm), 2, 0);
        grid.Add(BuildRowsAndVideo(vm), 3, 0);
        grid.Add(BuildButtonBar(vm), 4, 0);
        Content = grid;

        // Space is taken on the way down, so it plays/pauses wherever the focus is - the list
        // and the buttons would otherwise keep it for themselves. Everything else waits its turn,
        // so Escape still closes an open drop-down before it closes the window.
        AddHandler(KeyDownEvent, (_, e) => vm.OnSpaceKeyDown(e), RoutingStrategies.Tunnel);
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += (_, _) => vm.Dispose();
    }

    private static Control BuildSetupBar(ImproveTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;

        var engineDot = new Ellipse { Width = 10, Height = 10, VerticalAlignment = VerticalAlignment.Center };
        engineDot.Bind(Shape.FillProperty, new Binding(nameof(vm.EngineDotBrush)) { Source = vm });

        var engineStatus = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.EngineStatus)) { Source = vm },
        };

        var engineSettingsName = $"{Se.Language.General.Engine} - {Se.Language.General.Settings}";
        var buttonEngineSettings = UiUtil.MakeButton(vm.ShowEngineSettingsCommand, IconNames.Settings, engineSettingsName)
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

        var numericMaxShift = UiUtil.MakeNumericUpDownOneDecimal(0.1m, 10m, 110, vm, nameof(vm.MaxShiftSeconds), defaultValue: 0.5m);
        AutomationProperties.SetName(numericMaxShift, l.MaxShift);

        var checkStart = UiUtil.MakeCheckBox(l.AdjustStartTimes, vm, nameof(vm.AdjustStart));
        var checkEnd = UiUtil.MakeCheckBox(l.AdjustEndTimes, vm, nameof(vm.AdjustEnd));
        var checkIsolate = UiUtil.MakeCheckBox(l.IsolateSpeech, vm, nameof(vm.IsolateSpeech));
        checkIsolate.Bind(IsEnabledProperty, new Binding(nameof(vm.IsIdle)) { Source = vm });

        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(comboAligner, l.AlignerHint);
            ToolTip.SetTip(numericMaxShift, l.MaxShiftHint);
            ToolTip.SetTip(checkIsolate, l.IsolateSpeechHint);
        }

        var left = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Group(engineDot, engineStatus, buttonEngineSettings),
                Group(Label(l.Aligner), comboAligner),
                Group(Label(l.MaxShift), numericMaxShift),
                Group(checkStart, checkEnd, checkIsolate),
            },
        };

        return new Border
        {
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(70, 128, 128, 128)),
            Background = new SolidColorBrush(Color.FromArgb(18, 128, 128, 128)),
            Padding = new Thickness(12, 6),
            Child = left,
        };
    }

    private static TextBlock Label(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Opacity = 0.85,
    };

    private static StackPanel Group(params Control[] controls)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 4, 18, 4),
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.AddRange(controls);
        return panel;
    }

    private static Control BuildVisualizerArea(ImproveTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;

        var (labelOriginal, borderOriginal, avOriginal) = BuildLabeledVisualizer(l.Original, OriginalTint);
        var (labelAligned, borderAligned, avAligned) = BuildLabeledVisualizer(l.Aligned, AlignedTint);

        vm.AudioVisualizerOriginal = avOriginal;
        vm.AudioVisualizerAligned = avAligned;

        // Either waveform can be scrolled or zoomed; the other follows. Setting a property to
        // the value it already has raises nothing, so the two handlers do not ping-pong.
        Mirror(avOriginal, avAligned);
        Mirror(avAligned, avOriginal);
        ReloadParagraphsWhenViewMoves(avOriginal, vm);
        ReloadParagraphsWhenViewMoves(avAligned, vm);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        var checkSpeechOnly = UiUtil.MakeCheckBox(l.ShowSpeechOnly, vm, nameof(vm.ShowSpeechOnly));
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
            Spacing = 6,
            Margin = new Thickness(2, 6, 5, 3),
            Children =
            {
                swatch,
                new TextBlock { Text = title, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center },
            },
        };

        var av = new AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
            ParagraphBackground = tint,
            ParagraphSelectedBackground = CurrentTint,
        };

        var border = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            ClipToBounds = true,
            Background = Brushes.Black,
            Child = av,
        };
        return (label, border, av);
    }

    private static Control BuildChangeNavBar(ImproveTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;

        var buttonPrev = UiUtil.MakeButton(vm.PreviousChangeCommand, IconNames.ArrowUpThin, $"{l.PreviousChange} (F7)");
        buttonPrev.Bind(IsEnabledProperty, new Binding(nameof(vm.CanGoPrevious)) { Source = vm });

        var buttonNext = UiUtil.MakeButton(vm.NextChangeCommand, IconNames.ArrowDownThin, $"{l.NextChange} (F8)");
        buttonNext.Bind(IsEnabledProperty, new Binding(nameof(vm.CanGoNext)) { Source = vm });

        var labelPosition = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            MinWidth = 120,
            TextAlignment = TextAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ChangePositionLabel)) { Source = vm },
        };

        var nav = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonPrev, labelPosition, buttonNext },
        };

        var buttonPlayPause = UiUtil.MakeButton(vm.TogglePlayPauseCommand, "mdi-play-pause", $"{l.PlayPause} (Space)");
        var buttonPlayAligned = UiUtil.MakeButton(l.PlayAligned, vm.PlaySelectedAlignedCommand).WithIconLeft(IconNames.Play);
        var buttonPlayOriginal = UiUtil.MakeButton(l.PlayOriginal, vm.PlaySelectedOriginalCommand).WithIconLeft(IconNames.Play);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonPlayAligned, $"{l.PlayAlignedHint} (F5)");
            ToolTip.SetTip(buttonPlayOriginal, $"{l.PlayOriginalHint} (Shift+F5)");
        }

        nav.Children.Add(UiUtil.MakeVerticalSeparator(margin: new Thickness(8, 2)));
        nav.Children.Add(buttonPlayPause);
        nav.Children.Add(buttonPlayOriginal);
        nav.Children.Add(buttonPlayAligned);

        var summary = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Opacity = 0.8,
            FontSize = UiUtil.ScaledFontSize(12),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SummaryLine)) { Source = vm },
        };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), ColumnSpacing = 14 };
        grid.Add(nav, 0, 0);
        grid.Add(summary, 0, 1);

        return new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 128, 128, 128)),
            BorderThickness = new Thickness(0, 1, 0, 1),
            Padding = new Thickness(6),
            Child = grid,
        };
    }

    private static Control BuildRowsAndVideo(ImproveTimeCodesViewModel vm)
    {
        vm.VideoPlayer = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayer.FullScreenIsVisible = false;

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,340"), ColumnSpacing = 4 };
        grid.Add(BuildRowsView(vm), 0, 0);
        grid.Add(new GridSplitter { Width = 4, ResizeDirection = GridResizeDirection.Columns, Background = Brushes.Transparent }, 0, 1);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(vm.VideoPlayer), 0, 2);
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
            Header = l.Apply,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ImproveTimeCodesRow>((_, _) => new Border
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ImproveTimeCodesRow.Apply)) { Mode = BindingMode.TwoWay },
                    // Disabled rather than hidden: a hidden box collapses the row, and the rows
                    // would all change height the moment the results arrive.
                    [!IsEnabledProperty] = new Binding(nameof(ImproveTimeCodesRow.IsChanged)),
                    [!AutomationProperties.NameProperty] = new Binding(nameof(ImproveTimeCodesRow.Text)),
                },
            }),
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

        tableView.Columns.AddRange(new[]
        {
            applyColumn,
            MakeTextColumn(Se.Language.General.NumberSymbol, new Binding(nameof(ImproveTimeCodesRow.Number)), new GridLength(60)),
            MakeTextColumn(Se.Language.General.Show, new Binding(nameof(ImproveTimeCodesRow.StartTime)) { Converter = new TimeSpanToDisplayFullConverter() }, new GridLength(115)),
            MakeTextColumn(l.StartShift, new Binding(nameof(ImproveTimeCodesRow.StartShift)), new GridLength(95), TextAlignment.Right),
            MakeTextColumn(l.EndShift, new Binding(nameof(ImproveTimeCodesRow.EndShift)), new GridLength(95), TextAlignment.Right),
            MakeTextColumn(Se.Language.General.Text, new Binding(nameof(ImproveTimeCodesRow.Text)), new GridLength(1, GridUnitType.Star)),
            statusColumn,
        });

        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(tableView, l.ApplyHint);
        }

        return UiUtil.MakeBorderForControlNoPadding(tableView);
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
            buttonAlign.Classes.Set("accent", !vm.HasResult);
            buttonOk.Classes.Set("accent", vm.HasResult);
        }

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.HasResult))
            {
                UpdateAccent();
            }
        };
        UpdateAccent();

        // Progress and status share the line with the buttons, next to the Align that drives them.
        var status = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Opacity = 0.85,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)) { Source = vm },
        };

        var progress = UiUtil.MakeProgressBar();
        progress.Width = double.NaN;
        progress.HorizontalAlignment = HorizontalAlignment.Stretch;
        progress.VerticalAlignment = VerticalAlignment.Center;
        progress.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)) { Source = vm });
        progress.Bind(IsVisibleProperty, new Binding(nameof(vm.IsAligning)) { Source = vm });
        progress.Bind(ProgressBar.IsIndeterminateProperty, new Binding(nameof(vm.IsProgressIndeterminate)) { Source = vm });

        var buttons = UiUtil.MakeButtonBar(buttonAlign, buttonOk, buttonCancel);
        // The bar takes all the room left of the buttons, with its text underneath.
        status.FontSize = UiUtil.ScaledFontSize(12);
        var progressPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2, 10, 0, 0),
            Children = { progress, status },
        };

        var bar = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 20 };
        bar.Add(progressPanel, 0, 0);
        bar.Add(buttons, 0, 1);
        return bar;
    }
}
