using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

public class CheckArteErrorsWindow : Window
{
    private readonly CheckArteErrorsViewModel _vm;
    private ComboBox _comboBoxLanguage = null!;

    public CheckArteErrorsWindow(CheckArteErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.CheckArteErrors.Title);
        Width = 1100;
        Height = 680;
        MinWidth = 900;
        MinHeight = 640;
        CanResize = true;
        SizeToContent = SizeToContent.Manual;
        WindowState = WindowState.Normal;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        var closeButton = UiUtil.MakeButton("Close", vm.CancelCommand);
        var topBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
        };
        topBar.Add(new TextBlock
        {
            Text = Se.Language.Tools.CheckArteErrors.Title,
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        }, 0, 0);
        topBar.Add(closeButton, 0, 1);

        var runChecksButton = UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.RunChecks, vm.RunChecksCommand);
        runChecksButton.HorizontalAlignment = HorizontalAlignment.Stretch;
        var applyCorrectionsButton = UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.ApplyCorrections, vm.OkCommand);
        applyCorrectionsButton.HorizontalAlignment = HorizontalAlignment.Stretch;

        var runChecksBorder = new Border
        {
            BorderBrush = Brushes.DodgerBlue,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(3),
            Child = runChecksButton,
        };
        var applyCorrectionsBorder = new Border
        {
            BorderBrush = Brushes.LimeGreen,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(3),
            Child = applyCorrectionsButton,
        };

        var primaryActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 10,
        };
        primaryActions.Add(runChecksBorder, 0, 0);
        primaryActions.Add(applyCorrectionsBorder, 0, 1);

        var resultsSeparator = new Border
        {
            Height = 2,
            Background = Brushes.Orange,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var fixesButtons = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.SelectAllFixable, vm.FixesSelectAllCommand),
            UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.ClearSelection, vm.FixesClearSelectionCommand),
            UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.UndoLastChange, vm.UndoCommand),
            UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.DownloadErrorReport, vm.DownloadErrorReportCommand)
        ).WithAlignmentLeft();

        var resultsHeader = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                resultsSeparator,
                fixesButtons,
            },
        };

        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(4, 0, 10, 0),
        };
        summaryText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FixesSummaryText)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(topBar, 0, 0);
        grid.Add(settingsView, 1, 0);
        grid.Add(primaryActions, 2, 0);
        grid.Add(resultsHeader, 3, 0);
        grid.Add(summaryText, 4, 0);
        grid.Add(fixesView, 5, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, _comboBoxLanguage);
        // Always start at the review size above. Restoring saved dimensions can make
        // a normal-state window fill the screen just as a maximized window would.
    }

    private Border MakeSettingsView(CheckArteErrorsViewModel vm)
    {
        _comboBoxLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        _comboBoxLanguage.MinWidth = 170;

        var sdh = new CheckBox
        {
            Content = "SDH",
            VerticalAlignment = VerticalAlignment.Center,
        };
        sdh.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.IsSdh)) { Mode = BindingMode.TwoWay });

        var ebuOptions = UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.HeaderInfo, vm.OpenEbuOptionsCommand);

        var sourceFrameRate = UiUtil.MakeComboBox(vm.SourceFrameRates, vm, nameof(vm.SelectedSourceFrameRate));
        sourceFrameRate.MinWidth = 150;

        var maxCells = new NumericUpDown { Minimum = 1, Maximum = 100, Increment = 1, Width = 118 };
        maxCells.Bind(NumericUpDown.ValueProperty, new Binding(nameof(vm.TeletextMaxCells)) { Mode = BindingMode.TwoWay });

        var minimumGapFrames = new NumericUpDown { Minimum = 0, Maximum = 250, Increment = 1, Width = 118 };
        minimumGapFrames.Bind(NumericUpDown.ValueProperty, new Binding(nameof(vm.MinimumGapFrames)) { Mode = BindingMode.TwoWay });

        var tolerance = new NumericUpDown { Minimum = 0, Maximum = 100, Increment = 1, Width = 118 };
        tolerance.Bind(NumericUpDown.ValueProperty, new Binding(nameof(vm.ReadingDurationTolerancePercent)) { Mode = BindingMode.TwoWay });

        var acceptShort = new CheckBox
        {
            Content = Se.Language.Tools.CheckArteErrors.AcceptShortDurations,
            VerticalAlignment = VerticalAlignment.Center,
        };
        acceptShort.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.AcceptShortDurations)) { Mode = BindingMode.TwoWay });

        var shortMinimumFrames = new NumericUpDown { Minimum = 1, Maximum = 250, Increment = 1, Width = 104 };
        shortMinimumFrames.Bind(NumericUpDown.ValueProperty, new Binding(nameof(vm.ShortMinimumFrames)) { Mode = BindingMode.TwoWay });
        shortMinimumFrames.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.AcceptShortDurations)));

        var artePreset = UiUtil.MakeButton(Se.Language.Tools.CheckArteErrors.Preset, vm.ApplyArtePresetCommand);
        artePreset.IsEnabled = !vm.IsArtePresetActive;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsArtePresetActive))
            {
                artePreset.IsEnabled = !vm.IsArtePresetActive;
            }
        };

        var shiftToStart = new CheckBox
        {
            Content = Se.Language.Tools.CheckArteErrors.ShiftWholeFileTo,
            VerticalAlignment = VerticalAlignment.Center,
        };
        shiftToStart.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.ShiftWholeFileToStartTimeCode)) { Mode = BindingMode.TwoWay });

        var targetStartTimeCode = new TimeCodeUpDown { MinWidth = 130 };
        targetStartTimeCode.Bind(TimeCodeUpDown.ValueProperty, new Binding(nameof(vm.TargetStartTimeCode)) { Mode = BindingMode.TwoWay });
        targetStartTimeCode.Bind(IsEnabledProperty, new Binding(nameof(vm.ShiftWholeFileToStartTimeCode)));

        StackPanel Pair(string label, Control control) => new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center },
                control,
            },
        };

        var row1 = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Children =
            {
                Pair(Se.Language.Tools.CheckArteErrors.Language, _comboBoxLanguage),
                sdh,
                ebuOptions,
                Pair(Se.Language.Tools.CheckArteErrors.FrameRate, sourceFrameRate),
                new TextBlock { Text = "→ 25 fps", VerticalAlignment = VerticalAlignment.Center },
            },
        };

        var row2 = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Children =
            {
                Pair(Se.Language.Tools.CheckArteErrors.TeletextMaxCells, maxCells),
                Pair(Se.Language.Tools.CheckArteErrors.MinimumGapFrames, minimumGapFrames),
                Pair(Se.Language.Tools.CheckArteErrors.ReadingDurationTolerance, tolerance),
            },
        };

        var shortMinPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock { Text = Se.Language.Tools.CheckArteErrors.Minimum, VerticalAlignment = VerticalAlignment.Center },
                shortMinimumFrames,
                new TextBlock { Text = Se.Language.Tools.CheckArteErrors.FramesShort, VerticalAlignment = VerticalAlignment.Center },
            },
        };

        var shiftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { shiftToStart, targetStartTimeCode },
        };

        var row3 = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Children =
            {
                acceptShort,
                shortMinPanel,
                artePreset,
                shiftPanel,
            },
        };

        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Checks));

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<CheckArteErrorsViewModel.ArteCheckItem>((item, _) =>
            {
                var cb = new CheckBox
                {
                    Focusable = false,
                    IsEnabled = item.IsImplemented,
                    [!ToggleButton.IsCheckedProperty] =
                        new Binding(nameof(CheckArteErrorsViewModel.ArteCheckItem.IsSelected))
                        {
                            Mode = BindingMode.TwoWay
                        },
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(1),
                    Child = cb,
                };
            }),
            Width = new GridLength(70),
        });

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(CheckArteErrorsViewModel.ArteCheckItem.DisplayName)),
            Width = new GridLength(1, GridUnitType.Star),
        });

        TableViewExtras.AddSpaceToggle<CheckArteErrorsViewModel.ArteCheckItem>(
            dataGrid,
            item => item.IsSelected,
            (item, value) => { if (item.IsImplemented) item.IsSelected = value; });

        var checksPanel = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),
            MinHeight = 180,
        };
        checksPanel.Add(dataGrid, 0, 0);

        var checksExpander = new Expander
        {
            Header = Se.Language.Tools.CheckArteErrors.Checks,
            IsExpanded = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = checksPanel,
        };

        var panel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(8),
            Children =
            {
                row1,
                row2,
                row3,
                checksExpander,
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(panel);
    }

    private static Control MakeSubtitlePreview(CheckArteErrorsViewModel.ArteFixItem item, bool after)
    {
        var panel = new StackPanel { Spacing = 4 };
        void Add(string time, TextBlock text)
        {
            if (!string.IsNullOrEmpty(time))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = time,
                    Foreground = UiTheme.IsDarkThemeEnabled() ? Brushes.LightSkyBlue : Brushes.DarkSlateBlue,
                    FontWeight = FontWeight.SemiBold,
                });
            }
            text.TextWrapping = TextWrapping.NoWrap;
            text.VerticalAlignment = VerticalAlignment.Top;
            panel.Children.Add(text);
        }
        if (after && item.SplitParagraphs != null)
        {
            foreach (var part in item.SplitParagraphs)
            {
                Add(CheckArteErrorsViewModel.FormatTimeRange(part), new TextBlock { Text = part.Text });
            }
        }
        else
        {
            var (beforeText, afterText) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
            Add(item.ShowTiming ? item.TimeRange : string.Empty, after ? afterText : beforeText);
        }
        return new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(4),
            Child = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = panel,
            },
        };
    }

    private TableView MakeFixesTable(IReadOnlyList<CheckArteErrorsViewModel.ArteFixItem> items)
    {
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = _vm;
        // Variable-height previews destabilize the virtual panel's estimated scroll extent.
        // Measuring real rows keeps scrollbar dragging anchored, including the last subtitle.
        dataGrid.ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel());
        dataGrid.ItemsSource = items;
        ScrollViewer.SetVerticalScrollBarVisibility(dataGrid, ScrollBarVisibility.Disabled);

        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Apply,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<CheckArteErrorsViewModel.ArteFixItem>((item, _) =>
                {
                    var cb = new CheckBox
                    {
                        Focusable = false,
                        [!ToggleButton.IsCheckedProperty] =
                            new Binding(nameof(CheckArteErrorsViewModel.ArteFixItem.Apply))
                            {
                                Mode = BindingMode.TwoWay
                            },
                        HorizontalAlignment = HorizontalAlignment.Center,
                        IsEnabled = item.CanBeFixed,
                    };

                    return new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = cb,
                    };
                }),
                Width = new GridLength(70),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(CheckArteErrorsViewModel.ArteFixItem.IndexDisplay)),
                Width = new GridLength(60),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Before,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<CheckArteErrorsViewModel.ArteFixItem>((item, _) =>
                {
                    return MakeSubtitlePreview(item, false);
                }),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.After,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<CheckArteErrorsViewModel.ArteFixItem>((item, _) =>
                {
                    return MakeSubtitlePreview(item, true);
                }),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Reason,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(CheckArteErrorsViewModel.ArteFixItem.Reason)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });

        TableViewExtras.AddSpaceToggle<CheckArteErrorsViewModel.ArteFixItem>(
            dataGrid,
            item => !item.CanBeFixed || item.Apply,
            (item, value) =>
            {
                if (item.CanBeFixed)
                {
                    item.Apply = value;
                }
            });

        return dataGrid;
    }

    private Border MakeFixesView(CheckArteErrorsViewModel vm)
    {
        var groups = new ItemsControl
        {
            ItemsSource = vm.FixGroups,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Spacing = 6 }),
            ItemTemplate = new FuncDataTemplate<CheckArteErrorsViewModel.ArteFixGroup>((group, _) =>
            {
                var header = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ColumnSpacing = 16,
                };
                header.Add(new TextBlock { Text = group.Header, VerticalAlignment = VerticalAlignment.Center }, 0, 0);
                header.Add(UiUtil.MakeButtonBar(
                    UiUtil.MakeButton(Se.Language.General.SelectAll, group.SelectAllCommand),
                    UiUtil.MakeButton(Se.Language.General.InvertSelection, group.InvertSelectionCommand)), 0, 1);
                var expander = new Expander
                {
                    Header = header,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    DataContext = group,
                    Content = MakeFixesTable(group.Items),
                };
                expander.Bind(Expander.IsExpandedProperty,
                    new Binding(nameof(group.IsExpanded)) { Mode = BindingMode.TwoWay });
                return expander;
            }),
        };
        var groupScroll = new ScrollViewer
        {
            Content = groups,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var noErrorsText = new TextBlock
        {
            Text = Se.Language.Tools.CheckArteErrors.NoErrorsDetected,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 18,
            Opacity = 0.75,
            IsVisible = vm.Fixes.Count == 0,
        };

        void UpdateEmptyState()
        {
            noErrorsText.IsVisible = vm.FixGroups.Count == 0;
            groupScroll.IsVisible = vm.FixGroups.Count > 0;
        }

        vm.FixGroups.CollectionChanged += (_, _) => UpdateEmptyState();
        UpdateEmptyState();

        var resultsGrid = new Grid();
        resultsGrid.Children.Add(groupScroll);
        resultsGrid.Children.Add(noErrorsText);

        return UiUtil.MakeBorderForControlNoPadding(resultsGrid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}
