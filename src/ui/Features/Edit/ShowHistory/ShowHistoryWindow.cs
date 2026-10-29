using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.ShowHistory;

/// <summary>
/// Undo history, in the editor look (see <see cref="EditorChrome"/>): the history list, Cancel and
/// Restore selected at the bottom.
/// </summary>
public class ShowHistoryWindow : Window
{
    public ShowHistoryWindow(ShowHistoryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Edit.ShowHistory);
        Width = 810;
        Height = 640;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        // TableView (Avalonia 12.1) pilot - DataGrid is in maintenance mode upstream and
        // TableView is the recommended control for read-only tabular data. This window is
        // the simplest grid in SE (read-only, no sorting, no cell themes), so it is used to
        // validate look and behaviour parity before considering wider adoption.
        var tableView = new TableView
        {
            SelectionMode = SelectionMode.Single,
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
        AutomationProperties.SetName(tableView, Se.Language.Edit.ShowHistory);
        UiUtil.ApplyTableViewRowStyle(tableView);
        TableViewExtras.AttachListNavigation(tableView);
        tableView.SelectionChanged += (sender, args) =>
        {
            vm.IsRollbackEnabled = tableView.SelectedItem != null;
        };

        var buttonRollback = UiUtil.MakeButton(Se.Language.Edit.RestoreSelected, vm.RollbackToCommand).WithBindEnabled(nameof(vm.IsRollbackEnabled));
        buttonRollback.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var list = new Border
        {
            Child = EditorChrome.MakeListFrame(tableView, clearListBackground: true),
            Padding = new Thickness(14),
        };

        Content = EditorChrome.MakeRoot(list, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonRollback));

        // The list, not an action button, gets first focus (hack to make OnKeyDown work) - a
        // focused button clicks on bare Space.
        UiUtil.FocusOnFirstActivation(this, tableView);
        KeyDown += vm.OnKeyDown;
    }
}
