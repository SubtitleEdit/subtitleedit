using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.BridgeGaps;

public class SortByWindow : Window
{
    public SortByWindow(SortByViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.SortBy.Title;
        CanResize = true;
        Width = 1200;
        Height = 800;
        MinWidth = 1000;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var gridControls = MakeSortControls(vm);

        var subtitleView = MakeSubtitleView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Bridge gap smaller than
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Subtitle view
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(gridControls, 0);
        grid.Add(subtitleView, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        Loaded += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private Grid MakeSortControls(SortByViewModel vm)
    {
        var labelProperty = UiUtil.MakeLabel(Se.Language.General.Property);
        var comboBoxProperty = new ComboBox
        {
            Width = 200,
            DataContext = vm,
            ItemsSource = vm.AvailableProperties,
        };
        comboBoxProperty.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(SortByViewModel.SelectedAvailableProperty)) { Mode = BindingMode.TwoWay });

        var checkBoxAscending = UiUtil.MakeCheckBox(Se.Language.General.Ascending, vm, nameof(SortByViewModel.NewCriterionAscending));

        var buttonAdd = UiUtil.MakeButton(Se.Language.General.Add, vm.AddSortCriterionCommand).WithMarginLeft(10);

        var listBoxSortCriteria = new ListBox
        {
            Width = 300,
            Height = 200,
            DataContext = vm,
        };
        listBoxSortCriteria.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(SortByViewModel.SortCriteria)));
        listBoxSortCriteria.Bind(ListBox.SelectedItemProperty, new Binding(nameof(SortByViewModel.SelectedSortCriterion)) { Mode = BindingMode.TwoWay });
        listBoxSortCriteria.ItemTemplate = new FuncDataTemplate<SortCriterion>((criterion, _) =>
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(5)
            };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(SortCriterion.DisplayText)));
            return textBlock;
        }, true);

        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Remove, vm.RemoveSortCriterionCommand).WithMinWidth(130).WithMarginTop(15);
        var buttonMoveUp = UiUtil.MakeButton(Se.Language.General.MoveUp, vm.MoveSortCriterionUpCommand).WithMinWidth(130);
        var buttonMoveDown = UiUtil.MakeButton(Se.Language.General.MoveDown, vm.MoveSortCriterionDownCommand).WithMinWidth(130);
        var buttonToggle = UiUtil.MakeButton(Se.Language.General.ToggleDirection, vm.ToggleSortDirectionCommand).WithMinWidth(130);
        var buttonClear = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearSortCriteriaCommand).WithMinWidth(130);

        var stackPanelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Margin = new Thickness(10, 0, 0, 0),
            Children =
            {
                buttonRemove,
                buttonMoveUp,
                buttonMoveDown,
                buttonToggle,
                buttonClear
            }
        };

        var stackPanelAdd = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                labelProperty,
                comboBoxProperty,
                checkBoxAscending,
                buttonAdd
            }
        };

        var stackPanelList = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                UiUtil.MakeBorderForControl(listBoxSortCriteria),
                stackPanelButtons
            }
        };

        var labelSortOrder = UiUtil.MakeLabel(Se.Language.Tools.SortBy.SortOrder).WithBold().WithMarginBottom(5);

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10),
            Children =
            {
                labelSortOrder,
                stackPanelAdd,
                stackPanelList
            }
        };

        return new Grid
        {
            Children = { mainPanel }
        };
    }

    private Border MakeSubtitleView(SortByViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var twoDecimalConverter = new DoubleToTwoDecimalConverter();
        var dataGridSubtitle = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.Subtitles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Cps,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.CharactersPerSecond)) { Converter = twoDecimalConverter },
                    IsReadOnly = true,
                },
            },
        };

        dataGridSubtitle.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(SortByViewModel.Subtitles)) { Mode = BindingMode.TwoWay });
        dataGridSubtitle.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(SortByViewModel.SelectedSubtitle)) { Mode = BindingMode.TwoWay });


        return UiUtil.MakeBorderForControlNoPadding(dataGridSubtitle);
    }
}
