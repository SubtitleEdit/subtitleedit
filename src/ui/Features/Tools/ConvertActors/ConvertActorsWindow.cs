using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

public class ConvertActorsWindow : Window
{
    public ConvertActorsWindow(ConvertActorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ConvertActors.Title;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var labelFrom = UiUtil.MakeLabel(Se.Language.Tools.ConvertActors.ConvertActorFrom);
        var comboBoxFrom = UiUtil.MakeComboBox(vm.FromTypes, vm, nameof(vm.SelectedFromType));
        comboBoxFrom.SelectionChanged += vm.SelectionChanged;

        var labelTo = UiUtil.MakeLabel(Se.Language.Tools.ConvertActors.ConvertActorTo);
        var comboBoxTo = UiUtil.MakeComboBox(vm.ToTypes, vm, nameof(vm.SelectedToType));
        comboBoxTo.SelectionChanged += vm.SelectionChanged;

        var checkBoxSetColor = UiUtil.MakeCheckBox(Se.Language.Tools.ConvertActors.SetColor, vm, nameof(vm.SetColor));
        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.SelectedColor));
        colorPicker.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.SetColor)) { Source = vm, Mode = BindingMode.OneWay });

        var checkBoxChangeCasing = UiUtil.MakeCheckBox(Se.Language.General.ChangeCasing, vm, nameof(vm.ChangeCasing));
        var comboBoxCasing = new ComboBox
        {
            ItemsSource = vm.CasingOptions,
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        comboBoxCasing.Bind(ComboBox.SelectedIndexProperty, new Binding(nameof(vm.SelectedCasingIndex)) { Mode = BindingMode.TwoWay });
        comboBoxCasing.Bind(ComboBox.IsVisibleProperty, new Binding(nameof(vm.IsCasingVisible)) { Source = vm, Mode = BindingMode.OneWay });
        comboBoxCasing.SelectionChanged += vm.SelectionChanged;

        var checkBoxOnlyNames = UiUtil.MakeCheckBox(Se.Language.Tools.ConvertActors.OnlyNames, vm, nameof(vm.OnlyNames));

        var panelConversion = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
        };
        panelConversion.Add(labelFrom, 0, 0);
        panelConversion.Add(comboBoxFrom, 0, 1);
        panelConversion.Add(labelTo, 1, 0);
        panelConversion.Add(comboBoxTo, 1, 1);

        var panelOptions = UiUtil.MakeVerticalPanel(
            UiUtil.MakeHorizontalPanel(checkBoxSetColor, colorPicker),
            UiUtil.MakeHorizontalPanel(checkBoxChangeCasing, comboBoxCasing),
            checkBoxOnlyNames);

        var panelControls = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 24,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        panelControls.Add(panelConversion, 0, 0);
        panelControls.Add(panelOptions, 0, 1);

        var subtitleView = MakeSubtitleView(vm);

        var labelStatus = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        grid.Add(panelControls, 0);
        grid.Add(subtitleView, 1);
        grid.Add(labelStatus, 2);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeSubtitleView(ConvertActorsViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var colorConverter = new TextWithSubtitleSyntaxHighlightingConverter();

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
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = false,
                    Width = new DataGridLength(55),
                    CellTemplate = new FuncDataTemplate<ConvertActorsDisplayItem>((_, _) =>
                        new CheckBox
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            [!CheckBox.IsCheckedProperty] = new Binding(nameof(ConvertActorsDisplayItem.IsChecked)) { Mode = BindingMode.TwoWay },
                        }),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ConvertActorsDisplayItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ConvertActorsDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTemplate = new FuncDataTemplate<ConvertActorsDisplayItem>((_, _) =>
                    {
                        var border = new Border { Padding = new Thickness(4, 2) };
                        var textBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.NoWrap,
                            [!TextBlock.InlinesProperty] = new Binding(nameof(ConvertActorsDisplayItem.Text)) { Converter = colorConverter, Mode = BindingMode.OneWay },
                        };
                        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                        {
                            textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                        }
                        border.Child = textBlock;
                        return border;
                    }),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellTemplate = new FuncDataTemplate<ConvertActorsDisplayItem>((_, _) =>
                    {
                        var border = new Border { Padding = new Thickness(4, 2) };
                        var textBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.NoWrap,
                            [!TextBlock.InlinesProperty] = new Binding(nameof(ConvertActorsDisplayItem.NewText)) { Converter = colorConverter, Mode = BindingMode.OneWay },
                        };
                        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                        {
                            textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                        }
                        border.Child = textBlock;
                        return border;
                    }),
                },
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
