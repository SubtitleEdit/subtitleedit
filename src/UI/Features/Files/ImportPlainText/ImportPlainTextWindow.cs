using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public class ImportPlainTextWindow : Window
{
    public ImportPlainTextWindow(ImportPlainTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportPlainText;
        CanResize = true;
        Width = 1200;
        Height = 850;
        MinWidth = 1100;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var buttonImportFiles = UiUtil.MakeButton(Se.Language.File.Import.ImportFilesDotDotDot, vm.FilesImportCommand).WithMinWidth(110);
        buttonImportFiles.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm });
        var buttonImportFile = UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.FileImportCommand).WithMinWidth(110);
        buttonImportFile.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm, Converter = new InverseBooleanConverter() });
        var checkBoxImportFiles = UiUtil.MakeCheckBox(Se.Language.File.Import.MultipleFiles, vm, nameof(vm.IsImportFilesVisible));
        checkBoxImportFiles.IsCheckedChanged += (s, e) => vm.CheckBoxImportFilesChanged();
        var panelImport = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                buttonImportFiles,
                buttonImportFile,
                checkBoxImportFiles,
            },
            Spacing = 10,
        };

        var labelNumberOfSubtitles = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.NumberOfSubtitles)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(panelImport, 0);
        grid.Add(MakeTextBoxAndControlsView(vm), 1);
        grid.Add(MakeSubtitleGridView(vm), 2);
        grid.Add(labelNumberOfSubtitles, 3, 0);
        grid.Add(panelButtons, 3, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Grid MakeTextBoxAndControlsView(ImportPlainTextViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        var textBox = new TextBox
        {
            IsReadOnly = false,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 350,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DataContext = vm,
        };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PlainText)) { Mode = BindingMode.TwoWay });
        textBox.Bind(TextBox.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm, Converter = new InverseBooleanConverter() });
        textBox.TextChanged += (s, e) => vm.PlainTextChanged();
        var sizeConverter = new FileSizeConverter();

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = 348,
            DataContext = vm,
            ItemsSource = vm.Files,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(DisplayFile.FileName)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Size,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(DisplayFile.Size)) { Converter = sizeConverter, Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedFile)) { Source = vm });

        // hack to make drag and drop work on the DataGrid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);
        dropHost.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm });

        var flyout = new MenuFlyout();
        dropHost.ContextFlyout = flyout;
        var menuItemImport = new MenuItem
        {
            Header = Se.Language.General.ImportDotDotDot,
            DataContext = vm,
            Command = vm.FilesImportCommand,
        };
        flyout.Items.Add(menuItemImport);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.FilesClearCommand,
        };
        flyout.Items.Add(menuItemClear);


        grid.Add(textBox, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(dropHost), 0);
        grid.Add(MakeControlsView(vm), 0, 1);

        return grid;
    }

    private static Border MakeControlsView(ImportPlainTextViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 320,
        };

        var labelSplit = UiUtil.MakeLabel(Se.Language.File.Import.SplitTextAt);
        var comboBoxSplit = new ComboBox
        {
            DataContext = vm,
        };
        comboBoxSplit.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(vm.SplitAtOptions)) { Source = vm });
        comboBoxSplit.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(vm.SelectedSplitAtOption)) { Source = vm, Mode = BindingMode.TwoWay });
        comboBoxSplit.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm, Converter = new InverseBooleanConverter() });
        comboBoxSplit.SelectionChanged += (s, e) => vm.SplitAtOptionChanged();

        var panelSplit = UiUtil.MakeHorizontalPanel(labelSplit, comboBoxSplit);

        var labelGap = UiUtil.MakeLabel(Se.Language.File.Import.GapMs);
        var numericUpDownGap = UiUtil.MakeNumericUpDownInt(0, 1000, Se.Settings.Tools.BridgeGaps.MinGapMs, 130, vm, nameof(vm.MinGapMs));
        numericUpDownGap.ValueChanged += vm.GapChanged;
        var panelGap = UiUtil.MakeHorizontalPanel(labelGap, numericUpDownGap).WithMarginTop(30);

        grid.Add(panelSplit, 0);
        grid.Add(panelGap, 1);


        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeSubtitleGridView(ImportPlainTextViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var fileSizeConverter = new FileSizeConverter();
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
            ItemsSource = vm.Subtitles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Hide,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter, Mode = BindingMode.OneWay },
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)) { Source = vm });

        grid.Add(dataGrid, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }
}
