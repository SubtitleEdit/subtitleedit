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
        Title = UiUtil.MakeWindowTitle("Check and fix ARTE errors");
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

        var buttonGenerateReport = UiUtil.MakeButton("Generate report", vm.GenerateReportCommand);
        var buttonAnalyze = UiUtil.MakeButton("Analyze", vm.AnalyzeCommand);
        var buttonOk = UiUtil.MakeButton("Start correction", vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonGenerateReport,
            buttonAnalyze,
            UiUtil.MakeButton("Undo", vm.UndoCommand),
            buttonOk,
            UiUtil.MakeButton("Close", vm.CancelCommand)
        );

        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(4, 0, 10, 0),
        };
        summaryText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FixesSummaryText)));

        var buttonSelectPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.ChecksSelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.ChecksInverseSelectionCommand)
        ).WithAlignmentLeft().WithAlignmentTop();

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
                new ColumnDefinition { Width = new GridLength(340, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(settingsView, 0, 0);
        grid.Add(fixesView, 0, 1);
        grid.Add(buttonSelectPanel, 1);
        grid.Add(panelButtons, 1, 1);
        grid.Add(summaryText, 2, 0, 1, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, _comboBoxLanguage);
        // Always start at the review size above. Restoring saved dimensions can make
        // a normal-state window fill the screen just as a maximized window would.
    }

    private Border MakeSettingsView(CheckArteErrorsViewModel vm)
    {
        _comboBoxLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        _comboBoxLanguage.MinWidth = 110;
        _comboBoxLanguage.MaxWidth = 130;

        var sdh = new CheckBox
        {
            Content = "SDH",
            VerticalAlignment = VerticalAlignment.Center,
        };
        sdh.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.IsSdh)) { Mode = BindingMode.TwoWay });

        var ebuOptions = UiUtil.MakeButton(
            vm.OpenEbuOptionsCommand,
            IconNames.Cogs,
            "EBU STL header/options");

        // This lives in a 336 px settings column. A horizontal StackPanel lets the
        // language picker grow past that column and leaves the gear visually stranded
        // beside the result view. A grid keeps every control inside the setup area.
        var languagePanel = new Grid
        {
            VerticalAlignment = VerticalAlignment.Center,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 6,
        };
        languagePanel.Add(new TextBlock { Text = "Language", VerticalAlignment = VerticalAlignment.Center }, 0, 0);
        languagePanel.Add(_comboBoxLanguage, 0, 1);
        languagePanel.Add(sdh, 0, 2);
        languagePanel.Add(ebuOptions, 0, 3);

        var sourceFrameRate = UiUtil.MakeComboBox(vm.SourceFrameRates, vm, nameof(vm.SelectedSourceFrameRate));
        sourceFrameRate.MinWidth = 90;

        var frameRatePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock { Text = "Frame rate", VerticalAlignment = VerticalAlignment.Center },
                sourceFrameRate,
                new TextBlock { Text = "→ 25 fps", VerticalAlignment = VerticalAlignment.Center },
            },
        };

        var shiftToStart = new CheckBox
        {
            Content = "Shift file to TC In",
            VerticalAlignment = VerticalAlignment.Center,
        };
        shiftToStart.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(vm.ShiftWholeFileToStartTimeCode)) { Mode = BindingMode.TwoWay });

        var targetStartTimeCode = new TimeCodeUpDown
        {
            MinWidth = 112,
        };
        targetStartTimeCode.Bind(TimeCodeUpDown.ValueProperty, new Binding(nameof(vm.TargetStartTimeCode)) { Mode = BindingMode.TwoWay });
        targetStartTimeCode.Bind(IsEnabledProperty, new Binding(nameof(vm.ShiftWholeFileToStartTimeCode)));

        var shiftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { shiftToStart, targetStartTimeCode },
        };

        var panelTop = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8),
            Spacing = 4,
            Children =
            {
                languagePanel,
                shiftPanel,
                frameRatePanel,
            }
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

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(336, GridUnitType.Pixel) },
            },
        };

        grid.Add(panelTop, 0, 0);
        grid.Add(dataGrid, 1, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
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
            Text = "No ARTE errors detected.",
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

        var fixesSelectPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.FixesSelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.FixesInverseSelectionCommand)
        ).WithAlignmentLeft().WithAlignmentTop();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 6,
        };

        grid.Add(fixesSelectPanel, 0, 0);
        grid.Add(resultsGrid, 1, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
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
