using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class ProfilesWindow : Window
{
    public ProfilesWindow(ProfilesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Profiles;
        CanResize = true;
        Width = 1100;
        Height = 750;
        MinWidth = 800;
        MinHeight = 700;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.Tools.AdjustDurations.AdjustVia,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeDataGrid(vm), 0, 0);
        grid.Add(MakeControlsGrid(vm), 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeDataGrid(ProfilesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
        };

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
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Name,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ProfileDisplay.Name)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.Options.Settings.SingleLineMaxLength,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ProfileDisplay.SingleLineMaxLength)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.MaxCharactersPerSecond,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ProfileDisplay.MaxCharsPerSec)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Profiles)) { Source = vm });
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedProfile)) { Source = vm });

        var buttonExport = UiUtil.MakeButton(vm.ExportCommand, IconNames.Export, Se.Language.General.ExportDotDotDot);
        var buttonImport = UiUtil.MakeButton(vm.ImportCommand, IconNames.Import, Se.Language.General.ImportDotDotDot);
        var buttonCopy = UiUtil.MakeButton(vm.CopyCommand, IconNames.Copy, Se.Language.General.Copy);
        var buttonDelete = UiUtil.MakeButton(vm.DeleteCommand, IconNames.Trash, Se.Language.General.Delete);
        var buttonClear = UiUtil.MakeButton(vm.ClearCommand, IconNames.Close, Se.Language.General.Clear);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonExport,
            buttonImport,
            buttonCopy,
            buttonDelete,
            buttonClear
            ).WithAlignmentLeft();

        grid.Add(dataGrid, 0);
        grid.Add(panelButtons, 1);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }

    private static Grid MakeControlsGrid(ProfilesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(0),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(250, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.Name));

        var labelSingleLineMaxLength = UiUtil.MakeLabel(Se.Language.Options.Settings.SingleLineMaxLength);
        var numericUpDownSingleLineMaxLength = UiUtil.MakeNumericUpDownInt(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.SingleLineMaxLength));

        var labelOptimalCharsPerSec = UiUtil.MakeLabel(Se.Language.Options.Settings.OptimalCharsPerSec);
        var numericUpDownOptimalCharsPerSec = UiUtil.MakeNumericUpDownInt(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.OptimalCharsPerSec));

        var labelMaxCharsPerSec = UiUtil.MakeLabel(Se.Language.General.MaxCharactersPerSecond);
        var numericUpDownMaxCharsPerSec = UiUtil.MakeNumericUpDownInt(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxCharsPerSec));

        var labelMaxWordsPerMin = UiUtil.MakeLabel(Se.Language.Options.Settings.MaxWordsPerMin);
        var numericUpDownMaxWordsPerMin = UiUtil.MakeNumericUpDownInt(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxWordsPerMin));

        var labelMinDurationMs = UiUtil.MakeLabel(Se.Language.Options.Settings.MinDurationMs);
        var numericUpDownMinDurationMs = UiUtil.MakeNumericUpDownInt(0, 10000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MinDurationMs));

        var labelMaxDurationMs = UiUtil.MakeLabel(Se.Language.Options.Settings.MaxDurationMs);
        var numericUpDownMaxDurationMs = UiUtil.MakeNumericUpDownInt(0, 10000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxDurationMs));

        var labelMinGapMs = UiUtil.MakeLabel(Se.Language.Options.Settings.MinGapMs);
        var numericUpDownMinGapMs = UiUtil.MakeNumericUpDownInt(0, 10000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MinGapMs));

        var labelMaxLines = UiUtil.MakeLabel(Se.Language.Options.Settings.MaxLines);
        var numericUpDownMaxLines = UiUtil.MakeNumericUpDownInt(1, 10, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxLines));

        var labelUnbreakLinesShorterThan = UiUtil.MakeLabel(Se.Language.Options.Settings.UnbreakSubtitlesShortThan);
        var numericUpDownUnbreakLinesShorterThan = UiUtil.MakeNumericUpDownInt(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.UnbreakLinesShorterThan));

        var labelDialogStyle = UiUtil.MakeLabel(Se.Language.Options.Settings.DialogStyle);
        var comboBoxDialogStyle = new ComboBox
        {
            Width = 250,
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.DialogStyles)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.DialogStyle)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };

        var labelContinuationStyle = UiUtil.MakeLabel(Se.Language.Options.Settings.ContinuationStyle);
        var comboBoxContinuationStyle = new ComboBox
        {
            Width = 250,
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.ContinuationStyles)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.ContinuationStyle)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };

        var labelCpsLineLengthStyle = UiUtil.MakeLabel(Se.Language.Options.Settings.CpsLineLengthStyle);
        var comboBoxCpsLineLengthStyle = new ComboBox
        {
            Width = 250,
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.CpsLineLengthStrategies)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.CpsLineLengthStrategy)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };

        grid.Add(labelName, 0, 0);
        grid.Add(textBoxName, 0, 1);

        grid.Add(labelSingleLineMaxLength, 1, 0);
        grid.Add(numericUpDownSingleLineMaxLength, 1, 1);

        grid.Add(labelOptimalCharsPerSec, 2, 0);
        grid.Add(numericUpDownOptimalCharsPerSec, 2, 1);

        grid.Add(labelMaxCharsPerSec, 3, 0);
        grid.Add(numericUpDownMaxCharsPerSec, 3, 1);

        grid.Add(labelMaxWordsPerMin, 4, 0);
        grid.Add(numericUpDownMaxWordsPerMin, 4, 1);

        grid.Add(labelMinDurationMs, 5, 0);
        grid.Add(numericUpDownMinDurationMs, 5, 1);

        grid.Add(labelMaxDurationMs, 6, 0);
        grid.Add(numericUpDownMaxDurationMs, 6, 1);

        grid.Add(labelMinGapMs, 7, 0);
        grid.Add(numericUpDownMinGapMs, 7, 1);

        grid.Add(labelMaxLines, 8, 0);
        grid.Add(numericUpDownMaxLines, 8, 1);

        grid.Add(labelUnbreakLinesShorterThan, 9, 0);
        grid.Add(numericUpDownUnbreakLinesShorterThan, 9, 1);

        grid.Add(labelDialogStyle, 10, 0);
        grid.Add(comboBoxDialogStyle, 10, 1);

        grid.Add(labelContinuationStyle, 11, 0);
        grid.Add(comboBoxContinuationStyle, 11, 1);

        grid.Add(labelCpsLineLengthStyle, 12, 0);
        grid.Add(comboBoxCpsLineLengthStyle, 12, 1);

        return grid;
    }
}
