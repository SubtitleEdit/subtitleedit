using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickFontName;

/// <summary>
/// Pick a font name, in the shared picker layout (see <see cref="EditorChrome"/>): the font
/// lists with the search box in the header, the preview below, Cancel and OK at the bottom.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var textBoxSearch = EditorChrome.MakeSearchBox(vm, nameof(vm.SearchText), Se.Language.General.SearchFontNames);
        textBoxSearch.TextChanged += (s, e) => vm.SearchTextChanged();

        var numericUpDownFontSize = UiUtil.MakeNumericUpDownOneDecimal(5, 1000, 130, vm, nameof(vm.FontSize));
        Avalonia.Automation.AutomationProperties.SetName(numericUpDownFontSize, Se.Language.General.FontSize);
        var panelFontSize = EditorChrome.MakeLabeledRow(Se.Language.General.FontSize, numericUpDownFontSize, 90)
            .WithBindIsVisible(nameof(vm.IsFontSizeVisible));

        var switchFontBold = EditorChrome.MakeSwitch(nameof(vm.IsFontBold), Se.Language.General.Bold);
        switchFontBold.IsCheckedChanged += (s, e) => vm.FontBoldChanged();
        var panelFontBold = EditorChrome.MakeSwitchRow(switchFontBold, Se.Language.General.Bold)
            .WithBindIsVisible(nameof(vm.IsFontBoldVisible));

        var panelOptions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24,
            Margin = new Thickness(0, 0, 0, 8),
            Children = { panelFontSize, panelFontBold },
        };

        var tabControlFonts = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(0, 6, 0, 0),
            Items =
            {
                new TabItem
                {
                    Header = Se.Language.Tools.PickFontNameInstalledFonts,
                    Content = MakeFontsView(vm, vm.FontNames, nameof(vm.SelectedFontName), Se.Language.Tools.PickFontNameInstalledFonts),
                },
                new TabItem
                {
                    Header = Se.Language.Tools.PickFontNameCollectedFonts,
                    Content = MakeFontsView(vm, vm.CollectedFontNames, nameof(vm.SelectedCollectedFontName), Se.Language.Tools.PickFontNameCollectedFonts),
                },
            },
        };
        tabControlFonts.Bind(TabControl.SelectedIndexProperty, new Binding(nameof(vm.SelectedTabIndex)) { Source = vm, Mode = BindingMode.TwoWay });

        var header = new DockPanel { MinHeight = 28, Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(textBoxSearch, Dock.Right);
        header.Children.Add(textBoxSearch);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Fonts));

        var fontsSection = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        fontsSection.Add(header, 0);
        fontsSection.Add(panelOptions, 1);
        fontsSection.Add(tabControlFonts, 2);

        var previewSection = MakePreviewView(vm);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(fontsSection, 0);
        sections.Add(previewSection, 1);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBoxSearch); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeFontsView(PickFontNameViewModel vm, System.Collections.IEnumerable itemsSource, string selectedItemPath, string accessibleName)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.WithAccessibleName(accessibleName);
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

        return EditorChrome.MakeListFrame(dataGrid, clearListBackground: true);
    }

    private static Border MakePreviewView(PickFontNameViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        var frame = new Border
        {
            Child = image,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8),
            ClipToBounds = true,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        var header = EditorChrome.MakeSectionHeader(Se.Language.General.Preview);
        header.Margin = new Thickness(0, 0, 0, 8);
        grid.Add(header, 0);
        grid.Add(frame, 1);

        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }
}
