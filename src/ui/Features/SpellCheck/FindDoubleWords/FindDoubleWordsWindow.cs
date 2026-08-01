using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleWords;

public class FindDoubleWordsWindow : Window
{
    public FindDoubleWordsWindow(FindDoubleWordsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.DoubleWords;
        CanResize = true;
        Width = 600;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasDoubleWords));
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonGoTo, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
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

        grid.Add(MakeGridView(vm), 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work

        KeyDown += (s, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeGridView(FindDoubleWordsViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView();
        tableView.Height = double.NaN; // auto size inside scroll viewer
        tableView.Margin = new Thickness(2);
        tableView.ItemsSource = vm.Subtitles;
        tableView.DataContext = vm.Subtitles;

        tableView.DoubleTapped += vm.OnBookmarksGridDoubleTapped;

        // Columns (the DataGrid sized the number column to content; TableView treats
        // Auto as star, so it gets a fixed width instead)
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(DoubleWordItem.Number)),
            Width = new GridLength(60),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(DoubleWordItem.Text)),
            Width = new GridLength(1, GridUnitType.Star), // star sizing to take all available space
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.DoubleWords,
            Binding = new Binding(nameof(DoubleWordItem.Hit)),
            Width = new GridLength(1, GridUnitType.Star), // star sizing to take all available space
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        TableViewExtras.BindSelectedItem(tableView, vm, nameof(vm.SelectedSubtitle));
        tableView.DoubleTapped += (s, e) => vm.GoToCommand.Execute(null);
        tableView.KeyDown += (s, e) => vm.GridKeyDown(e);
        tableView.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && tableView.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                tableView.SelectedItem = target;
                if (target != null)
                {
                    tableView.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        return UiUtil.MakeBorderForControlNoPadding(tableView);
    }
}
