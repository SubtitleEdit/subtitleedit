using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleLines;

/// <summary>
/// Neighbouring lines with the same text, in the editor look (see <see cref="EditorChrome"/>):
/// the list, Done and Go to at the bottom.
/// </summary>
public class FindDoubleLinesWindow : Window
{
    public FindDoubleLinesWindow(FindDoubleLinesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.DoubleLines);
        CanResize = true;
        Width = 600;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasDoubleLines));
        buttonGoTo.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);

        var tableView = MakeGridView(vm);
        // A section header over the list, like the other list windows
        var list = EditorChrome.MakeListSection(Se.Language.General.DoubleLines, tableView, first: true);

        Content = EditorChrome.MakeRoot(list, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonGoTo));

        // The list, not an action button, gets first focus (hack to make OnKeyDown work) - a
        // focused button clicks on bare Space; Enter in the list goes to the line.
        UiUtil.FocusOnFirstActivation(this, tableView);

        KeyDown += (s, e) => vm.OnKeyDown(e);

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static TableView MakeGridView(FindDoubleLinesViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView();
        tableView.Height = double.NaN;
        tableView.ItemsSource = vm.Subtitles;
        tableView.DataContext = vm.Subtitles;
        AutomationProperties.SetName(tableView, Se.Language.General.DoubleLines);

        tableView.DoubleTapped += vm.OnBookmarksGridDoubleTapped;

        // Columns (the DataGrid sized the number column to content; TableView treats
        // Auto as star, so it gets a fixed width instead)
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(DoubleLineItem.Number)),
            Width = new GridLength(60),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(DoubleLineItem.Text)),
            Width = new GridLength(1, GridUnitType.Star),
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

        return tableView;
    }
}
