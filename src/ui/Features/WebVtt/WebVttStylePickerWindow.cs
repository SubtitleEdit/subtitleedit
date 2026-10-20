using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.WebVtt;

/// <summary>
/// Pick WebVTT styles to import/export - styled like ASSA draw: the list with select all /
/// invert, the selected style's CSS under it, the accept button in the accent.
/// </summary>
public class WebVttStylePickerWindow : Window
{
    private static readonly FontFamily MonoFont = new("Menlo, Consolas, Cascadia Mono, DejaVu Sans Mono, monospace");

    public WebVttStylePickerWindow(WebVttStylePickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        CanResize = true;
        Width = 640;
        Height = 540;
        MinWidth = 420;
        MinHeight = 320;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        // Header: title with select all / invert
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, vm.SelectAllCommand, 24));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.InvertSelectionCommand, 24));
        var header = new DockPanel { Margin = new Thickness(14, 10, 10, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Styles));

        var css = new Border
        {
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 6),
            MinHeight = 64,
            Margin = new Thickness(8, 0, 8, 10),
            Child = new TextBlock
            {
                FontFamily = MonoFont,
                FontSize = 11.5,
                TextWrapping = TextWrapping.Wrap,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.SelectedStyleCss)),
            },
        };

        var list = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        list.Add(header, 0, 0);
        list.Add(MakeStylesView(vm, out var stylesGrid), 1, 0);
        list.Add(css, 2, 0);

        var mainGrid = new Grid { RowDefinitions = { new RowDefinition(new GridLength(1, GridUnitType.Star)), new RowDefinition(GridLength.Auto) } };
        mainGrid.Add(new Border { Child = list, Background = EditorChrome.PanelBrush }, 0, 0);
        mainGrid.Add(MakeButtonBar(vm), 1, 0);
        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(stylesGrid); });
        KeyDown += vm.KeyDown;
    }

    private static Border MakeButtonBar(WebVttStylePickerViewModel vm)
    {
        var buttonOk = UiUtil.MakeButton(string.Empty, vm.OkCommand).WithBindContent(nameof(vm.ButtonAcceptText));
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        return EditorChrome.WrapButtonBar(buttons);
    }

    private static TableView MakeStylesView(WebVttStylePickerViewModel vm, out TableView tableView)
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
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<WebVttStyleDisplay>(nameof(WebVttStyleDisplay.IsSelected), focusable: true),
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

        dataGrid.Margin = new Thickness(8, 0, 8, 8);
        AutomationProperties.SetName(dataGrid, Se.Language.General.Styles);
        return dataGrid;
    }
}
