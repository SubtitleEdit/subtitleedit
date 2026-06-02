using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;

public class MergeTwoSubtitlesWindow : Window
{
    public MergeTwoSubtitlesWindow(MergeTwoSubtitlesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.MergeTwoSubtitles.Title;
        CanResize = true;
        Width = 1200;
        Height = 800;
        MinWidth = 950;
        MinHeight = 600;
        vm.Window = this;
        DataContext = vm;

        var listsView = MakeListsView(vm);
        var formatRow = MakeFormatRow(vm);
        var stylesView = MakeStylesView(vm);
        var previewView = MakePreviewView(vm);

        var buttonOk = UiUtil.MakeButton(Se.Language.Tools.MergeTwoSubtitles.Merge, vm.OkCommand)
            .WithBindEnabled(nameof(vm.IsMergeEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var bottomGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        bottomGrid.Add(stylesView, 0, 0);
        bottomGrid.Add(previewView, 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // lists
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // format combo
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // styles + preview
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(listsView, 0, 0);
        grid.Add(formatRow, 1, 0);
        grid.Add(bottomGrid, 2, 0);
        grid.Add(buttonPanel, 3, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }

    private static Border MakeListsView(MergeTwoSubtitlesViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        grid.Add(MakeOneListView(vm,
            Se.Language.Tools.MergeTwoSubtitles.Subtitle1,
            nameof(vm.Items1),
            nameof(vm.SelectedItem1),
            nameof(vm.File1Display),
            vm.LoadFile1Command), 0, 0);

        grid.Add(MakeOneListView(vm,
            Se.Language.Tools.MergeTwoSubtitles.Subtitle2,
            nameof(vm.Items2),
            nameof(vm.SelectedItem2),
            nameof(vm.File2Display),
            vm.LoadFile2Command), 0, 1);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }

    private static Border MakeOneListView(MergeTwoSubtitlesViewModel vm,
        string title,
        string itemsPath,
        string selectedItemPath,
        string fileNamePath,
        CommunityToolkit.Mvvm.Input.IRelayCommand loadCommand)
    {
        var labelTitle = UiUtil.MakeLabel(title);
        var labelFile = UiUtil.MakeLabel(string.Empty).WithBindText(vm, fileNamePath);
        var buttonLoad = UiUtil.MakeButton(Se.Language.Tools.MergeTwoSubtitles.LoadFromFile, loadCommand);

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();

        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = null,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.Number)),
                    IsReadOnly = true,
                    Width = new DataGridLength(60, DataGridLengthUnitType.Pixel),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.StartTime,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.StartTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                    Width = new DataGridLength(120, DataGridLengthUnitType.Pixel),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.EndTime,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.EndTime)) { Converter = fullTimeConverter },
                    IsReadOnly = true,
                    Width = new DataGridLength(120, DataGridLengthUnitType.Pixel),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(MergeTwoSubtitlesDisplayItem.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(itemsPath) { Source = vm });
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(selectedItemPath) { Source = vm });
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGrid.SelectedItem = target;
                dataGrid.ScrollIntoView(target, null);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                labelTitle,
                labelFile,
            },
        };

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
            RowSpacing = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        grid.Add(headerPanel, 0, 0);
        grid.Add(dataGrid, 1, 0);
        grid.Add(buttonLoad, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static StackPanel MakeFormatRow(MergeTwoSubtitlesViewModel vm)
    {
        var label = UiUtil.MakeLabel(Se.Language.Tools.MergeTwoSubtitles.OutputFormat);
        var combo = UiUtil.MakeComboBox(vm.OutputFormats, vm, nameof(vm.SelectedOutputFormat))
            .WithMinWidth(260);

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                label,
                combo,
            },
        };
    }

    private static Border MakeStylesView(MergeTwoSubtitlesViewModel vm)
    {
        var l = Se.Language.Tools.MergeTwoSubtitles;

        var col1 = MakeStyleColumn(vm, l.Style1,
            nameof(vm.FontName1), nameof(vm.FontSize1),
            nameof(vm.Bold1), nameof(vm.Italic1),
            nameof(vm.PrimaryColor1), nameof(vm.OutlineColor1),
            nameof(vm.OutlineWidth1), nameof(vm.ShadowWidth1),
            nameof(vm.AlignTop1), "AlignGroup1");

        var col2 = MakeStyleColumn(vm, l.Style2,
            nameof(vm.FontName2), nameof(vm.FontSize2),
            nameof(vm.Bold2), nameof(vm.Italic2),
            nameof(vm.PrimaryColor2), nameof(vm.OutlineColor2),
            nameof(vm.OutlineWidth2), nameof(vm.ShadowWidth2),
            nameof(vm.AlignTop2), "AlignGroup2");

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        grid.Add(col1, 0, 0);
        grid.Add(col2, 0, 1);

        var border = UiUtil.MakeBorderForControl(grid);
        border.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsAssaSelected)) { Source = vm });
        return border;
    }

    private static Grid MakeStyleColumn(MergeTwoSubtitlesViewModel vm,
        string title,
        string fontNamePath, string fontSizePath,
        string boldPath, string italicPath,
        string primaryColorPath, string outlineColorPath,
        string outlineWidthPath, string shadowWidthPath,
        string alignTopPath, string alignGroupName)
    {
        var l = Se.Language.Tools.MergeTwoSubtitles;

        var labelTitle = UiUtil.MakeLabel(title);
        labelTitle.FontWeight = FontWeight.Bold;

        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboFontName = UiUtil.MakeComboBox(vm.FontNames, vm, fontNamePath).WithMinWidth(180);

        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var numFontSize = UiUtil.MakeNumericUpDownInt(6, 200, 20, 140, vm, fontSizePath);

        var checkBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, boldPath);
        var checkItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, italicPath);
        var stylePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { checkBold, checkItalic },
        };

        var labelPrimary = UiUtil.MakeLabel(Se.Language.General.TextColor);
        var primaryPicker = UiUtil.MakeColorPickerButton(vm, primaryColorPath, false);

        var labelOutline = UiUtil.MakeLabel(Se.Language.General.OutlineColor);
        var outlinePicker = UiUtil.MakeColorPickerButton(vm, outlineColorPath, false);

        var labelOutlineWidth = UiUtil.MakeLabel(l.OutlineWidth);
        var numOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 20, 140, vm, outlineWidthPath);

        var labelShadowWidth = UiUtil.MakeLabel(l.ShadowWidth);
        var numShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 20, 140, vm, shadowWidthPath);

        var radioTop = UiUtil.MakeRadioButton(l.AlignTop, vm, alignTopPath, alignGroupName);
        var radioBottom = new RadioButton
        {
            Content = l.AlignBottom,
            GroupName = alignGroupName,
            DataContext = vm,
        };
        radioBottom.Bind(RadioButton.IsCheckedProperty, new Binding(alignTopPath)
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
            Converter = new InverseBooleanConverter(),
        });
        var alignPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { radioTop, radioBottom },
        };

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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 6,
        };

        grid.Add(labelTitle, 0, 0, 1, 2);
        grid.Add(labelFontName, 1, 0); grid.Add(comboFontName, 1, 1);
        grid.Add(labelFontSize, 2, 0); grid.Add(numFontSize, 2, 1);
        grid.Add(stylePanel, 3, 1);
        grid.Add(labelPrimary, 4, 0); grid.Add(primaryPicker, 4, 1);
        grid.Add(labelOutline, 5, 0); grid.Add(outlinePicker, 5, 1);
        grid.Add(labelOutlineWidth, 6, 0); grid.Add(numOutlineWidth, 6, 1);
        grid.Add(labelShadowWidth, 7, 0); grid.Add(numShadowWidth, 7, 1);
        grid.Add(alignPanel, 8, 1);

        return grid;
    }

    private static Border MakePreviewView(MergeTwoSubtitlesViewModel vm)
    {
        var labelPreview = UiUtil.MakeLabel(Se.Language.General.Preview);

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.None,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowSpacing = 5,
        };
        grid.Add(labelPreview, 0, 0);
        grid.Add(image, 1, 0);

        var border = UiUtil.MakeBorderForControl(grid);
        border.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsAssaSelected)) { Source = vm });
        return border;
    }
}
