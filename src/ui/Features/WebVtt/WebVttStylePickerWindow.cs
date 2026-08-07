using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.WebVtt;

public class WebVttStylePickerWindow : Window
{
    public WebVttStylePickerWindow(WebVttStylePickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 600;
        Height = 500;
        MinWidth = 400;
        MinHeight = 300;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        var labelStyles = UiUtil.MakeLabel(Se.Language.General.Styles).WithBold();

        var labelCss = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SelectedStyleCss));
        labelCss.MinHeight = 60;
        labelCss.VerticalContentAlignment = VerticalAlignment.Top;

        var buttonOk = UiUtil.MakeButton(string.Empty, vm.OkCommand).WithBindContent(nameof(vm.ButtonAcceptText));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(labelStyles, 0);
        grid.Add(MakeStylesView(vm, out var stylesGrid), 1);
        grid.Add(UiUtil.MakeBorderForControl(labelCss), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        Activated += delegate { TableViewExtras.FocusRow(stylesGrid); };
        KeyDown += vm.KeyDown;
    }

    private static Border MakeStylesView(WebVttStylePickerViewModel vm, out TableView tableView)
    {
        // No header sorting: checked styles are applied/written in list order.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        tableView = dataGrid;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Styles;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<WebVttStyleDisplay>((_, _) =>
                new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(WebVttStyleDisplay.IsSelected)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                }),
            Width = new GridLength(80),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FontName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.FontNameDisplay)),
            Width = new GridLength(160),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.FontSize,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(WebVttStyleDisplay.FontSizeDisplay)),
            Width = new GridLength(90),
        });

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedStyle)) { Source = vm });
        TableViewExtras.AttachListNavigation(dataGrid);

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem
        {
            Header = Se.Language.General.SelectAll,
            DataContext = vm,
            Command = vm.SelectAllCommand,
        });
        flyout.Items.Add(new MenuItem
        {
            Header = Se.Language.General.InvertSelection,
            DataContext = vm,
            Command = vm.InvertSelectionCommand,
        });
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        return UiUtil.MakeBorderForControl(dataGrid);
    }
}
