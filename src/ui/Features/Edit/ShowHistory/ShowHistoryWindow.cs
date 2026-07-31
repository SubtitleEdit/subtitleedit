using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.ShowHistory;

public class ShowHistoryWindow : Window
{
    public ShowHistoryWindow(ShowHistoryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.ShowHistory;
        Width = 810;
        Height = 640;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        // TableView (Avalonia 12.1) pilot - DataGrid is in maintenance mode upstream and
        // TableView is the recommended control for read-only tabular data. This window is
        // the simplest grid in SE (read-only, no sorting, no cell themes), so it is used to
        // validate look and behaviour parity before considering wider adoption.
        var tableView = new TableView
        {
            SelectionMode = SelectionMode.Single,
            Margin = new Thickness(0, 10, 0, 0),
            [!TableView.ItemsSourceProperty] = new Binding(nameof(vm.HistoryItems)),
            [!TableView.SelectedItemProperty] = new Binding(nameof(vm.SelectedHistoryItem)),
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Columns =
            {
                new TableViewColumn
                {
                    Header = Se.Language.General.Time,
                    Binding = new Binding(nameof(ShowHistoryDisplayItem.Time)),
                    Width = GridLength.Auto,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                },
                new TableViewColumn
                {
                    Header = Se.Language.General.Description,
                    Binding = new Binding(nameof(ShowHistoryDisplayItem.Description)),
                    Width = new GridLength(3, GridUnitType.Star),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                },
            },
        };
        UiUtil.ApplyTableViewRowStyle(tableView);
        tableView.SelectionChanged += (sender, args) =>
        {
            vm.IsRollbackEnabled = tableView.SelectedItem != null;
        };

        var buttonRollback = UiUtil.MakeButton(Se.Language.Edit.RestoreSelected, vm.RollbackToCommand).WithBindEnabled(nameof(vm.IsRollbackEnabled));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonRollback,
            buttonCancel
        );

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

        grid.Add(tableView, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }
}
