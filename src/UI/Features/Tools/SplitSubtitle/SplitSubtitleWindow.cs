using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

public class SplitSubtitleWindow : Window
{
    public SplitSubtitleWindow(SplitSubtitleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.SplitSubtitle.Title;
        CanResize = true;
        Width = 900;
        Height = 700;
        MinWidth = 650;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButton(Se.Language.Tools.SplitSubtitle.SaveSplitParts, vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // input/output info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // split options
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // split items list
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
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

        grid.Add(MakeInputOutputView(vm), 0);
        grid.Add(MakeOptionsView(vm), 1);
        grid.Add(MakeListView(vm), 2);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeInputOutputView(SplitSubtitleViewModel vm)
    {
        var labelSubtitleInfo = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SubtitleInfo));

        var labelOutputFolder = UiUtil.MakeLabel(Se.Language.General.OutputFolder).WithMinWidth(100);
        var textBoxOutputFolder = UiUtil.MakeTextBox(450, vm, nameof(vm.OutputFolder));
        var buttonBrowse = UiUtil.MakeBrowseButton(vm.BrowseCommand);
        var buttonOpen = UiUtil.MakeButton(vm.OpenFolderCommand, IconNames.FolderOpen, Se.Language.General.OpenOutputFolder);
        var panelOutputFolder = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelOutputFolder,
                textBoxOutputFolder,
                buttonBrowse,
                buttonOpen,
            }
        };

        var labelFormat = UiUtil.MakeLabel(Se.Language.General.Format).WithMinWidth(100);

        var comboBoxFormat = new ComboBox
        {
            Width = 200,
            Height = 30,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Formats)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedSubtitleFormat)),
            DataContext = vm,
            ItemTemplate = new FuncDataTemplate<object>((item, _) =>
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleFormat.Name)),
                    Width = 150,
                }, true)
        };

        var panelFormat = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelFormat,
                comboBoxFormat,
            }
        };

        var labelEncoding = UiUtil.MakeLabel(Se.Language.General.Encoding).WithMinWidth(100);
        var comboBoxEncoding = new ComboBox
        {
            Width = 200,
            Height = 30,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Encodings)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEncoding)),
            DataContext = vm,
        };

        var panelEncoding = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelEncoding,
                comboBoxEncoding,
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                labelSubtitleInfo,
                panelOutputFolder,
                panelFormat,
                panelEncoding,
            },
            Spacing = 5,
        };

        return UiUtil.MakeBorderForControl(panel);
    }

    private static Border MakeOptionsView(SplitSubtitleViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var radioLines = UiUtil.MakeRadioButton(Se.Language.General.Lines, vm, nameof(vm.SplitByLines));
        radioLines.IsCheckedChanged += (s, e) => vm.SetDirty();   
        var radioCharacters = UiUtil.MakeRadioButton(Se.Language.General.Characters, vm, nameof(vm.SplitByCharacters));
        radioCharacters.IsCheckedChanged += (s, e) => vm.UpdateSplit();
        var radioTime = UiUtil.MakeRadioButton(Se.Language.General.Time, vm, nameof(vm.SplitByTime));
        radioTime.IsCheckedChanged += (s, e) => vm.SetDirty();
        var panelSplitBy = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Children = { radioLines, radioCharacters, radioTime },
            Margin = new Avalonia.Thickness(0, 0, 20, 0),
        };

        var labelParts = UiUtil.MakeLabel(Se.Language.Tools.SplitSubtitle.NumberOfEqualParts);
        var numberUpDownParts = UiUtil.MakeNumericUpDownInt(1, vm.PartsMax, 1, 110, vm, nameof(vm.NumberOfEqualParts));
        numberUpDownParts.ValueChanged += (s, e) => vm.SetDirty();
        numberUpDownParts.Bind(NumericUpDown.MaximumProperty, new Binding(nameof(vm.PartsMax)) { Source = vm, Mode = BindingMode.OneWay });
        var panelParts = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Children = { labelParts, numberUpDownParts },
        };

        grid.Add(panelSplitBy, 0);
        grid.Add(panelParts, 0, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Control MakeListView(SplitSubtitleViewModel vm)
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
            DataContext = vm,
            ItemsSource = vm.SplitItems,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NoSymbolLines,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.Lines)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Characters,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.Characters)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SplitDisplayItem.FileName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSpiltItem)) { Source = vm });

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
