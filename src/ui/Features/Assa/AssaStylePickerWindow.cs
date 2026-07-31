using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class AssaStylePickerWindow : Window
{
    public AssaStylePickerWindow(AssaStylePickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 800;
        Height = 550;
        MinWidth = 550;
        MinHeight = 400;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
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

        var labelFontsAndImages = UiUtil.MakeLabel(Se.Language.General.Styles);

        var buttonImport = UiUtil.MakeButton(string.Empty, vm.OkCommand).WithBindContent(nameof(vm.ButtonAcceptText));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonImport, buttonCancel);

        grid.Add(labelFontsAndImages, 0);
        grid.Add(MakeDataGrid(vm, out var stylesGrid), 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        Activated += delegate { TableViewExtras.FocusRow(stylesGrid); };
        KeyDown += vm.KeyDown;
    }

    private static Border MakeDataGrid(AssaStylePickerViewModel vm, out TableView tableView)
    {
        var usagesColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Usages,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(StyleDisplay.FontSize)),
            Width = new GridLength(90),
        };
        usagesColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowUsageCount))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        // No header sorting: the checked styles are imported/applied in list order
        // (e.g. appended to the file's style list, which is written to the header),
        // so the collection order is not presentation-only.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        tableView = dataGrid;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Styles;

        // The usages column has a bound visibility, so all columns go through a
        // TableViewColumnManager (TableView itself has no column IsVisible).
        var columnManager = new TableViewColumnManager(dataGrid);
        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<StyleDisplay>((item, _) =>
            new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(StyleDisplay.IsSelected)),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80),
        });
        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(StyleDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FontName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(StyleDisplay.FontName)),
            Width = new GridLength(180),
        });
        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FontSize,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(StyleDisplay.FontSize)),
            Width = new GridLength(90),
        });
        columnManager.Add(usagesColumn);

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedStyle)) { Source = vm });

        return UiUtil.MakeBorderForControl(dataGrid);
    }
}
