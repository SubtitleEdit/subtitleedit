using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public class FixNamesWindow : Window
{

    private FixNamesViewModel _vm;

    public FixNamesWindow(FixNamesViewModel vm)
    {
        var lang = Se.Language.Tools.ChangeCasing;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = lang.Title;
        Width = 900;
        Height = 800;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Name title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Names
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Extra names label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Extra names entry
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Hits title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Hits
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
        };

        var row = 0;
        var labelTitle = UiUtil.MakeLabel(lang.FixNames);
        grid.Add(labelTitle, row, 0);

        row++;
        var labelNamesCount = new Label();
        labelNamesCount.Bind(Label.ContentProperty, new Binding(nameof(vm.NamesCount)) { Mode = BindingMode.OneWay });
        grid.Add(labelNamesCount, row, 0);

        row++;
        grid.Add(MakeNamesView(vm), row, 0);

        row++;
        var buttonSelectAll = UiUtil.MakeButton(Se.Language.General.SelectAll, vm.NamesSelectAllCommand);
        var buttonInvertSelection = UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.NamesInvertSelectionCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonSelectAll,
            buttonInvertSelection
        );  
        grid.Add(panelButtons, row, 0);

        row++;
        var labelExtraNames = UiUtil.MakeLabel(lang.ExtraNames);
        grid.Add(labelExtraNames, row, 0);

        row++;
        var textBoxExtraNames = UiUtil.MakeTextBox(600, vm, nameof(vm.ExtraNames));
        textBoxExtraNames.Watermark = lang.EnterExtraNamesHint;
        var buttonAddExtraName = UiUtil.MakeButton(Se.Language.General.Refresh, vm.AddExtraNameCommand);
        var stackExtraNames = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(0, 5, 0, 20),
            Children =
            {
                textBoxExtraNames,
                buttonAddExtraName,
            }
        };
        grid.Add(stackExtraNames, row, 0);

        row++;
        var labelHits = UiUtil.MakeLabel(new Binding(nameof(vm.HitCount)));
        grid.Add(labelHits, row, 0);

        row++;
        grid.Add(MakeHitsView(vm), row, 0);

        // OK and Cancel buttons
        row++;
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtonsBottom = UiUtil.MakeButtonBar(
            buttonOk,
            buttonCancel
        );

        grid.Add(panelButtonsBottom, row, 0);

        Content = grid;
    }

    private static Border MakeNamesView(FixNamesViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            Height = double.NaN, 
            ItemsSource = vm.Names, 
            CanUserSortColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            DataContext = vm,
        };

        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            CellTemplate = new FuncDataTemplate<FixNameItem>(static (item, _) =>
            new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNameItem.IsChecked)),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                }
            }),
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(FixNameItem.Name)),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            IsReadOnly = true,
        });

        var border = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        return border;
    }

    private static Border MakeHitsView(FixNamesViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            Height = double.NaN,
            ItemsSource = vm.Hits,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            DataContext = vm,
        };

        dataGrid.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = Se.Language.General.Apply,
            Binding = new Binding(nameof(FixNameHitItem.IsEnabled)),
            Width = new DataGridLength(80),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(FixNameHitItem.LineIndexDisplay)),
            Width = new DataGridLength(140),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Before,
            Binding = new Binding(nameof(FixNameHitItem.Before)),
            Width = new DataGridLength(220),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.After,
            Binding = new Binding(nameof(FixNameHitItem.After)),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });

        var border = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        return border;
    }

    protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
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
