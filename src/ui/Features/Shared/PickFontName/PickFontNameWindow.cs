using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickFontName;

public class PickFontNameWindow : Window
{
    public PickFontNameWindow(PickFontNameViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.PickFontNameTitle;
        CanResize = true;
        Width = 800;
        Height = 700;
        MinWidth = 500;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var textBoxSearch = new TextBox
        {
            PlaceholderText = Se.Language.General.SearchFontNames,
            Margin = new Thickness(10),
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        textBoxSearch.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        textBoxSearch.TextChanged += (s, e) => vm.SearchTextChanged();
        var panelSearch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelSearch,
                textBoxSearch,
            }
        };

        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownOneDecimal(5, 1000, 200, vm, nameof(vm.FontSize));
        var panelFontSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelFontSize,
                numericUpDownFontSize,
            }
        }.WithBindVisible(vm, nameof(vm.IsFontSizeVisible));

        var labelFontBold = UiUtil.MakeLabel(Se.Language.General.Bold);
        var checkBoxFontBold = UiUtil.MakeCheckBox(string.Empty, vm, nameof(vm.IsFontBold));
        checkBoxFontBold.IsCheckedChanged += (s, e) => vm.FontBoldChanged();    
        var panelFontBold = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelFontBold,
                checkBoxFontBold,
            }
        }.WithBindVisible(vm, nameof(vm.IsFontBoldVisible));

        var tabControlFonts = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Items =
            {
                new TabItem
                {
                    Header = Se.Language.Tools.PickFontNameInstalledFonts,
                    Content = MakeFontsView(vm, vm.FontNames, nameof(vm.SelectedFontName)),
                },
                new TabItem
                {
                    Header = Se.Language.Tools.PickFontNameCollectedFonts,
                    Content = MakeFontsView(vm, vm.CollectedFontNames, nameof(vm.SelectedCollectedFontName)),
                },
            },
        };
        tabControlFonts.Bind(TabControl.SelectedIndexProperty, new Binding(nameof(vm.SelectedTabIndex)) { Source = vm, Mode = BindingMode.TwoWay });

        var previewView = MakePreviewView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
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

        grid.Add(panelSearch, 0);
        grid.Add(panelFontSize, 1);
        grid.Add(panelFontBold, 2);
        grid.Add(tabControlFonts, 3);
        grid.Add(previewView, 4);
        grid.Add(buttonPanel, 5);

        Content = grid;

        Activated += delegate { textBoxSearch.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeFontsView(PickFontNameViewModel vm, System.Collections.IEnumerable itemsSource, string selectedItemPath)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = itemsSource;

        var fontNameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.FontName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding("."),
            Width = new GridLength(1, GridUnitType.Star),
        };
        dataGrid.Columns.Add(fontNameColumn);

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(selectedItemPath));
        dataGrid.SelectionChanged += vm.FontNameGridSelectionChanged;

        // Font list order is presentation-only (OK uses the selected item), so the
        // in-place header sorter is safe. Note a new search resets the order.
        new TableViewHeaderSorter(dataGrid)
            .AddSortable<string, string>(fontNameColumn, x => x);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

    private static Border MakePreviewView(PickFontNameViewModel vm)
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

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        grid.Add(image, 0);

        return UiUtil.MakeBorderForControl(grid);
    }
}
