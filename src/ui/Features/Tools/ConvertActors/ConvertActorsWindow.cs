using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
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

        Activated += delegate { comboBoxFrom.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeSubtitleView(ConvertActorsViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var colorConverter = new TextWithSubtitleSyntaxHighlightingConverter();

        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // subtitle lines in timeline order.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Subtitles;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(55),
                    CellTemplate = new FuncDataTemplate<ConvertActorsDisplayItem>((_, _) =>
                        new CheckBox
                        {
                            Focusable = false,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            [!CheckBox.IsCheckedProperty] = new Binding(nameof(ConvertActorsDisplayItem.IsChecked)) { Mode = BindingMode.TwoWay },
                        }),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(ConvertActorsDisplayItem.Number)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(ConvertActorsDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(1, GridUnitType.Star),
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
                new SeTableViewColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(1, GridUnitType.Star),
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
        });

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        TableViewExtras.AddSpaceToggle<ConvertActorsDisplayItem>(dataGrid,
            item => item.IsChecked, (item, v) => item.IsChecked = v);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
