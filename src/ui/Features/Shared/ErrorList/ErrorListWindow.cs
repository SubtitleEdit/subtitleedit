using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

/// <summary>
/// "List errors": a verdict header, one summary card per error class (click to
/// filter), and the affected lines in a table. Double-click / Enter / "Go to"
/// jumps to the line in the main grid.
/// </summary>
public class ErrorListWindow : Window
{
    public ErrorListWindow(ErrorListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.ErrorList.Title;
        CanResize = true;
        Width = 960;
        Height = 680;
        MinWidth = 720;
        MinHeight = 460;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.ErrorList;

        var labelTitle = new TextBlock
        {
            Text = l.Title,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };
        var labelSummary = new TextBlock
        {
            FontSize = 14,
            Opacity = 0.75,
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Summary)),
        };
        var header = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 12),
            Children = { labelTitle, labelSummary },
        };

        var cards = SummaryCard.MakeCardsPanel(vm.Cards, vm.SetFilterCommand);

        var labelTip = new TextBlock
        {
            Text = l.Tip,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
            Margin = new Thickness(0, 10, 0, 10),
        };

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasErrors));
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonGoTo, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
        };

        grid.Add(header, 0, 0);
        grid.Add(cards, 1, 0);
        grid.Add(MakeErrorsGridView(vm), 2, 0);
        grid.Add(labelTip, 3, 0);
        grid.Add(panelButtons, 4, 0);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work

        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static Border MakeErrorsGridView(ErrorListViewModel vm)
    {
        var l = Se.Language.ErrorList;
        var dataGrid = TableViewExtras.MakeTableView(alwaysSelected: false, multiSelect: false);
        dataGrid.Height = double.NaN; // auto size inside scroll viewer
        dataGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Subtitles));

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ErrorListItem.Number)),
            Width = new GridLength(60),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Error,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<ErrorListItem>((_, _) =>
            {
                var dot = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 9,
                    Height = 9,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!Avalonia.Controls.Shapes.Shape.FillProperty] = new Binding(nameof(ErrorListItem.Brush)),
                };
                var text = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(ErrorListItem.Category)),
                };
                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Margin = new Thickness(6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { dot, text },
                };
            }),
            Width = new GridLength(160),
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(ErrorListItem.Show)),
            Width = new GridLength(105),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(ErrorListItem.Hide)),
            Width = new GridLength(105),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = l.Detail,
            Binding = new Binding(nameof(ErrorListItem.Detail)),
            Width = new GridLength(200),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(ErrorListItem.Text)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
        });
        AutomationProperties.SetName(dataGrid, l.Title);

        TableViewExtras.BindSelectedItem(dataGrid, vm, nameof(vm.SelectedSubtitle));
        dataGrid.DoubleTapped += vm.OnGridDoubleTapped;
        dataGrid.KeyDown += (s, e) => vm.GridKeyDown(e);
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGrid.SelectedItem = target;
                if (target != null)
                {
                    dataGrid.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
