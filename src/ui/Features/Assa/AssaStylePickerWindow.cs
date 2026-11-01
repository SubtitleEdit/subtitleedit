using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// Pick styles to import/export/copy - styled like ASSA draw: the list with check all/none,
/// the accept button in the accent.
/// </summary>
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
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        // Header: title with check all / none
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, new RelayCommand(() => SetAll(vm, true)), 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-regular fa-square", Se.Language.General.SelectNone, new RelayCommand(() => SetAll(vm, false)), 24));
        var header = new DockPanel { Margin = new Thickness(14, 10, 10, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Styles));

        var list = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) } };
        list.Add(header, 0, 0);
        list.Add(MakeDataGrid(vm, out var stylesGrid), 1, 0);

        var mainGrid = new Grid { RowDefinitions = { new RowDefinition(new GridLength(1, GridUnitType.Star)), new RowDefinition(GridLength.Auto) } };
        mainGrid.Add(new Border { Child = list, Background = EditorChrome.PanelBrush }, 0, 0);
        mainGrid.Add(MakeButtonBar(vm), 1, 0);
        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(stylesGrid); });
        KeyDown += vm.KeyDown;
    }

    private static void SetAll(AssaStylePickerViewModel vm, bool isSelected)
    {
        foreach (var style in vm.Styles)
        {
            style.IsSelected = isSelected;
        }
    }

    private static Border MakeButtonBar(AssaStylePickerViewModel vm)
    {
        var buttonAccept = UiUtil.MakeButton(string.Empty, vm.OkCommand).WithBindContent(nameof(vm.ButtonAcceptText));
        buttonAccept.IsDefault = true; // the dialog's accept button - Enter runs it (#14586)
        buttonAccept.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonCancel));
        buttons.Children.Add(EditorChrome.WithoutMargin(buttonAccept));

        return EditorChrome.WrapButtonBar(buttons);
    }

    private static TableView MakeDataGrid(AssaStylePickerViewModel vm, out TableView tableView)
    {
        var usagesColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Usages,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(StyleDisplay.UsageCount)),
            Width = new GridLength(90),
        };
        usagesColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowUsageCount))
        {
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        var categoryColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Category,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(StyleDisplay.CategoryDisplay)),
            Width = new GridLength(140),
        };
        categoryColumn.Bind(SeTableViewColumn.IsVisibleProperty, new Binding(nameof(vm.ShowCategory))
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

        // The usages and category columns have a bound visibility, so all columns go through a
        // TableViewColumnManager (TableView itself has no column IsVisible).
        var columnManager = new TableViewColumnManager(dataGrid);
        columnManager.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<StyleDisplay>(nameof(StyleDisplay.IsSelected), focusable: true),
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
        columnManager.Add(categoryColumn);
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

        dataGrid.Margin = new Thickness(8, 0, 8, 8);
        AutomationProperties.SetName(dataGrid, Se.Language.General.Styles);
        return dataGrid;
    }
}
