using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

public class CheckArteErrorsWindow : Window
{
    private readonly CheckArteErrorsViewModel _vm;
    private ComboBox _comboBoxProfile = null!;

    public CheckArteErrorsWindow(CheckArteErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle("Check and fix ARTE errors");
        Width = 1100;
        Height = 680;
        MinWidth = 900;
        MinHeight = 640;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonGenerateReport = UiUtil.MakeButton("Generate report", vm.GenerateReportCommand);
        var buttonAnalyze = UiUtil.MakeButton("Analyze", vm.AnalyzeCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonGenerateReport,
            buttonAnalyze,
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            Margin = new Thickness(4, 0, 10, 0),
        };
        summaryText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FixesSummaryText)));

        var buttonSelectPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.ChecksSelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.ChecksInverseSelectionCommand),
            summaryText
        ).WithAlignmentLeft().WithAlignmentTop();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(310, GridUnitType.Pixel) },
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
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, _comboBoxProfile);
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeSettingsView(CheckArteErrorsViewModel vm)
    {
        _comboBoxProfile = UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile));

        var panelProfile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                UiUtil.MakeTextBlock("ARTE profile").WithMarginRight(5),
                _comboBoxProfile,
            }
        };

        var panelTop = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8),
            Spacing = 4,
            Children =
            {
                panelProfile,
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
            Binding = new Binding(nameof(CheckArteErrorsViewModel.ArteCheckItem.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        });

        TableViewExtras.AddSpaceToggle<CheckArteErrorsViewModel.ArteCheckItem>(
            dataGrid,
            item => item.IsSelected,
            (item, value) => item.IsSelected = value);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(306, GridUnitType.Pixel) },
            },
        };

        grid.Add(panelTop, 0, 0);
        grid.Add(dataGrid, 1, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }

    private Border MakeFixesView(CheckArteErrorsViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = _vm;
        dataGrid.ItemsSource = _vm.Fixes;

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
                    var (beforeBlock, _) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                    return new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = beforeBlock,
                    };
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
                    var (_, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                    return new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = afterBlock,
                    };
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

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));

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
        grid.Add(dataGrid, 1, 0);

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
