using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public class FixNetflixErrorsWindow : Window
{
    private readonly FixNetflixErrorsViewModel _vm;

    public FixNetflixErrorsWindow(FixNetflixErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.NetflixCheckAndFix.Title;
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

        var buttonGenerateReport = UiUtil.MakeButton(Se.Language.Tools.NetflixCheckAndFix.GenerateReport, vm.GenerateReportCommand)
            .WithIconLeft(IconNames.Netflix);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonGenerateReport,
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

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeSettingsView(FixNetflixErrorsViewModel vm)
    {
        var panelTop = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8),
            Children =
            {
                UiUtil.MakeTextBlock(Se.Language.General.Language).WithMarginRight(5),
                UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            }
        };

        // Grid with list of checks
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            [!DataGrid.ItemsSourceProperty] = new Binding(nameof(vm.Checks))
        };

        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            CellTemplate = new FuncDataTemplate<NetflixCheckDisplayItem>((item, _) =>
            {
                var cb = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(NetflixCheckDisplayItem.IsSelected)) { Mode = BindingMode.TwoWay },
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                cb.IsCheckedChanged += (_, __) => vm.SetDirty();
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(1),
                    Child = cb
                };
            }),
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(NetflixCheckDisplayItem.Name)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
        });
        _ = new DataGridCheckboxMultiSelect<NetflixCheckDisplayItem>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

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

    private Border MakeFixesView(FixNetflixErrorsViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = _vm,
            ItemsSource = _vm.Fixes,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        var cb = new CheckBox
                        {
                            Focusable = false,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNetflixErrorsItem.Apply)) { Mode = BindingMode.TwoWay },
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };
                        cb.IsEnabled = item.CanBeFixed;

                        return new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = cb,
                        };
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.IndexDisplay)),
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (beforeBlock, _) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = beforeBlock,
                        };
                    }),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (_, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = afterBlock,
                        };
                    }),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Reason,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.Reason)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        _ = new DataGridCheckboxMultiSelect<FixNetflixErrorsItem>(dataGrid,
            item => item.Apply, (item, v) => item.Apply = v,
            canToggle: item => item.CanBeFixed);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
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
